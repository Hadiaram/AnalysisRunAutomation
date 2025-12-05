using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ETABS_Plugin
{
    /// <summary>
    /// Represents a column definition from CSV
    /// </summary>
    public class ColumnCsvRow
    {
        public string ColumnName { get; set; } = "";
        public string Label { get; set; } = ""; // Custom label/name for the column
        public string Story { get; set; } = ""; // Story/level to assign column to
        public double X { get; set; }
        public double Y { get; set; }
        public double BaseElevation { get; set; }
        public double Height { get; set; }
        public string SectionName { get; set; } = "";
        public double RotationAngle { get; set; }
        public string ColumnLabel { get; set; } = "";
    }

    /// <summary>
    /// Simple CSV parser for column definitions
    /// </summary>
    public class ColumnCsvParser
    {
        public bool ParseFile(string filePath, out List<ColumnCsvRow> rows, out List<string> errors)
        {
            rows = new List<ColumnCsvRow>();
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

        public bool ParseLines(string[] lines, out List<ColumnCsvRow> rows, out List<string> errors)
        {
            rows = new List<ColumnCsvRow>();
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

            // Required columns
            string[] requiredColumns = new[]
            {
                "ColumnName", "X", "Y", "BaseElevation", "Height", "SectionName"
            };

            // Optional columns with defaults
            string[] optionalColumns = new[]
            {
                "Label", "Story", "RotationAngle", "ColumnLabel"
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
                    var row = new ColumnCsvRow
                    {
                        ColumnName = GetValue(headerMap, values, "ColumnName", ""),
                        Label = GetValue(headerMap, values, "Label", ""),
                        Story = GetValue(headerMap, values, "Story", ""),
                        X = GetDoubleValue(headerMap, values, "X", 0),
                        Y = GetDoubleValue(headerMap, values, "Y", 0),
                        BaseElevation = GetDoubleValue(headerMap, values, "BaseElevation", 0),
                        Height = GetDoubleValue(headerMap, values, "Height", 3),
                        SectionName = GetValue(headerMap, values, "SectionName", ""),
                        RotationAngle = GetDoubleValue(headerMap, values, "RotationAngle", 0),
                        ColumnLabel = GetValue(headerMap, values, "ColumnLabel", "")
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
            sb.AppendLine("ColumnName,Label,Story,X,Y,BaseElevation,Height,SectionName,RotationAngle,ColumnLabel");

            // Documentation
            sb.AppendLine("# CSV Format for Column Import");
            sb.AppendLine("# Required columns: ColumnName, X, Y, BaseElevation, Height, SectionName");
            sb.AppendLine("# Optional columns: Label, Story, RotationAngle, ColumnLabel");
            sb.AppendLine("#");
            sb.AppendLine("# Column Descriptions:");
            sb.AppendLine("#   ColumnName: Identifier for this column in the CSV (used for error reporting)");
            sb.AppendLine("#   Label: Custom name/label for the column in ETABS (leave blank for auto-generated)");
            sb.AppendLine("#   Story: Story/level to assign column to (e.g., 'Story1', 'Level 2')");
            sb.AppendLine("#   X, Y: Column location coordinates in meters");
            sb.AppendLine("#   BaseElevation: Base elevation (Z-coordinate) in meters");
            sb.AppendLine("#   Height: Column height in meters");
            sb.AppendLine("#   SectionName: Frame section name (e.g., 'COL-300x300', 'COL-400x400')");
            sb.AppendLine("#   RotationAngle: Local axis rotation in degrees (default: 0)");
            sb.AppendLine("#   ColumnLabel: Column label for design (e.g., 'C1', 'C2')");
            sb.AppendLine("#");
            sb.AppendLine("# Coordinates and elevations are in meters");
            sb.AppendLine("# Lines starting with # are comments and will be ignored");
            sb.AppendLine("");

            // Example 1: Full specification
            sb.AppendLine("# Example 1: Column at grid intersection, 3m height");
            sb.AppendLine("Col_1,C-01,Story1,0,0,0,3,COL-400x400,0,C1");
            sb.AppendLine("");

            // Example 2: Column with rotation
            sb.AppendLine("# Example 2: Column with 45-degree rotation, at elevation 3m");
            sb.AppendLine("Col_2,C-02,Story2,4,0,3,3,COL-300x300,45,C2");
            sb.AppendLine("");

            // Example 3: Minimal specification
            sb.AppendLine("# Example 3: Column at different location with minimal specification");
            sb.AppendLine("Col_3,,,8,0,0,3.5,COL-500x500,0,C3");

            File.WriteAllText(outputPath, sb.ToString());
        }
    }
}
