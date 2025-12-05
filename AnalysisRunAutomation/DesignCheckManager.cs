using System;
using System.Collections.Generic;
using System.Text;
using ETABSv1;

namespace ETABS_Plugin
{
    /// <summary>
    /// Manages design checks for steel frames, concrete frames, composite beams, and slabs
    /// </summary>
    public class DesignCheckManager
    {
        private cSapModel _SapModel;

        public DesignCheckManager(cSapModel sapModel)
        {
            _SapModel = sapModel;
        }

        #region Steel Frame Design

        /// <summary>
        /// Start steel frame design check
        /// </summary>
        public bool StartSteelDesign(out string report)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== STARTING STEEL FRAME DESIGN CHECK ===");

            try
            {
                // Check if results are available
                bool resultsAvailable = _SapModel.DesignSteel.GetResultsAvailable();
                if (!resultsAvailable)
                {
                    sb.AppendLine("⚠ No analysis results available. Running analysis first...");

                    // Run analysis
                    int ret = _SapModel.Analyze.RunAnalysis();
                    if (ret != 0)
                    {
                        sb.AppendLine("✗ Analysis failed. Cannot proceed with design check.");
                        report = sb.ToString();
                        return false;
                    }
                    sb.AppendLine("✓ Analysis completed");
                }
                else
                {
                    sb.AppendLine("✓ Analysis results are available");
                }

                // Start steel design
                sb.AppendLine("Starting steel frame design check...");
                int result = _SapModel.DesignSteel.StartDesign();

                if (result == 0)
                {
                    sb.AppendLine("✓ Steel frame design check completed successfully!");

                    // Check if design results are now available
                    bool designResults = _SapModel.DesignSteel.GetResultsAvailable();
                    sb.AppendLine($"Design results available: {designResults}");

                    report = sb.ToString();
                    return true;
                }
                else
                {
                    sb.AppendLine($"✗ Steel design failed with error code: {result}");
                    report = sb.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ EXCEPTION: {ex.Message}");
                report = sb.ToString();
                return false;
            }
        }

        /// <summary>
        /// Extract steel frame design check results
        /// </summary>
        public bool ExtractSteelDesignResults(out string csvData, out string report)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder csv = new StringBuilder();

            sb.AppendLine("=== EXTRACTING STEEL FRAME DESIGN RESULTS ===");

            try
            {
                // Check if results are available
                bool resultsAvailable = _SapModel.DesignSteel.GetResultsAvailable();
                if (!resultsAvailable)
                {
                    sb.AppendLine("✗ No steel design results available. Run steel design check first.");
                    csvData = "";
                    report = sb.ToString();
                    return false;
                }

                // CSV Header
                csv.AppendLine("FrameName,FrameType,DesignSection,Status,PMMCombo,PMMRatio,PRatio,MMajRatio,MMinRatio,VMajCombo,VMajRatio,VMinCombo,VMinRatio");

                // Get summary results
                int NumberItems = 0;
                string[] FrameName = null;
                string[] FrameType = null;
                string[] DesignSect = null;
                int[] Status = null;
                string[] PMMCombo = null;
                double[] PMMRatio = null;
                double[] PRatio = null;
                double[] MMajRatio = null;
                double[] MMinRatio = null;
                string[] VMajCombo = null;
                double[] VMajRatio = null;
                string[] VMinCombo = null;
                double[] VMinRatio = null;
                string[] ErrorSummary = null;
                string[] WarningSummary = null;

                int ret = _SapModel.DesignSteel.GetSummaryResults_3(
                    ref NumberItems,
                    ref FrameName,
                    ref FrameType,
                    ref DesignSect,
                    ref Status,
                    ref PMMCombo,
                    ref PMMRatio,
                    ref PRatio,
                    ref MMajRatio,
                    ref MMinRatio,
                    ref VMajCombo,
                    ref VMajRatio,
                    ref VMinCombo,
                    ref VMinRatio,
                    ref ErrorSummary,
                    ref WarningSummary
                );

                if (ret != 0)
                {
                    sb.AppendLine($"✗ Failed to get steel design results. Error code: {ret}");
                    csvData = "";
                    report = sb.ToString();
                    return false;
                }

                if (NumberItems == 0)
                {
                    sb.AppendLine("⚠ No steel frame elements found in design results.");
                    csvData = csv.ToString();
                    report = sb.ToString();
                    return true;
                }

                // Extract data for each frame
                for (int i = 0; i < NumberItems; i++)
                {
                    csv.AppendLine($"{FrameName[i]},{FrameType[i]},{DesignSect[i]},{Status[i]}," +
                                   $"{PMMCombo[i]},{PMMRatio[i]:F4},{PRatio[i]:F4},{MMajRatio[i]:F4},{MMinRatio[i]:F4}," +
                                   $"{VMajCombo[i]},{VMajRatio[i]:F4},{VMinCombo[i]},{VMinRatio[i]:F4}");
                }

                sb.AppendLine($"✓ Extracted {NumberItems} steel frame design results");

                // Get failed elements
                int NumberNotPassedItems = 0;
                string[] NotPassedFrames = null;

                ret = _SapModel.DesignSteel.VerifyPassed(ref NumberNotPassedItems, ref NotPassedFrames);

                if (ret == 0 && NumberNotPassedItems > 0)
                {
                    sb.AppendLine($"⚠ {NumberNotPassedItems} elements did NOT pass steel design check:");
                    for (int i = 0; i < Math.Min(5, NumberNotPassedItems); i++)
                    {
                        sb.AppendLine($"   - {NotPassedFrames[i]}");
                    }
                    if (NumberNotPassedItems > 5)
                    {
                        sb.AppendLine($"   ... and {NumberNotPassedItems - 5} more");
                    }
                }
                else if (NumberNotPassedItems == 0)
                {
                    sb.AppendLine("✓ All elements passed steel design check!");
                }

                csvData = csv.ToString();
                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ EXCEPTION: {ex.Message}");
                csvData = "";
                report = sb.ToString();
                return false;
            }
        }

