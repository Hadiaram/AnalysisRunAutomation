using System;
using ETABSv1;

namespace ETABS_Plugin
{
    public class WallPlacementManager
    {
        private cSapModel _SapModel;

        public WallPlacementManager(cSapModel sapModel)
        {
            _SapModel = sapModel;
        }

        /// <summary>
        /// Places a wall in the ETABS model with specified parameters
        /// </summary>
        public bool PlaceWall(
            double x1, double y1,
            double x2, double y2,
            double x3, double y3,
            double x4, double y4,
            double elevation,
            string materialName,
            double thickness,
            double betaAngle,
            string pierLabel,
            string customLabel = "",
            string storyName = "",
            string propertyName = "",
            out string report)
        {
            try
            {
                report = "Starting wall placement...\r\n";

                // Set units to kN-m-C
                int ret = _SapModel.SetPresentUnits(eUnits.kN_m_C);
                if (!CheckResult(ret, "SetPresentUnits", ref report))
                    return false;

                // Determine property name
                string wallProp = propertyName;
                bool useExistingProperty = !string.IsNullOrWhiteSpace(propertyName);

                if (!useExistingProperty)
                {
                    // Auto-generate property name if not provided
                    wallProp = $"WALL_{(int)(thickness * 1000)}mm_{materialName}";

                    // Create wall property
                    ret = _SapModel.PropArea.SetWall(
                        Name: wallProp,
                        WallPropType: eWallPropType.Specified,
                        ShellType: eShellType.ShellThick,
                        MatProp: materialName,
                        Thickness: thickness,
                        color: -1,
                        notes: "",
                        GUID: ""
                    );

                    if (!CheckResult(ret, "PropArea.SetWall", ref report))
                        return false;

                    report += $"Wall property created: {wallProp}\r\n";
                }
                else
                {
                    report += $"Using existing property: {wallProp}\r\n";
                }

                // Create points
                string p1 = "", p2 = "", p3 = "", p4 = "";

                ret = _SapModel.PointObj.AddCartesian(x1, y1, elevation, ref p1);
                if (!CheckResult(ret, "PointObj.AddCartesian p1", ref report))
                    return false;

                ret = _SapModel.PointObj.AddCartesian(x2, y2, elevation, ref p2);
                if (!CheckResult(ret, "PointObj.AddCartesian p2", ref report))
                    return false;

                ret = _SapModel.PointObj.AddCartesian(x3, y3, elevation, ref p3);
                if (!CheckResult(ret, "PointObj.AddCartesian p3", ref report))
                    return false;

                ret = _SapModel.PointObj.AddCartesian(x4, y4, elevation, ref p4);
                if (!CheckResult(ret, "PointObj.AddCartesian p4", ref report))
                    return false;

                report += $"Points created: {p1}, {p2}, {p3}, {p4}\r\n";

                // Create wall area object with custom name if provided
                string[] loop = new[] { p1, p2, p3, p4 };
                string wallName = customLabel; // Use custom label if provided

                ret = _SapModel.AreaObj.AddByPoint(
                    NumberPoints: loop.Length,
                    Point: ref loop,
                    Name: ref wallName,
                    PropName: wallProp,
                    UserName: string.IsNullOrWhiteSpace(customLabel) ? "" : customLabel
                );

                if (!CheckResult(ret, "AreaObj.AddByPoint", ref report))
                    return false;

                report += $"Wall area created: {wallName}";
                if (!string.IsNullOrWhiteSpace(customLabel) && wallName != customLabel)
                {
                    report += $" (requested: {customLabel})";
                }
                report += "\r\n";

                // Assign to story if specified
                if (!string.IsNullOrWhiteSpace(storyName))
                {
                    ret = _SapModel.AreaObj.SetGroupAssign(wallName, storyName);
                    if (ret == 0)
                    {
                        report += $"Assigned to story: {storyName}\r\n";
                    }
                    else
                    {
                        report += $"⚠ Warning: Could not assign to story '{storyName}' (code: {ret})\r\n";
                    }
                }

                // Set local axes orientation
                ret = _SapModel.AreaObj.SetLocalAxes(wallName, betaAngle);
                if (!CheckResult(ret, "AreaObj.SetLocalAxes", ref report))
                    return false;

                report += $"Local axes rotated by {betaAngle}°\r\n";

                // Set pier label if provided
                if (!string.IsNullOrEmpty(pierLabel))
                {
                    ret = _SapModel.AreaObj.SetPier(wallName, pierLabel);
                    if (!CheckResult(ret, "AreaObj.SetPier", ref report))
                        return false;

                    report += $"Pier label assigned: {pierLabel}\r\n";
                }

                report += $"\r\n✓ Wall placed successfully!\r\n";
                report += $"  - Name: {wallName}\r\n";
                report += $"  - Property: {wallProp}\r\n";
                report += $"  - Thickness: {thickness * 1000} mm\r\n";
                report += $"  - Elevation: {elevation} m\r\n";
                if (!string.IsNullOrWhiteSpace(storyName))
                    report += $"  - Story: {storyName}\r\n";

                return true;
            }
            catch (Exception ex)
            {
                report = $"ERROR: {ex.Message}\r\n";
                return false;
            }
        }

