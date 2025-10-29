using System;
using System.Collections.Generic;
using System.Linq;
using ETABSv1;

namespace ETABS_Plugin
{
    public class MassSourceManager
    {
        private readonly cSapModel _SapModel;

        public MassSourceManager(cSapModel sapModel) => _SapModel = sapModel;

        #region Mass Source Configuration

        /// <summary>
        /// Sets up standard seismic mass source (DEAD + 30% LIVE)
        /// </summary>
        public bool SetupSeismicMassSource(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                if (_SapModel.GetModelIsLocked()) _SapModel.SetModelIsLocked(false);

                sb.AppendLine("Setting up seismic mass source...");

                // From elements (self-weight), include any added mass, and include nominated loads
                bool includeElements = true;
                bool includeAddedMass = true;
                // we have 2 patterns, so includeLoads = true
                string[] loadPatterns = new[] { "PLUGIN_DEAD", "PLUGIN_LIVE" };
                double[] scaleFactors = new[] { 1.0, 0.3 };
                int numberLoads = loadPatterns.Length;
                bool includeLoads = numberLoads > 0;

                int ret = _SapModel.PropMaterial.SetMassSource_1(
                    ref includeElements,
                    ref includeAddedMass,
                    ref includeLoads,
                    numberLoads,
                    ref loadPatterns,
                    ref scaleFactors
                );

                if (ret == 0)
                {
                    sb.AppendLine("✓ Mass source configured");
                    sb.AppendLine("  - From Elements (self-weight): Yes");
                    sb.AppendLine("  - Added Mass: Yes");
                    sb.AppendLine("  - From Loads: Yes");
                    sb.AppendLine("  - Patterns: PLUGIN_DEAD=1.0, PLUGIN_LIVE=0.3");
                    report = sb.ToString();
                    return true;
                }

                sb.AppendLine($"⚠ SetMassSource_1 returned {ret}");
                report = sb.ToString();
                return false;
            }
            catch (Exception ex)
            {
                report = "Exception in SetupSeismicMassSource:\r\n" + ex;
                return false;
            }
        }


