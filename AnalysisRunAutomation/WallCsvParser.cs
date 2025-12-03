using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ETABS_Plugin
{
    /// <summary>
    /// Represents a wall definition from CSV
    /// </summary>
    public class WallCsvRow
    {
        public string WallName { get; set; } = "";
        public string Label { get; set; } = ""; // Custom label/name for the wall
        public string Story { get; set; } = ""; // Story/level to assign wall to
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double X3 { get; set; }
        public double Y3 { get; set; }
        public double X4 { get; set; }
        public double Y4 { get; set; }
        public double Elevation { get; set; }
        public string MaterialName { get; set; } = "";
        public double ThicknessMm { get; set; }
        public string PropertyName { get; set; } = ""; // Optional: use existing property instead of auto-creating
        public double BetaAngle { get; set; }
        public string PierLabel { get; set; } = "";
    }

    /// <summary>
    /// Simple CSV parser for wall definitions
    /// </summary>
    public class WallCsvParser
    {
        public bool ParseFile(string filePath, out List<WallCsvRow> rows, out List<string> errors)
        {
            rows = new List<WallCsvRow>();
            errors = new List<string>();

            if (!File.Exists(filePath))
            {
                errors.Add($"File not found: {filePath}");
                return false;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                return ParseLines(lines, out rows, out errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Error reading file: {ex.Message}");
                return false;
            }
        }

        public bool ParseLines(string[] lines, out List<WallCsvRow> rows, out List<string> errors)
        {
            rows = new List<WallCsvRow>();
            errors = new List<string>();

            if (lines == null || lines.Length == 0)
            {
                errors.Add("CSV file is empty");
                return false;
            }

            // Parse header
            var headerLine = lines[0].Trim();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                errors.Add("Header row is empty");
                return false;
            }

            var headers = ParseCsvLine(headerLine);
            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headers.Length; i++)
            {
                headerMap[headers[i].Trim()] = i;
            }

            // Required columns (only core coordinates and basic properties are truly required)
            string[] requiredColumns = new[]
            {
                "WallName", "X1", "Y1", "X2", "Y2", "X3", "Y3", "X4", "Y4", "Elevation"
            };

            // Optional columns with defaults
            string[] optionalColumns = new[]
            {
                "Label", "Story", "MaterialName", "ThicknessMm", "PropertyName", "BetaAngle", "PierLabel"
            };

            foreach (var required in requiredColumns)
            {
                if (!headerMap.ContainsKey(required))
                {
                    errors.Add($"Missing required column: {required}");
                }
            }

            if (errors.Any())
                return false;

            // Parse data rows
            for (int lineNum = 1; lineNum < lines.Length; lineNum++)
            {
                var line = lines[lineNum].Trim();

                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("//"))
                    continue;

                var values = ParseCsvLine(line);

                try
                {
                    var row = new WallCsvRow
                    {
                        WallName = GetValue(headerMap, values, "WallName", ""),
                        Label = GetValue(headerMap, values, "Label", ""),
                        Story = GetValue(headerMap, values, "Story", ""),
                        X1 = GetDoubleValue(headerMap, values, "X1", 0),
                        Y1 = GetDoubleValue(headerMap, values, "Y1", 0),
                        X2 = GetDoubleValue(headerMap, values, "X2", 0),
                        Y2 = GetDoubleValue(headerMap, values, "Y2", 0),
                        X3 = GetDoubleValue(headerMap, values, "X3", 0),
                        Y3 = GetDoubleValue(headerMap, values, "Y3", 0),
                        X4 = GetDoubleValue(headerMap, values, "X4", 0),
                        Y4 = GetDoubleValue(headerMap, values, "Y4", 0),
                        Elevation = GetDoubleValue(headerMap, values, "Elevation", 0),
                        MaterialName = GetValue(headerMap, values, "MaterialName", ""),
                        ThicknessMm = GetDoubleValue(headerMap, values, "ThicknessMm", 200),
                        PropertyName = GetValue(headerMap, values, "PropertyName", ""),
                        BetaAngle = GetDoubleValue(headerMap, values, "BetaAngle", 0),
                        PierLabel = GetValue(headerMap, values, "PierLabel", "")
                    };

                    rows.Add(row);
                }
                catch (Exception ex)
                {
                    errors.Add($"Line {lineNum + 1}: {ex.Message}");
                }
            }

            return errors.Count == 0;
        }

        private string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString().Trim());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            values.Add(currentValue.ToString().Trim());
            return values.ToArray();
        }

        private string GetValue(Dictionary<string, int> headerMap, string[] values, string columnName, string defaultValue)
        {
            if (headerMap.TryGetValue(columnName, out int index) && index < values.Length)
            {
                var value = values[index].Trim();
                return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
            }
            return defaultValue;
        }

        private double GetDoubleValue(Dictionary<string, int> headerMap, string[] values, string columnName, double defaultValue)
        {
            var strValue = GetValue(headerMap, values, columnName, null);

            if (string.IsNullOrWhiteSpace(strValue))
                return defaultValue;

            if (double.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;

            throw new FormatException($"Invalid number format for column '{columnName}': {strValue}");
        }

        /// <summary>
        /// Generates a sample CSV template
        /// </summary>
        public void GenerateTemplate(string outputPath)
        {
            var sb = new StringBuilder();

            // Header with all columns
            sb.AppendLine("WallName,Label,Story,X1,Y1,X2,Y2,X3,Y3,X4,Y4,Elevation,MaterialName,ThicknessMm,PropertyName,BetaAngle,PierLabel");

            // Documentation
            sb.AppendLine("# CSV Format for Wall Import");
            sb.AppendLine("# Required columns: WallName, X1-Y1 through X4-Y4 (4 corners), Elevation");
            sb.AppendLine("# Optional columns: Label, Story, MaterialName, ThicknessMm, PropertyName, BetaAngle, PierLabel");
            sb.AppendLine("#");
            sb.AppendLine("# Column Descriptions:");
            sb.AppendLine("#   WallName: Identifier for this wall in the CSV (used for error reporting)");
            sb.AppendLine("#   Label: Custom name/label for the wall in ETABS (leave blank for auto-generated)");
            sb.AppendLine("#   Story: Story/level to assign wall to (e.g., 'Story1', 'Level 2')");
            sb.AppendLine("#   X1,Y1 to X4,Y4: Four corner coordinates in meters (counterclockwise)");
            sb.AppendLine("#   Elevation: Z-coordinate in meters");
            sb.AppendLine("#   MaterialName: Material (e.g., 'CONC', 'C25') - leave blank to use default");
            sb.AppendLine("#   ThicknessMm: Wall thickness in millimeters (default: 200)");
            sb.AppendLine("#   PropertyName: Existing wall property name (leave blank to auto-create)");
            sb.AppendLine("#   BetaAngle: Local axis rotation in degrees (default: 0)");
            sb.AppendLine("#   PierLabel: Pier label for design (e.g., 'P1', 'P2')");
            sb.AppendLine("#");
            sb.AppendLine("# Coordinates are in meters, thickness in millimeters");
            sb.AppendLine("# Lines starting with # are comments and will be ignored");
            sb.AppendLine("");

            // Example 1: Full specification
            sb.AppendLine("# Example 1: Rectangular wall 4m long, 0.2m thick, at Story1");
            sb.AppendLine("Wall_1,W-01,Story1,0,0,4,0,4,0.2,0,0.2,0,CONC,200,,90,P1");
            sb.AppendLine("");

            // Example 2: Using existing property
            sb.AppendLine("# Example 2: Wall using existing property 'WALL200', at elevation 3m");
            sb.AppendLine("Wall_2,W-02,Story2,0,0,4,0,4,0.2,0,0.2,3,,,WALL200,90,P1");
            sb.AppendLine("");

            // Example 3: Minimal specification (auto-generated property)
            sb.AppendLine("# Example 3: Wall along Y axis with minimal specification");
            sb.AppendLine("Wall_3,,,0,0,0,4,0.2,4,0.2,0,0,,,,0,P2");

            File.WriteAllText(outputPath, sb.ToString());
        }
    }
}
