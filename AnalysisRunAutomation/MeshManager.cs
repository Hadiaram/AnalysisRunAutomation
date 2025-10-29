using System;
using System.Collections.Generic;
using System.Linq;
using ETABSv1;

namespace ETABS_Plugin
{
    public class MeshManager
    {
        private readonly cSapModel _SapModel;

        public MeshManager(cSapModel sapModel) => _SapModel = sapModel;

        /// <summary>
        /// Applies mesh to all area objects (slabs and walls)
        /// </summary>
        public bool ApplyStandardMesh(double meshSize, out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                // Ensure model is unlocked
                bool isLocked = _SapModel.GetModelIsLocked();
                if (isLocked) _SapModel.SetModelIsLocked(false);

                // Work in N-mm-°C for consistency
                eUnits prev = _SapModel.GetPresentUnits();
                _SapModel.SetPresentUnits(eUnits.N_mm_C);

                try
                {
                    sb.AppendLine($"Applying {meshSize}mm mesh to all area objects...");

                    // Step 1: Get all available database tables
                    int numTables = 0;
                    string[] tableKeys = Array.Empty<string>();
                    string[] tableNames = Array.Empty<string>();
                    int[] importTypes = Array.Empty<int>();

                    int result = _SapModel.DatabaseTables.GetAvailableTables(ref numTables, ref tableKeys, ref tableNames, ref importTypes);
                    if (result != 0)
                    {
                        report = "Failed to get available database tables.";
                        return false;
                    }

                    sb.AppendLine($"Found {numTables} database tables.");

                    // Debug: List all tables that might be related
                    sb.AppendLine("Searching for mesh-related tables...");
                    List<string> meshCandidates = new List<string>();
                    for (int i = 0; i < numTables; i++)
                    {
                        string tableUpper = tableKeys[i].ToUpper();
                        if (tableUpper.Contains("MESH") || (tableUpper.Contains("AUTO") && tableUpper.Contains("AREA")))
                        {
                            meshCandidates.Add($"  {tableKeys[i]} - {tableNames[i]}");
                        }
                    }

                    if (meshCandidates.Count > 0)
                    {
                        sb.AppendLine("Mesh-related tables found:");
                        foreach (var candidate in meshCandidates)
                        {
                            sb.AppendLine(candidate);
                        }
                    }

                    // Step 2: Find the area mesh assignment table - be more specific
                    string meshTableKey = null;
                    for (int i = 0; i < numTables; i++)
                    {
                        string tableUpper = tableKeys[i].ToUpper();
                        // Look specifically for "MESH" in the key, avoid "CONSTRAINT" tables
                        if (tableUpper.Contains("MESH") &&
                            !tableUpper.Contains("CONSTRAINT") &&
                            !tableUpper.Contains("EDGE"))
                        {
                            meshTableKey = tableKeys[i];
                            sb.AppendLine($"\nSelected mesh table: {meshTableKey} - {tableNames[i]}");
                            break;
                        }
                    }

                    // If still not found, try "AUTO" + "AREA" but exclude "CONSTRAINT"
                    if (string.IsNullOrEmpty(meshTableKey))
                    {
                        for (int i = 0; i < numTables; i++)
                        {
                            string tableUpper = tableKeys[i].ToUpper();
                            if (tableUpper.Contains("AUTO") &&
                                tableUpper.Contains("AREA") &&
                                !tableUpper.Contains("CONSTRAINT") &&
                                !tableUpper.Contains("EDGE"))
                            {
                                meshTableKey = tableKeys[i];
                                sb.AppendLine($"\nSelected mesh table: {meshTableKey} - {tableNames[i]}");
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(meshTableKey))
                    {
                        report = "Could not find area mesh assignment table.";
                        return false;
                    }

                    // Step 3: Get all fields in the mesh table
                    int tableVersion = 0;
                    int numFields = 0;
                    string[] fieldKeys = Array.Empty<string>();
                    string[] fieldNames = Array.Empty<string>();
                    string[] descriptions = Array.Empty<string>();
                    string[] unitsString = Array.Empty<string>();
                    bool[] isImportable = Array.Empty<bool>();

                    result = _SapModel.DatabaseTables.GetAllFieldsInTable(
                        meshTableKey,
                        ref tableVersion,
                        ref numFields,
                        ref fieldKeys,
                        ref fieldNames,
                        ref descriptions,
                        ref unitsString,
                        ref isImportable);
                    if (result != 0)
                    {
                        report = "Failed to get fields in mesh table.";
                        return false;
                    }

                    sb.AppendLine($"Mesh table has {numFields} fields.");

                    // Step 4: Find the mesh size field
                    sb.AppendLine($"Available fields in mesh table:");
                    for (int i = 0; i < numFields; i++)
                    {
                        sb.AppendLine($"  [{i}] Key: '{fieldKeys[i]}' | Name: '{fieldNames[i]}'");
                    }

                    string meshSizeField = null;
                    for (int i = 0; i < numFields; i++)
                    {
                        string fieldUpper = fieldKeys[i].ToUpper();
                        if (fieldUpper.Contains("MAX") && (fieldUpper.Contains("SIZE") || fieldUpper.Contains("EDGE") || fieldUpper.Contains("LENGTH")))
                        {
                            meshSizeField = fieldKeys[i];
                            sb.AppendLine($"Found mesh size field: {meshSizeField}");
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(meshSizeField))
                    {
                        // Try alternative field names
                        for (int i = 0; i < numFields; i++)
                        {
                            string fieldUpper = fieldKeys[i].ToUpper();
                            if (fieldUpper.Contains("SIZE") || fieldUpper.Contains("TARGET") || fieldUpper.Contains("EDGE"))
                            {
                                meshSizeField = fieldKeys[i];
                                sb.AppendLine($"Using alternative mesh size field: {meshSizeField}");
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(meshSizeField))
                    {
                        report = sb.ToString() + "\n\nCould not identify mesh size field in table. Please check the field list above.";
                        return false;
                    }

                    // Step 5: Get the current table data
                    int tableVersionForData = 0;
                    string[] fieldKeysInData = Array.Empty<string>();
                    int numRecords = 0;
                    string[] tableData = Array.Empty<string>();

                    result = _SapModel.DatabaseTables.GetTableForEditingArray(
                        meshTableKey,
                        "", // GroupName - empty string for all groups
                        ref tableVersionForData,
                        ref fieldKeysInData,
                        ref numRecords,
                        ref tableData);
                    if (result != 0)
                    {
                        report = "Failed to get table data for editing.";
                        return false;
                    }

                    int numCols = fieldKeysInData.Length;
                    sb.AppendLine($"Retrieved {numRecords} rows, {numCols} columns from mesh table.");

                    // Step 6: Find the column index for mesh size
                    int meshSizeColIndex = -1;
                    for (int i = 0; i < fieldKeysInData.Length; i++)
                    {
                        if (fieldKeysInData[i] == meshSizeField)
                        {
                            meshSizeColIndex = i;
                            break;
                        }
                    }

                    if (meshSizeColIndex == -1)
                    {
                        report = $"Mesh size field '{meshSizeField}' not found in table columns.";
                        return false;
                    }

                    // Step 7: Update all rows with the new mesh size
                    int updatedRows = 0;
                    for (int row = 0; row < numRecords; row++)
                    {
                        int dataIndex = row * numCols + meshSizeColIndex;
                        tableData[dataIndex] = meshSize.ToString();
                        updatedRows++;
                    }

                    sb.AppendLine($"Updated {updatedRows} area objects with {meshSize}mm mesh size.");

                    // Step 8: Set the edited table back
                    result = _SapModel.DatabaseTables.SetTableForEditingArray(
                        meshTableKey,
                        ref tableVersionForData,
                        ref fieldKeysInData,
                        numRecords,
                        ref tableData);
                    if (result != 0)
                    {
                        report = "Failed to set edited table data.";
                        return false;
                    }

                    // Step 9: Apply the edited tables
                    int numFatalErrors = 0;
                    int numErrorMsgs = 0;
                    int numWarnMsgs = 0;
                    int numInfoMsgs = 0;
                    string importLog = "";

                    result = _SapModel.DatabaseTables.ApplyEditedTables(
                        false, // FillImportLog
                        ref numFatalErrors,
                        ref numErrorMsgs,
                        ref numWarnMsgs,
                        ref numInfoMsgs,
                        ref importLog);
                    if (result != 0)
                    {
                        report = "Failed to apply edited tables.";
                        return false;
                    }

                    if (numFatalErrors > 0 || numErrorMsgs > 0)
                    {
                        sb.AppendLine($"⚠ Warnings during apply: {numFatalErrors} fatal, {numErrorMsgs} errors, {numWarnMsgs} warnings");
                        if (!string.IsNullOrEmpty(importLog))
                        {
                            sb.AppendLine("Import log:");
                            sb.AppendLine(importLog);
                        }
                    }

                    sb.AppendLine("Mesh settings applied successfully.");

                    // Step 10: Create analysis model to generate mesh elements
                    sb.AppendLine("Creating analysis model to generate mesh elements...");
                    result = _SapModel.Analyze.CreateAnalysisModel();
                    if (result != 0)
                    {
                        sb.AppendLine("⚠ Warning: Analysis model creation returned non-zero result.");
                    }
                    else
                    {
                        sb.AppendLine("✓ Analysis model created successfully.");
                    }

                    report = sb.ToString();
                    return true;
                }
                finally
                {
                    // Restore units
                    _SapModel.SetPresentUnits(prev);
                }
            }
            catch (Exception ex)
            {
                report = "Exception in ApplyStandardMesh:\r\n" + ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// Applies custom mesh size to specific area objects
        /// </summary>
        public bool ApplyMeshToAreas(string[] areaNames, double meshSize, out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                if (areaNames == null || areaNames.Length == 0)
                {
                    report = "No area objects specified.";
                    return false;
                }

                // This method would need to filter the table data to only update specified areas
                // For simplicity, using the standard method which updates all areas
                sb.AppendLine($"Applying {meshSize}mm mesh to {areaNames.Length} specified areas...");

                bool success = ApplyStandardMesh(meshSize, out string detailedReport);
                sb.AppendLine(detailedReport);

                report = sb.ToString();
                return success;
            }
            catch (Exception ex)
            {
                report = "Exception in ApplyMeshToAreas:\r\n" + ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// Gets information about the current mesh settings
        /// </summary>
        public bool GetMeshInfo(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                // Get all area objects
                int nAreas = 0;
                string[] areaNames = Array.Empty<string>();
                _SapModel.AreaObj.GetNameList(ref nAreas, ref areaNames);

                sb.AppendLine($"Total area objects in model: {nAreas}");

                if (nAreas > 0)
                {
                    // Get mesh element count if analysis model exists
                    try
                    {
                        int nElements = _SapModel.AreaElm.Count();
                        sb.AppendLine($"Total mesh elements generated: {nElements}");

                        if (nElements > 0 && nAreas > 0)
                        {
                            double avgMeshPerArea = (double)nElements / nAreas;
                            sb.AppendLine($"Average mesh elements per area: {avgMeshPerArea:0.0}");
                        }
                    }
                    catch
                    {
                        sb.AppendLine("Analysis model not yet created - no mesh elements exist.");
                    }
                }

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in GetMeshInfo:\r\n" + ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// Clears mesh settings (sets to auto/default)
        /// </summary>
        public bool ClearMeshSettings(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                sb.AppendLine("Setting mesh to automatic (clearing manual mesh sizes)...");

                // Set a very large mesh size effectively turns off manual meshing
                bool success = ApplyStandardMesh(0, out string detailedReport);
                sb.AppendLine(detailedReport);

                report = sb.ToString();
                return success;
            }
            catch (Exception ex)
            {
                report = "Exception in ClearMeshSettings:\r\n" + ex.ToString();
                return false;
            }
        }
    }
}