        #endregion

        #region Concrete Frame Design

        /// <summary>
        /// Start concrete frame design check
        /// </summary>
        public bool StartConcreteDesign(out string report)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== STARTING CONCRETE FRAME DESIGN CHECK ===");

            try
            {
                // Check if analysis results are available
                int NumberResults = 0;
                int ret = _SapModel.Results.Setup.GetCaseStatus(ref NumberResults);

                if (NumberResults == 0)
                {
                    sb.AppendLine("⚠ No analysis results available. Running analysis first...");

                    ret = _SapModel.Analyze.RunAnalysis();
                    if (ret != 0)
                    {
                        sb.AppendLine("✗ Analysis failed. Cannot proceed with design check.");
                        report = sb.ToString();
                        return false;
                    }
                    sb.AppendLine("✓ Analysis completed");
                }
                else
                {
                    sb.AppendLine("✓ Analysis results are available");
                }

                // Start concrete design
                sb.AppendLine("Starting concrete frame design check...");
                ret = _SapModel.DesignConcrete.StartDesign();

                if (ret == 0)
                {
                    sb.AppendLine("✓ Concrete frame design check completed successfully!");
                    report = sb.ToString();
                    return true;
                }
                else
                {
                    sb.AppendLine($"✗ Concrete design failed with error code: {ret}");
                    report = sb.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ EXCEPTION: {ex.Message}");
                report = sb.ToString();
                return false;
            }
        }

        /// <summary>
        /// Extract concrete column design results
        /// </summary>
        public bool ExtractConcreteColumnResults(out string csvData, out string report)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder csv = new StringBuilder();

            sb.AppendLine("=== EXTRACTING CONCRETE COLUMN DESIGN RESULTS ===");