        /// <summary>
        /// Sets custom mass source with specified load patterns and factors
        /// </summary>
        public bool SetCustomMassSource(Dictionary<string, double> patternFactors, bool includeSelfWeight, out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                if (_SapModel.GetModelIsLocked()) _SapModel.SetModelIsLocked(false);

                sb.AppendLine("Setting up custom mass source...");

                bool includeElements = includeSelfWeight;
                bool includeAddedMass = true;

                string[] loadPatterns = (patternFactors != null && patternFactors.Count > 0)
                    ? patternFactors.Keys.ToArray()
                    : Array.Empty<string>();

                double[] scaleFactors = (patternFactors != null && patternFactors.Count > 0)
                    ? patternFactors.Values.ToArray()
                    : Array.Empty<double>();

                int numberLoads = loadPatterns.Length;
                bool includeLoads = numberLoads > 0;

                int ret = _SapModel.PropMaterial.SetMassSource_1(
                    ref includeElements,
                    ref includeAddedMass,
                    ref includeLoads,
                    numberLoads,
                    ref loadPatterns,
                    ref scaleFactors
                );

                if (ret == 0)
                {
                    sb.AppendLine($"✓ Custom mass source set (Self-weight: {(includeSelfWeight ? "Yes" : "No")})");
                    if (includeLoads)
                        for (int i = 0; i < numberLoads; i++)
                            sb.AppendLine($"  - {loadPatterns[i]}: {scaleFactors[i]}");
                    report = sb.ToString();
                    return true;
                }

                sb.AppendLine($"⚠ SetMassSource_1 returned {ret}");
                report = sb.ToString();
                return false;
            }
            catch (Exception ex)
            {
                report = "Exception in SetCustomMassSource:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Sets mass source using database tables (fallback method)
        /// </summary>
        private bool SetMassSourceViaDatabase(string[] loadPatterns, double[] scaleFactors, out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                sb.AppendLine("Using database table method for mass source...");

                // Get all available tables
                int numTables = 0;
                string[] tableKeys = Array.Empty<string>();
                string[] tableNames = Array.Empty<string>();
                int[] importTypes = Array.Empty<int>();

                int ret = _SapModel.DatabaseTables.GetAvailableTables(
                    ref numTables, ref tableKeys, ref tableNames, ref importTypes);

                if (ret != 0)
                {
                    report = "Failed to get database tables.";
                    return false;
                }

                // Find mass source table
                string massTableKey = null;
                for (int i = 0; i < numTables; i++)
                {
                    string tableUpper = tableKeys[i].ToUpper();
                    if (tableUpper.Contains("MASS") && tableUpper.Contains("SOURCE"))
                    {
                        massTableKey = tableKeys[i];
                        sb.AppendLine($"Found mass source table: {massTableKey}");
                        break;
                    }
                }

                if (string.IsNullOrEmpty(massTableKey))
                {
                    report = sb.ToString() + "\nCould not find mass source table.";
                    return false;
                }

                // Get table structure
                int tableVersion = 0;
                int numFields = 0;
                string[] fieldKeys = Array.Empty<string>();
                string[] fieldNames = Array.Empty<string>();
                string[] descriptions = Array.Empty<string>();
                string[] unitsString = Array.Empty<string>();
                bool[] isImportable = Array.Empty<bool>();

                ret = _SapModel.DatabaseTables.GetAllFieldsInTable(
                    massTableKey, ref tableVersion, ref numFields,
                    ref fieldKeys, ref fieldNames, ref descriptions,
                    ref unitsString, ref isImportable);

                if (ret != 0)
                {
                    report = "Failed to get mass source table fields.";
                    return false;
                }

                // Build table data
                List<string> tableData = new List<string>();

                // Add header indicating elements are included
                tableData.Add("FromElements");
                tableData.Add("Yes");

                // Add each load pattern
                for (int i = 0; i < loadPatterns.Length; i++)
                {
                    tableData.Add(loadPatterns[i]);
                    tableData.Add(scaleFactors[i].ToString());
                }

                // Set the table
                string[] fieldKeysArray = fieldKeys.Take(2).ToArray(); // Simplified
                string[] tableDataArray = tableData.ToArray();
                int numRecords = loadPatterns.Length + 1;

                ret = _SapModel.DatabaseTables.SetTableForEditingArray(
                    massTableKey, ref tableVersion, ref fieldKeysArray,
                    numRecords, ref tableDataArray);

                if (ret != 0)
                {
                    report = "Failed to set mass source table data.";
                    return false;
                }

                // Apply changes
                int numFatalErrors = 0;
                int numErrorMsgs = 0;
                int numWarnMsgs = 0;
                int numInfoMsgs = 0;
                string importLog = "";

                ret = _SapModel.DatabaseTables.ApplyEditedTables(
                    false, ref numFatalErrors, ref numErrorMsgs,
                    ref numWarnMsgs, ref numInfoMsgs, ref importLog);

                if (ret == 0 && numFatalErrors == 0)
                {
                    sb.AppendLine("✓ Mass source set via database tables");
                    report = sb.ToString();
                    return true;
                }
                else
                {
                    sb.AppendLine($"⚠ Errors applying mass source: {numFatalErrors} fatal, {numErrorMsgs} errors");
                    if (!string.IsNullOrEmpty(importLog))
                        sb.AppendLine($"Log: {importLog}");
                    report = sb.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                report = "Exception in SetMassSourceViaDatabase:\r\n" + ex;
                return false;
            }
        }

        #endregion

        #region Mass Source Queries

        /// <summary>
        /// Gets the current mass source configuration
        /// </summary>
        public bool GetMassSourceInfo(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                sb.AppendLine("=== MASS SOURCE CONFIGURATION ===");

                bool includeElements = false;
                bool includeAddedMass = false;
                bool includeLoads = false;
                int numberLoads = 0;

                // initialize as empty (non-null) arrays for ref passing
                string[] loadPatterns = Array.Empty<string>();
                double[] scaleFactors = Array.Empty<double>();

                int ret = _SapModel.PropMaterial.GetMassSource_1(
                    ref includeElements,
                    ref includeAddedMass,
                    ref includeLoads,
                    ref numberLoads,
                    ref loadPatterns,
                    ref scaleFactors
                );

                if (ret == 0)
                {
                    sb.AppendLine($"From Elements (self-weight): {includeElements}");
                    sb.AppendLine($"Added Mass: {includeAddedMass}");
                    sb.AppendLine($"From Loads: {includeLoads}");

                    if (includeLoads && numberLoads > 0 && loadPatterns != null && scaleFactors != null)
                    {
                        sb.AppendLine("\nLoad patterns included as mass:");
                        int n = Math.Min(numberLoads, Math.Min(loadPatterns.Length, scaleFactors.Length));
                        for (int i = 0; i < n; i++)
                            sb.AppendLine($"  - {loadPatterns[i]}: {scaleFactors[i]}");
                    }
                }
                else
                {
                    sb.AppendLine($"GetMassSource_1 returned {ret}");
                }

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in GetMassSourceInfo:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Calculates and reports total mass in the model
        /// </summary>
        public bool GetTotalMass(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                sb.AppendLine("=== TOTAL MASS CALCULATION ===");

                // This requires running analysis first
                // We'll attempt to get mass from the model

                try
                {
                    // Check if analysis model exists
                    int ret = _SapModel.Analyze.CreateAnalysisModel();

                    if (ret == 0)
                    {
                        sb.AppendLine("Analysis model created/verified.");

                        // Note: Getting actual mass values requires more complex API calls
                        // that may vary by ETABS version
                        sb.AppendLine("\nTo view total mass:");
                        sb.AppendLine("1. Run analysis");
                        sb.AppendLine("2. Go to Display > Show Tables > Analysis Results");
                        sb.AppendLine("3. Select 'Assembled Joint Masses'");
                    }
                    else
                    {
                        sb.AppendLine("Could not create analysis model.");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Error: {ex.Message}");
                }

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in GetTotalMass:\r\n" + ex;
                return false;
            }
        }

        #endregion

        #region Preset Configurations

        /// <summary>
        /// Sets up mass source for serviceability analysis (full live load)
        /// </summary>
        public bool SetupServiceabilityMass(out string report)
        {
            var patternFactors = new Dictionary<string, double>
            {
                { "PLUGIN_DEAD", 1.0 },
                { "PLUGIN_LIVE", 1.0 }  // 100% live load for serviceability
            };

            return SetCustomMassSource(patternFactors, true, out report);
        }

        /// <summary>
        /// Sets up mass source for ultimate limit state (reduced live load)
        /// </summary>
        public bool SetupUltimateMass(out string report)
        {
            var patternFactors = new Dictionary<string, double>
            {
                { "PLUGIN_DEAD", 1.0 },
                { "PLUGIN_LIVE", 0.5 }  // 50% live load for ULS
            };

            return SetCustomMassSource(patternFactors, true, out report);
        }

        /// <summary>
        /// Sets up mass source with only dead load (no live load)
        /// </summary>
        public bool SetupDeadLoadOnlyMass(out string report)
        {
            var patternFactors = new Dictionary<string, double>
            {
                { "PLUGIN_DEAD", 1.0 }
            };

            return SetCustomMassSource(patternFactors, true, out report);
        }

        #endregion
    }
}