        /// <summary>
        /// Gets all available concrete material names from the model
        /// </summary>
        public bool GetConcreteMaterials(out string[] materials, out string error)
        {
            try
            {
                int numNames = 0;
                string[] names = null;

                int ret = _SapModel.PropMaterial.GetNameList(ref numNames, ref names);
                if (ret != 0)
                {
                    error = "Failed to get material list";
                    materials = new string[0];
                    return false;
                }

                // Filter for concrete materials
                var concreteList = new System.Collections.Generic.List<string>();
                for (int i = 0; i < numNames; i++)
                {
                    eMatType matType = eMatType.Steel;
                    int color = 0;
                    string notes = "";
                    string guid = "";

                    ret = _SapModel.PropMaterial.GetMaterial(names[i], ref matType, ref color, ref notes, ref guid);
                    if (ret == 0 && matType == eMatType.Concrete)
                    {
                        concreteList.Add(names[i]);
                    }
                }

                materials = concreteList.ToArray();
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                materials = new string[0];
                return false;
            }
        }

        /// <summary>
        /// Imports multiple walls from a CSV file
        /// </summary>
        public bool ImportWallsFromCsv(string csvFilePath, out string report)
        {
            var sb = new System.Text.StringBuilder();

            try
            {
                sb.AppendLine($"=== CSV IMPORT: {System.IO.Path.GetFileName(csvFilePath)} ===\r\n");

                // Parse CSV file
                var parser = new WallCsvParser();
                if (!parser.ParseFile(csvFilePath, out var rows, out var parseErrors))
                {
                    sb.AppendLine("✗ CSV parsing failed:");
                    foreach (var error in parseErrors)
                    {
                        sb.AppendLine($"  - {error}");
                    }
                    report = sb.ToString();
                    return false;
                }

                sb.AppendLine($"✓ CSV parsed successfully: {rows.Count} wall(s) found\r\n");

                if (rows.Count == 0)
                {
                    sb.AppendLine("⚠ No walls to import");
                    report = sb.ToString();
                    return true;
                }

                // Import each wall
                int successCount = 0;
                int failCount = 0;

                foreach (var row in rows)
                {
                    sb.AppendLine($"\r\n--- Processing wall: {row.WallName} ---");

                    string wallReport = "";
                    bool success = PlaceWall(
                        row.X1, row.Y1,
                        row.X2, row.Y2,
                        row.X3, row.Y3,
                        row.X4, row.Y4,
                        row.Elevation,
                        row.MaterialName,
                        row.ThicknessMm / 1000.0, // Convert mm to m
                        row.BetaAngle,
                        row.PierLabel,
                        row.Label,        // Custom label
                        row.Story,        // Story assignment
                        row.PropertyName, // Existing property
                        out wallReport
                    );

                    sb.Append(wallReport);

                    if (success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }

                sb.AppendLine($"\r\n=== IMPORT SUMMARY ===");
                sb.AppendLine($"Total walls: {rows.Count}");
                sb.AppendLine($"✓ Successful: {successCount}");
                sb.AppendLine($"✗ Failed: {failCount}");

                report = sb.ToString();
                return failCount == 0;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"\r\n✗ Unexpected error during CSV import: {ex.Message}");
                report = sb.ToString();
                return false;
            }
        }

        /// <summary>
        /// Generates a CSV template file
        /// </summary>
        public bool GenerateCsvTemplate(string outputPath, out string report)
        {
            try
            {
                var parser = new WallCsvParser();
                parser.GenerateTemplate(outputPath);

                report = $"✓ CSV template generated successfully:\r\n{outputPath}\r\n\r\n" +
                         "The template includes:\r\n" +
                         "- Header row with all columns (required and optional)\r\n" +
                         "- Detailed column descriptions in comments\r\n" +
                         "- 3 example walls showing different use cases\r\n\r\n" +
                         "Key features:\r\n" +
                         "- Label: Set custom wall names\r\n" +
                         "- Story: Assign walls to specific stories\r\n" +
                         "- PropertyName: Use existing wall properties\r\n" +
                         "- PierLabel: Assign pier labels for design\r\n\r\n" +
                         "Edit the template to define your walls, then import it using 'Import from CSV'.";

                return true;
            }
            catch (Exception ex)
            {
                report = $"✗ Failed to generate CSV template: {ex.Message}";
                return false;
            }
        }

        private bool CheckResult(int ret, string operation, ref string report)
        {
            if (ret != 0)
            {
                report += $"✗ {operation} failed (return code: {ret})\r\n";
                return false;
            }
            return true;
        }
    }
}
