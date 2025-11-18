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
