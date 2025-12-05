using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ETABS_Plugin
{
    /// <summary>
    /// Base class for CSV parsing functionality
    /// Provides common methods for reading, parsing, and validating CSV files
    /// </summary>
    public abstract class CsvParserBase
    {
        /// <summary>
        /// Parses a CSV line into individual values, handling quoted strings
        /// </summary>
        protected string[] ParseCsvLine(string line)
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

        /// <summary>
        /// Gets a string value from the CSV row, with default value if not found or empty
        /// </summary>
        protected string GetValue(Dictionary<string, int> headerMap, string[] values, string columnName, string defaultValue)
        {
            if (headerMap.TryGetValue(columnName, out int index) && index < values.Length)
            {
                var value = values[index].Trim();
                return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a double value from the CSV row, with default value if not found or empty
        /// </summary>
        protected double GetDoubleValue(Dictionary<string, int> headerMap, string[] values, string columnName, double defaultValue)
        {
            var strValue = GetValue(headerMap, values, columnName, null);

            if (string.IsNullOrWhiteSpace(strValue))
                return defaultValue;

            if (double.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;

            throw new FormatException($"Invalid number format for column '{columnName}': {strValue}");
        }

        /// <summary>
        /// Gets an integer value from the CSV row, with default value if not found or empty
        /// </summary>
        protected int GetIntValue(Dictionary<string, int> headerMap, string[] values, string columnName, int defaultValue)
        {
            var strValue = GetValue(headerMap, values, columnName, null);

            if (string.IsNullOrWhiteSpace(strValue))
                return defaultValue;

            if (int.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
                return result;

            throw new FormatException($"Invalid integer format for column '{columnName}': {strValue}");
        }

        /// <summary>
        /// Gets a boolean value from the CSV row, with default value if not found or empty
        /// Accepts: true/false, yes/no, 1/0
        /// </summary>
        protected bool GetBoolValue(Dictionary<string, int> headerMap, string[] values, string columnName, bool defaultValue)
        {
            var strValue = GetValue(headerMap, values, columnName, null);

            if (string.IsNullOrWhiteSpace(strValue))
                return defaultValue;

            strValue = strValue.ToLower().Trim();

            if (strValue == "true" || strValue == "yes" || strValue == "1")
                return true;

            if (strValue == "false" || strValue == "no" || strValue == "0")
                return false;

            throw new FormatException($"Invalid boolean format for column '{columnName}': {strValue}");
        }

        /// <summary>
        /// Creates a header map from column names to their indices
        /// </summary>
        protected Dictionary<string, int> CreateHeaderMap(string[] headers)
        {
            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headers.Length; i++)
            {
                headerMap[headers[i].Trim()] = i;
            }

            return headerMap;
        }

        /// <summary>
        /// Validates that all required columns are present in the header map
        /// </summary>
        protected bool ValidateRequiredColumns(Dictionary<string, int> headerMap, string[] requiredColumns, List<string> errors)
        {
            bool isValid = true;

            foreach (var required in requiredColumns)
            {
                if (!headerMap.ContainsKey(required))
                {
                    errors.Add($"Missing required column: {required}");
                    isValid = false;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Checks if a line should be skipped (empty or comment)
        /// </summary>
        protected bool ShouldSkipLine(string line)
        {
            var trimmed = line.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ||
                   trimmed.StartsWith("#") ||
                   trimmed.StartsWith("//");
        }

        /// <summary>
        /// Reads all lines from a CSV file
        /// </summary>
        protected bool ReadCsvFile(string filePath, out string[] lines, out List<string> errors)
        {
            lines = null;
            errors = new List<string>();

            if (!File.Exists(filePath))
            {
                errors.Add($"File not found: {filePath}");
                return false;
            }

            try
            {
                lines = File.ReadAllLines(filePath);
                return true;
            }
            catch (Exception ex)
            {
                errors.Add($"Error reading file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Writes a CSV template to a file
        /// </summary>
        protected void WriteCsvTemplate(string outputPath, StringBuilder content)
        {
            File.WriteAllText(outputPath, content.ToString());
        }

        /// <summary>
        /// Adds a header row to the template
        /// </summary>
        protected void AddTemplateHeader(StringBuilder sb, string[] columns)
        {
            sb.AppendLine(string.Join(",", columns));
        }

        /// <summary>
        /// Adds a comment line to the template
        /// </summary>
        protected void AddTemplateComment(StringBuilder sb, string comment)
        {
            sb.AppendLine($"# {comment}");
        }

        /// <summary>
        /// Adds a blank line to the template
        /// </summary>
        protected void AddTemplateBlankLine(StringBuilder sb)
        {
            sb.AppendLine("");
        }

        /// <summary>
        /// Adds an example data row to the template
        /// </summary>
        protected void AddTemplateExample(StringBuilder sb, string exampleDescription, string[] values)
        {
            if (!string.IsNullOrWhiteSpace(exampleDescription))
            {
                AddTemplateComment(sb, exampleDescription);
            }
            sb.AppendLine(string.Join(",", values));
        }
    }

    /// <summary>
    /// Interface for CSV row data
    /// </summary>
    public interface ICsvRow
    {
        /// <summary>
        /// Gets the identifier/name of this row (used for error reporting)
        /// </summary>
        string GetIdentifier();
    }
}
