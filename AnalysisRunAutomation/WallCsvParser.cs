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

            // Required columns
            string[] requiredColumns = new[]
            {
                "WallName", "X1", "Y1", "X2", "Y2", "X3", "Y3", "X4", "Y4",
                "Elevation", "MaterialName", "ThicknessMm", "BetaAngle", "PierLabel"
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

            // Header
            sb.AppendLine("WallName,X1,Y1,X2,Y2,X3,Y3,X4,Y4,Elevation,MaterialName,ThicknessMm,BetaAngle,PierLabel");

            // Sample rows (comments)
            sb.AppendLine("# Sample walls - modify or delete these rows");
            sb.AppendLine("# Coordinates in meters, thickness in millimeters");
            sb.AppendLine("");
            sb.AppendLine("# Example 1: Rectangular wall 4m long, 0.2m thick, at elevation 0");
            sb.AppendLine("Wall_1,0,0,4,0,4,0.2,0,0.2,0,CONC,200,90,P1");
            sb.AppendLine("");
            sb.AppendLine("# Example 2: Same wall at elevation 3m");
            sb.AppendLine("Wall_2,0,0,4,0,4,0.2,0,0.2,3,CONC,200,90,P1");
            sb.AppendLine("");
            sb.AppendLine("# Example 3: Wall along Y axis");
            sb.AppendLine("Wall_3,0,0,0,4,0.2,4,0.2,0,0,CONC,250,0,P2");

            File.WriteAllText(outputPath, sb.ToString());
        }
    }
}
