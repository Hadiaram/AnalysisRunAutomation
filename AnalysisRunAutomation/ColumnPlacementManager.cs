using System;
using ETABSv1;

namespace ETABS_Plugin
{
    public class ColumnPlacementManager
    {
        private cSapModel _SapModel;

        public ColumnPlacementManager(cSapModel sapModel)
        {
            _SapModel = sapModel;
        }

        /// <summary>
        /// Places a column in the ETABS model with specified parameters
        /// </summary>
        public bool PlaceColumn(
            double x,
            double y,
            double baseElevation,
            double height,
            string sectionName,
            double rotationAngle,
            string columnLabel,
            out string report,
            string customLabel = "",
            string storyName = ""
        )
        {
            try
            {
                report = "Starting column placement...\r\n";

                // Set units to kN-m-C
                int ret = _SapModel.SetPresentUnits(eUnits.kN_m_C);
                if (!CheckResult(ret, "SetPresentUnits", ref report))
                    return false;

                // Calculate top elevation
                double topElevation = baseElevation + height;

                // Create base point
                string basePoint = "";
                ret = _SapModel.PointObj.AddCartesian(x, y, baseElevation, ref basePoint);
                if (!CheckResult(ret, "PointObj.AddCartesian (base)", ref report))
                    return false;

                // Create top point
                string topPoint = "";
                ret = _SapModel.PointObj.AddCartesian(x, y, topElevation, ref topPoint);
                if (!CheckResult(ret, "PointObj.AddCartesian (top)", ref report))
                    return false;

                report += $"Points created: {basePoint} (base), {topPoint} (top)\r\n";

                // Create column frame object with custom name if provided
                string columnName = customLabel; // Use custom label if provided

                ret = _SapModel.FrameObj.AddByPoint(
                    Point1: basePoint,
                    Point2: topPoint,
                    Name: ref columnName,
                    PropName: sectionName,
                    UserName: string.IsNullOrWhiteSpace(customLabel) ? "" : customLabel
                );

                if (!CheckResult(ret, "FrameObj.AddByPoint", ref report))
                    return false;

                report += $"Column frame created: {columnName}";
                if (!string.IsNullOrWhiteSpace(customLabel) && columnName != customLabel)
                {
                    report += $" (requested: {customLabel})";
                }
                report += "\r\n";

                // Assign to story if specified
                if (!string.IsNullOrWhiteSpace(storyName))
                {
                    ret = _SapModel.FrameObj.SetGroupAssign(columnName, storyName);
                    if (ret == 0)
                    {
                        report += $"Assigned to story: {storyName}\r\n";
                    }
                    else
                    {
                        report += $"⚠ Warning: Could not assign to story '{storyName}' (code: {ret})\r\n";
                    }
                }

                // Set local axes orientation (rotation about local 2-axis)
                if (Math.Abs(rotationAngle) > 0.001)
                {
                    ret = _SapModel.FrameObj.SetLocalAxes(columnName, rotationAngle);
                    if (!CheckResult(ret, "FrameObj.SetLocalAxes", ref report))
                        return false;

                    report += $"Local axes rotated by {rotationAngle}°\r\n";
                }

                // Set column label if provided
                if (!string.IsNullOrEmpty(columnLabel))
                {
                    ret = _SapModel.FrameObj.SetPier(columnName, columnLabel);
                    if (!CheckResult(ret, "FrameObj.SetPier", ref report))
                        return false;

                    report += $"Column label assigned: {columnLabel}\r\n";
                }

                report += $"\r\n✓ Column placed successfully!\r\n";
                report += $"  - Name: {columnName}\r\n";
                report += $"  - Section: {sectionName}\r\n";
                report += $"  - Base Elevation: {baseElevation} m\r\n";
                report += $"  - Top Elevation: {topElevation} m\r\n";
                report += $"  - Height: {height} m\r\n";
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
        /// Gets all available frame section names from the model
        /// </summary>
        public bool GetFrameSections(out string[] sections, out string error)
        {
            try
            {
                int numNames = 0;
                string[] names = null;

                int ret = _SapModel.PropFrame.GetNameList(ref numNames, ref names);
                if (ret != 0)
                {
                    error = "Failed to get frame section list";
                    sections = new string[0];
                    return false;
                }

                sections = names ?? new string[0];
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                sections = new string[0];
                return false;
            }
        }

        /// <summary>
        /// Imports multiple columns from a CSV file
        /// </summary>
        public bool ImportColumnsFromCsv(string csvFilePath, out string report)
        {
            var sb = new System.Text.StringBuilder();

            try
            {
                sb.AppendLine($"=== CSV IMPORT: {System.IO.Path.GetFileName(csvFilePath)} ===\r\n");

                // Parse CSV file
                var parser = new ColumnCsvParser();
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

                sb.AppendLine($"✓ CSV parsed successfully: {rows.Count} column(s) found\r\n");

                if (rows.Count == 0)
                {
                    sb.AppendLine("⚠ No columns to import");
                    report = sb.ToString();
                    return true;
                }

                // Import each column
                int successCount = 0;
                int failCount = 0;

                foreach (var row in rows)
                {
                    sb.AppendLine($"\r\n--- Processing column: {row.ColumnName} ---");

                    string columnReport = "";
                    bool success = PlaceColumn(
                        row.X,
                        row.Y,
                        row.BaseElevation,
                        row.Height,
                        row.SectionName,
                        row.RotationAngle,
                        row.ColumnLabel,
                        out columnReport,
                        row.Label,
                        row.Story
                    );

                    sb.Append(columnReport);

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
                sb.AppendLine($"Total columns: {rows.Count}");
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
                var parser = new ColumnCsvParser();
                parser.GenerateTemplate(outputPath);

                report = $"✓ CSV template generated successfully:\r\n{outputPath}\r\n\r\n" +
                         "The template includes:\r\n" +
                         "- Header row with all columns (required and optional)\r\n" +
                         "- Detailed column descriptions in comments\r\n" +
                         "- 3 example columns showing different use cases\r\n\r\n" +
                         "Key features:\r\n" +
                         "- Label: Set custom column names\r\n" +
                         "- Story: Assign columns to specific stories\r\n" +
                         "- SectionName: Use existing frame sections\r\n" +
                         "- ColumnLabel: Assign column labels for design\r\n\r\n" +
                         "Edit the template to define your columns, then import it using 'Import from CSV'.";

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
