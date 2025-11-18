using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ETABSv1;

namespace ETABS_Plugin
{
    public class DataExtractionManager
    {
        private cSapModel _SapModel;

        public DataExtractionManager(cSapModel sapModel)
        {
            _SapModel = sapModel;
        }

        #region Analysis Results Extraction

        /// <summary>
        /// Extracts base reactions for all load cases and combinations
        /// </summary>
        public bool ExtractBaseReactions(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting base reactions...\r\n");

                // Get all load cases
                int nCases = 0;
                string[] caseNames = Array.Empty<string>();
                _SapModel.LoadCases.GetNameList(ref nCases, ref caseNames);

                // Get all load combinations
                int nCombos = 0;
                string[] comboNames = Array.Empty<string>();
                _SapModel.RespCombo.GetNameList(ref nCombos, ref comboNames);

                // Combine both
                var allLoadCases = new List<string>();
                allLoadCases.AddRange(caseNames);
                allLoadCases.AddRange(comboNames);

                reportSb.AppendLine($"Found {caseNames.Length} load cases and {comboNames.Length} combinations\r\n");

                // CSV Header
                sb.AppendLine("LoadCase,Fx(kN),Fy(kN),Fz(kN),Mx(kN-m),My(kN-m),Mz(kN-m)");

                int extractedCount = 0;

                // Extract reactions for each load case
                foreach (var loadCase in allLoadCases)
                {
                    if (GetBaseReactionsForCase(loadCase, out var reactions))
                    {
                        sb.AppendLine($"{loadCase}," +
                            $"{reactions.Fx:0.00},{reactions.Fy:0.00},{reactions.Fz:0.00}," +
                            $"{reactions.Mx:0.00},{reactions.My:0.00},{reactions.Mz:0.00}");
                        extractedCount++;
                    }
                }

                reportSb.AppendLine($"✓ Extracted base reactions for {extractedCount} load cases/combinations");
                csvData = sb.ToString();
                report = reportSb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}";
                return false;
            }
        }

        private bool GetBaseReactionsForCase(string loadCaseName, out BaseReactions reactions)
        {
            reactions = new BaseReactions();

            try
            {
                // Get all points
                int nPoints = 0;
                string[] ptNames = Array.Empty<string>();
                _SapModel.PointObj.GetNameList(ref nPoints, ref ptNames);

                double totalFx = 0, totalFy = 0, totalFz = 0;
                double totalMx = 0, totalMy = 0, totalMz = 0;

                foreach (var ptName in ptNames)
                {
                    // Check if point has restraints
                    bool[] restraints = new bool[6];
                    _SapModel.PointObj.GetRestraint(ptName, ref restraints);

                    // If any restraint exists, it's a support
                    if (restraints.Any(r => r))
                    {
                        // Get reactions
                        int numberResults = 0;
                        string[] obj = Array.Empty<string>();
                        string[] elm = Array.Empty<string>();
                        string[] pointElm = Array.Empty<string>();
                        string[] loadCase = Array.Empty<string>();
                        string[] stepType = Array.Empty<string>();
                        double[] stepNum = Array.Empty<double>();
                        double[] fx = Array.Empty<double>();
                        double[] fy = Array.Empty<double>();
                        double[] fz = Array.Empty<double>();
                        double[] mx = Array.Empty<double>();
                        double[] my = Array.Empty<double>();
                        double[] mz = Array.Empty<double>();

                        int ret = _SapModel.Results.JointReact(
                            ptName, eItemTypeElm.ObjectElm,
                            ref numberResults, ref obj, ref elm,
                            ref loadCase, ref stepType, ref stepNum,
                            ref fx, ref fy, ref fz, ref mx, ref my, ref mz);

                        if (ret == 0 && numberResults > 0)
                        {
                            // Find the result for our load case
                            for (int i = 0; i < numberResults; i++)
                            {
                                if (loadCase[i] == loadCaseName)
                                {
                                    totalFx += fx[i];
                                    totalFy += fy[i];
                                    totalFz += fz[i];
                                    totalMx += mx[i];
                                    totalMy += my[i];
                                    totalMz += mz[i];
                                    break;
                                }
                            }
                        }
                    }
                }

                // Convert to kN and kN·m
                reactions.Fx = totalFx / 1000.0;
                reactions.Fy = totalFy / 1000.0;
                reactions.Fz = totalFz / 1000.0;
                reactions.Mx = totalMx / 1e6;
                reactions.My = totalMy / 1e6;
                reactions.Mz = totalMz / 1e6;

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Project/Model Information Extraction

        /// <summary>
        /// Extracts project and model information including filename and units
        /// </summary>
        public bool ExtractProjectInfo(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting project/model information...\r\n");

                // Get model filename with full path
                string filenameWithPath = _SapModel.GetModelFilename(true);
                string filenameOnly = _SapModel.GetModelFilename(false);

                // Get present units
                eUnits units = _SapModel.GetPresentUnits();
                string unitsString = GetUnitsDescription(units);

                reportSb.AppendLine($"✓ Model Filename: {filenameOnly}");
                reportSb.AppendLine($"✓ Full Path: {filenameWithPath}");
                reportSb.AppendLine($"✓ Current Units: {unitsString}");

                // CSV Header
                sb.AppendLine("Property,Value");
                sb.AppendLine($"Model Filename,\"{filenameOnly}\"");
                sb.AppendLine($"Full Path,\"{filenameWithPath}\"");
                sb.AppendLine($"Current Units,{unitsString}");
                sb.AppendLine($"Units Enum Value,{(int)units}");

                // Add timestamp
                sb.AppendLine($"Extraction Date,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                reportSb.AppendLine("\r\n✓ Project information extracted successfully");
                csvData = sb.ToString();
                report = reportSb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Converts eUnits enum to readable description
        /// </summary>
        private string GetUnitsDescription(eUnits units)
        {
            switch (units)
            {
                case eUnits.lb_in_F:
                    return "lb_in_F (Pound-Inch-Fahrenheit)";
                case eUnits.lb_ft_F:
                    return "lb_ft_F (Pound-Foot-Fahrenheit)";
                case eUnits.kip_in_F:
                    return "kip_in_F (Kip-Inch-Fahrenheit)";
                case eUnits.kip_ft_F:
                    return "kip_ft_F (Kip-Foot-Fahrenheit)";
                case eUnits.kN_mm_C:
                    return "kN_mm_C (Kilonewton-Millimeter-Celsius)";
                case eUnits.kN_m_C:
                    return "kN_m_C (Kilonewton-Meter-Celsius)";
                case eUnits.kgf_mm_C:
                    return "kgf_mm_C (Kilogram-Force-Millimeter-Celsius)";
                case eUnits.kgf_m_C:
                    return "kgf_m_C (Kilogram-Force-Meter-Celsius)";
                case eUnits.N_mm_C:
                    return "N_mm_C (Newton-Millimeter-Celsius)";
                case eUnits.N_m_C:
                    return "N_m_C (Newton-Meter-Celsius)";
                case eUnits.Ton_mm_C:
                    return "Ton_mm_C (Metric Ton-Millimeter-Celsius)";
                case eUnits.Ton_m_C:
                    return "Ton_m_C (Metric Ton-Meter-Celsius)";
                case eUnits.kN_cm_C:
                    return "kN_cm_C (Kilonewton-Centimeter-Celsius)";
                case eUnits.kgf_cm_C:
                    return "kgf_cm_C (Kilogram-Force-Centimeter-Celsius)";
                case eUnits.N_cm_C:
                    return "N_cm_C (Newton-Centimeter-Celsius)";
                case eUnits.Ton_cm_C:
                    return "Ton_cm_C (Metric Ton-Centimeter-Celsius)";
                default:
                    return $"Unknown ({(int)units})";
            }
        }

        #endregion

        #region Story/Level Information Extraction

        /// <summary>
        /// Extracts all story information including names, elevations, heights, and properties
        /// </summary>
        public bool ExtractStoryInfo(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting story/level information...\r\n");

                // Get story data
                int numberStories = 0;
                string[] storyNames = Array.Empty<string>();
                double[] storyElevations = Array.Empty<double>();
                double[] storyHeights = Array.Empty<double>();
                bool[] isMasterStory = Array.Empty<bool>();
                string[] similarToStory = Array.Empty<string>();
                bool[] spliceAbove = Array.Empty<bool>();
                double[] spliceHeight = Array.Empty<double>();

                int ret = _SapModel.Story.GetStories(
                    ref numberStories,
                    ref storyNames,
                    ref storyElevations,
                    ref storyHeights,
                    ref isMasterStory,
                    ref similarToStory,
                    ref spliceAbove,
                    ref spliceHeight);

                if (ret != 0)
                {
                    csvData = "";
                    report = $"ERROR: Story.GetStories() returned error code {ret}";
                    return false;
                }

                reportSb.AppendLine($"✓ Found {numberStories} stories (plus Base level)");
                reportSb.AppendLine($"✓ Total entries: {storyNames.Length}");

                // CSV Header
                sb.AppendLine("StoryName,Elevation,Height,IsMasterStory,SimilarToStory,SpliceAbove,SpliceHeight");

                // Export each story
                for (int i = 0; i < storyNames.Length; i++)
                {
                    string name = storyNames[i];
                    double elevation = storyElevations[i];
                    double height = storyHeights[i];
                    bool isMaster = isMasterStory[i];
                    string similarTo = similarToStory[i];
                    bool splice = spliceAbove[i];
                    double spliceHt = spliceHeight[i];

                    sb.AppendLine($"\"{name}\",{elevation:0.0000},{height:0.0000}," +
                        $"{isMaster},{(string.IsNullOrEmpty(similarTo) ? "N/A" : similarTo)}," +
                        $"{splice},{spliceHt:0.0000}");
                }

                reportSb.AppendLine($"\r\nStory range:");
                if (storyNames.Length > 0)
                {
                    reportSb.AppendLine($"  Base: {storyElevations[0]:0.00}");
                    if (storyNames.Length > 1)
                    {
                        reportSb.AppendLine($"  Top Story: {storyNames[storyNames.Length - 1]} at {storyElevations[storyNames.Length - 1]:0.00}");
                    }
                }

                reportSb.AppendLine("\r\n✓ Story information extracted successfully");
                csvData = sb.ToString();
                report = reportSb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}";
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Saves CSV data to a file
        /// </summary>
        public bool SaveToFile(string csvData, string filePath, out string error)
        {
            try
            {
                File.WriteAllText(filePath, csvData);
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        #endregion

        #region Data Structures

        public struct BaseReactions
        {
            public double Fx { get; set; }
            public double Fy { get; set; }
            public double Fz { get; set; }
            public double Mx { get; set; }
            public double My { get; set; }
            public double Mz { get; set; }
        }

        #endregion
    }
}