            try
            {
                // CSV Header
                csv.AppendLine("FrameName,Location,Option,PMMCombo,PMMArea,PMMRatio,VMajorCombo,AVMajor,VMinorCombo,AVMinor,ErrorSummary,WarningSummary");

                // Get summary results for columns
                int NumberItems = 0;
                string[] FrameName = null;
                double[] Location = null;
                int[] MyOption = null;
                string[] PMMCombo = null;
                double[] PMMArea = null;
                double[] PMMRatio = null;
                string[] VMajorCombo = null;
                double[] AVMajor = null;
                string[] VMinorCombo = null;
                double[] AVMinor = null;
                string[] ErrorSummary = null;
                string[] WarningSummary = null;

                int ret = _SapModel.DesignConcrete.GetSummaryResultsColumn(
                    ref NumberItems,
                    ref FrameName,
                    ref Location,
                    ref MyOption,
                    ref PMMCombo,
                    ref PMMArea,
                    ref PMMRatio,
                    ref VMajorCombo,
                    ref AVMajor,
                    ref VMinorCombo,
                    ref AVMinor,
                    ref ErrorSummary,
                    ref WarningSummary
                );

                if (ret != 0)
                {
                    sb.AppendLine($"✗ Failed to get concrete column results. Error code: {ret}");
                    csvData = "";
                    report = sb.ToString();
                    return false;
                }

                if (NumberItems == 0)
                {
                    sb.AppendLine("⚠ No concrete column elements found in design results.");
                    csvData = csv.ToString();
                    report = sb.ToString();
                    return true;
                }

                // Extract data
                for (int i = 0; i < NumberItems; i++)
                {
                    string optionText = MyOption[i] == 1 ? "Check" : "Design";

                    csv.AppendLine($"{FrameName[i]},{Location[i]:F4},{optionText}," +
                                   $"{PMMCombo[i]},{PMMArea[i]:F4},{PMMRatio[i]:F4}," +
                                   $"{VMajorCombo[i]},{AVMajor[i]:F4}," +
                                   $"{VMinorCombo[i]},{AVMinor[i]:F4}," +
                                   $"\"{ErrorSummary[i]}\",\"{WarningSummary[i]}\"");
                }

                sb.AppendLine($"✓ Extracted {NumberItems} concrete column design results");

                csvData = csv.ToString();
                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ EXCEPTION: {ex.Message}");
                csvData = "";
                report = sb.ToString();
                return false;
            }
        }

        /// <summary>
        /// Extract concrete beam design results
        /// </summary>
        public bool ExtractConcreteBeamResults(out string csvData, out string report)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder csv = new StringBuilder();

            sb.AppendLine("=== EXTRACTING CONCRETE BEAM DESIGN RESULTS ===");

            try
            {
                // CSV Header
                csv.AppendLine("FrameName,Location,TopCombo,TopArea,BotCombo,BotArea,VCombo,VRebar,ErrorSummary,WarningSummary");

                // Get summary results for beams
                int NumberItems = 0;
                string[] FrameName = null;
                double[] Location = null;
                string[] TopCombo = null;
                double[] TopArea = null;
                string[] BotCombo = null;
                double[] BotArea = null;
                string[] VCombo = null;
                double[] VRebar = null;
                string[] ErrorSummary = null;
                string[] WarningSummary = null;

                int ret = _SapModel.DesignConcrete.GetSummaryResultsBeam_2(
                    ref NumberItems,
                    ref FrameName,
                    ref Location,
                    ref TopCombo,
                    ref TopArea,
                    ref BotCombo,
                    ref BotArea,
                    ref VCombo,
                    ref VRebar,
                    ref ErrorSummary,
                    ref WarningSummary
                );

                if (ret != 0)
                {
                    sb.AppendLine($"✗ Failed to get concrete beam results. Error code: {ret}");
                    csvData = "";
                    report = sb.ToString();
                    return false;
                }

                if (NumberItems == 0)
                {
                    sb.AppendLine("⚠ No concrete beam elements found in design results.");
                    csvData = csv.ToString();
                    report = sb.ToString();
                    return true;
                }

                // Extract data
                for (int i = 0; i < NumberItems; i++)
                {
                    csv.AppendLine($"{FrameName[i]},{Location[i]:F4}," +
                                   $"{TopCombo[i]},{TopArea[i]:F4}," +
                                   $"{BotCombo[i]},{BotArea[i]:F4}," +
                                   $"{VCombo[i]},{VRebar[i]:F4}," +
                                   $"\"{ErrorSummary[i]}\",\"{WarningSummary[i]}\"");
                }

                sb.AppendLine($"✓ Extracted {NumberItems} concrete beam design results");

                csvData = csv.ToString();
                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ EXCEPTION: {ex.Message}");
                csvData = "";
                report = sb.ToString();
                return false;
            }
        }

        #endregion

        #region Composite Beam Design

        /// <summary>
        /// Start composite beam design check
        /// </summary>
        public bool StartCompositeBeamDesign(out string report)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== STARTING COMPOSITE BEAM DESIGN CHECK ===");

            try
            {
                // Check if results are available
                bool resultsAvailable = _SapModel.DesignCompositeBeam.GetResultsAvailable();
                if (!resultsAvailable)
                {
                    sb.AppendLine("⚠ No analysis results available. Running analysis first...");

                    int ret = _SapModel.Analyze.RunAnalysis();
                    if (ret != 0)
                    {
                        sb.AppendLine("✗ Analysis failed. Cannot proceed with design check.");
                        report = sb.ToString();
                        return false;
                    }
                    sb.AppendLine("✓ Analysis completed");
                }

                // Start composite beam design
                sb.AppendLine("Starting composite beam design check...");
                int result = _SapModel.DesignCompositeBeam.StartDesign();

                if (result == 0)
                {
                    sb.AppendLine("✓ Composite beam design check completed successfully!");
                    report = sb.ToString();
                    return true;
                }
                else
                {
                    sb.AppendLine($"✗ Composite beam design failed with error code: {result}");
                    report = sb.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ EXCEPTION: {ex.Message}");
                report = sb.ToString();
                return false;
            }
        }

        /// <summary>
        /// Extract composite beam design results
        /// </summary>
        public bool ExtractCompositeBeamResults(out string csvData, out string report)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder csv = new StringBuilder();

            sb.AppendLine("=== EXTRACTING COMPOSITE BEAM DESIGN RESULTS ===");

            try
            {
                // Check if results are available
                bool resultsAvailable = _SapModel.DesignCompositeBeam.GetResultsAvailable();
                if (!resultsAvailable)
                {
                    sb.AppendLine("✗ No composite beam design results available. Run design check first.");
                    csvData = "";
                    report = sb.ToString();
                    return false;
                }

                // CSV Header
                csv.AppendLine("FrameName,Location,PassFail,StrengthRatio,ShearRatio,DeflectionRatio,ControllingCombo,ErrorSummary,WarningSummary");

                // Get summary results
                int NumberItems = 0;
                string[] FrameName = null;
                double[] Location = null;
                int[] PassFail = null;
                string[] ComboStrength = null;
                double[] StrengthRatio = null;
                string[] ComboShear = null;
                double[] ShearRatio = null;
                string[] ComboDeflection = null;
                double[] DeflectionRatio = null;
                string[] ErrorSummary = null;
                string[] WarningSummary = null;

                int ret = _SapModel.DesignCompositeBeam.GetSummaryResults(
                    ref NumberItems,
                    ref FrameName,
                    ref Location,
                    ref PassFail,
                    ref ComboStrength,
                    ref StrengthRatio,
                    ref ComboShear,
                    ref ShearRatio,
                    ref ComboDeflection,
                    ref DeflectionRatio,
                    ref ErrorSummary,
                    ref WarningSummary
                );

                if (ret != 0)
                {
                    sb.AppendLine($"✗ Failed to get composite beam results. Error code: {ret}");
                    csvData = "";
                    report = sb.ToString();
                    return false;
                }

                if (NumberItems == 0)
                {
                    sb.AppendLine("⚠ No composite beam elements found in design results.");
                    csvData = csv.ToString();
                    report = sb.ToString();
                    return true;
                }

                int failedCount = 0;

                // Extract data
                for (int i = 0; i < NumberItems; i++)
                {
                    string passFailText = PassFail[i] == 1 ? "PASS" : "FAIL";
                    if (PassFail[i] != 1) failedCount++;

                    // Get maximum ratio
                    double maxRatio = Math.Max(Math.Max(StrengthRatio[i], ShearRatio[i]), DeflectionRatio[i]);
                    string controllingCombo = ComboStrength[i];

                    if (ShearRatio[i] >= maxRatio) controllingCombo = ComboShear[i];
                    if (DeflectionRatio[i] >= maxRatio) controllingCombo = ComboDeflection[i];

                    csv.AppendLine($"{FrameName[i]},{Location[i]:F4},{passFailText}," +
                                   $"{StrengthRatio[i]:F4},{ShearRatio[i]:F4},{DeflectionRatio[i]:F4}," +
                                   $"{controllingCombo}," +
                                   $"\"{ErrorSummary[i]}\",\"{WarningSummary[i]}\"");
                }

                sb.AppendLine($"✓ Extracted {NumberItems} composite beam design results");

                if (failedCount > 0)
                {
                    sb.AppendLine($"⚠ {failedCount} elements FAILED composite beam design check");
                }
                else
                {
                    sb.AppendLine("✓ All elements passed composite beam design check!");
                }

                csvData = csv.ToString();
                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ EXCEPTION: {ex.Message}");
                csvData = "";
                report = sb.ToString();
                return false;
            }
        }

        #endregion

        #region Slab Design

        /// <summary>
        /// Start slab design check
        /// </summary>
        public bool StartSlabDesign(out string report)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== STARTING SLAB DESIGN CHECK ===");

            try
            {
                // Check if analysis results are available
                int NumberResults = 0;
                int ret = _SapModel.Results.Setup.GetCaseStatus(ref NumberResults);

                if (NumberResults == 0)
                {
                    sb.AppendLine("⚠ No analysis results available. Running analysis first...");

                    ret = _SapModel.Analyze.RunAnalysis();
                    if (ret != 0)
                    {
                        sb.AppendLine("✗ Analysis failed. Cannot proceed with design check.");
                        report = sb.ToString();
                        return false;
                    }
                    sb.AppendLine("✓ Analysis completed");
                }

                // Start slab design
                sb.AppendLine("Starting slab design check...");
                ret = _SapModel.DesignConcreteSlab.StartDesign();

                if (ret == 0)
                {
                    sb.AppendLine("✓ Slab design check completed successfully!");
                    report = sb.ToString();
                    return true;
                }
                else
                {
                    sb.AppendLine($"✗ Slab design failed with error code: {ret}");
                    report = sb.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ EXCEPTION: {ex.Message}");
                report = sb.ToString();
                return false;
            }
        }

        /// <summary>
        /// Extract slab design results
        /// </summary>
        public bool ExtractSlabDesignResults(out string csvData, out string report)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder csv = new StringBuilder();

            sb.AppendLine("=== EXTRACTING SLAB DESIGN RESULTS ===");

            try
            {
                // CSV Header
                csv.AppendLine("StripName,Span,Location,Status,TopCombo,TopMoment,TopArea,BotCombo,BotMoment,BotArea,ShearCombo,ShearForce,ShearArea");

                // Get summary results
                int NumberItems = 0;
                string[] StripName = null;
                int[] SpanNumber = null;
                double[] Location = null;
                int[] Status = null;
                string[] TopCombo = null;
                double[] TopMoment = null;
                double[] TopArea = null;
                string[] BotCombo = null;
                double[] BotMoment = null;
                double[] BotArea = null;
                string[] ShearCombo = null;
                double[] ShearForce = null;
                double[] ShearArea = null;

                int ret = _SapModel.DesignConcreteSlab.GetSummaryResultsFlexureAndShear(
                    ref NumberItems,
                    ref StripName,
                    ref SpanNumber,
                    ref Location,
                    ref Status,
                    ref TopCombo,
                    ref TopMoment,
                    ref TopArea,
                    ref BotCombo,
                    ref BotMoment,
                    ref BotArea,
                    ref ShearCombo,
                    ref ShearForce,
                    ref ShearArea
                );

                if (ret != 0)
                {
                    sb.AppendLine($"✗ Failed to get slab design results. Error code: {ret}");
                    csvData = "";
                    report = sb.ToString();
                    return false;
                }

                if (NumberItems == 0)
                {
                    sb.AppendLine("⚠ No slab design results found.");
                    csvData = csv.ToString();
                    report = sb.ToString();
                    return true;
                }

                // Extract data
                for (int i = 0; i < NumberItems; i++)
                {
                    string statusText = Status[i] == 1 ? "OK" : "FAIL";

                    csv.AppendLine($"{StripName[i]},{SpanNumber[i]},{Location[i]:F4},{statusText}," +
                                   $"{TopCombo[i]},{TopMoment[i]:F4},{TopArea[i]:F4}," +
                                   $"{BotCombo[i]},{BotMoment[i]:F4},{BotArea[i]:F4}," +
                                   $"{ShearCombo[i]},{ShearForce[i]:F4},{ShearArea[i]:F4}");
                }

                sb.AppendLine($"✓ Extracted {NumberItems} slab design results");

                csvData = csv.ToString();
                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ EXCEPTION: {ex.Message}");
                csvData = "";
                report = sb.ToString();
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Save CSV data to file
        /// </summary>
        public bool SaveToFile(string data, string filePath, out string errorMessage)
        {
            try
            {
                System.IO.File.WriteAllText(filePath, data);
                errorMessage = "";
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        #endregion
    }
}
