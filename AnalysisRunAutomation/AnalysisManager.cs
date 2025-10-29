using System;
using System.Collections.Generic;
using System.Linq;
using ETABSv1;

namespace ETABS_Plugin
{
    public class AnalysisManager
    {
        private readonly cSapModel _SapModel;

        public AnalysisManager(cSapModel sapModel) => _SapModel = sapModel;

        #region Analysis Execution

        /// <summary>
        /// Creates the analysis model (mesh generation, assignments validation)
        /// </summary>
        public bool CreateAnalysisModel(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                sb.AppendLine("Creating analysis model...");

                int ret = _SapModel.Analyze.CreateAnalysisModel();

                if (ret == 0)
                {
                    sb.AppendLine("✓ Analysis model created successfully");
                    sb.AppendLine("  - Mesh elements generated");
                    sb.AppendLine("  - Assignments validated");
                    sb.AppendLine("  - Model ready for analysis");
                    report = sb.ToString();
                    return true;
                }
                else
                {
                    sb.AppendLine($"⚠ Analysis model creation returned error code: {ret}");
                    report = sb.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                report = "Exception in CreateAnalysisModel:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Runs the structural analysis
        /// </summary>
        public bool RunAnalysis(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                // Ensure model is unlocked
                bool isLocked = _SapModel.GetModelIsLocked();
                if (isLocked) _SapModel.SetModelIsLocked(false);

                sb.AppendLine("=== RUNNING ANALYSIS ===");
                sb.AppendLine("This may take several minutes depending on model size...\r\n");

                // Create analysis model first
                sb.AppendLine("Step 1: Creating analysis model...");
                int ret = _SapModel.Analyze.CreateAnalysisModel();

                if (ret != 0)
                {
                    sb.AppendLine($"⚠ Analysis model creation failed with code: {ret}");
                    report = sb.ToString();
                    return false;
                }
                sb.AppendLine("✓ Analysis model created\r\n");

                // Run the analysis
                sb.AppendLine("Step 2: Running analysis...");
                ret = _SapModel.Analyze.RunAnalysis();

                if (ret == 0)
                {
                    sb.AppendLine("✓ Analysis completed successfully!\r\n");

                    // Get analysis summary
                    if (GetAnalysisSummary(out string summary))
                    {
                        sb.AppendLine(summary);
                    }

                    report = sb.ToString();
                    return true;
                }
                else
                {
                    sb.AppendLine($"✗ Analysis failed with error code: {ret}\r\n");
                    sb.AppendLine("Common issues:");
                    sb.AppendLine("  - Unstable structure (missing supports)");
                    sb.AppendLine("  - Mechanism (insufficient restraints)");
                    sb.AppendLine("  - Numerical issues (very stiff/soft elements)");
                    sb.AppendLine("  - Missing load cases");
                    sb.AppendLine("\nCheck ETABS messages window for details.");

                    report = sb.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                report = "Exception in RunAnalysis:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Runs analysis for specific load cases only
        /// </summary>
        public bool RunAnalysisForCases(string[] caseNames, out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                sb.AppendLine($"Running analysis for {caseNames.Length} specific cases...");

                // Set cases to run
                foreach (var caseName in caseNames)
                {
                    _SapModel.Analyze.SetRunCaseFlag(caseName, true, true);
                }

                // Run analysis
                int ret = _SapModel.Analyze.RunAnalysis();

                if (ret == 0)
                {
                    sb.AppendLine("✓ Analysis completed for specified cases");
                    report = sb.ToString();
                    return true;
                }
                else
                {
                    sb.AppendLine($"✗ Analysis failed with error code: {ret}");
                    report = sb.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                report = "Exception in RunAnalysisForCases:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Deletes analysis results (clears previous analysis)
        /// </summary>
        public bool DeleteAnalysisResults(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                sb.AppendLine("Deleting previous analysis results...");

                int ret = _SapModel.Analyze.DeleteResults("", true); // Empty string = all cases

                if (ret == 0)
                {
                    sb.AppendLine("✓ Analysis results deleted");
                    report = sb.ToString();
                    return true;
                }
                else
                {
                    sb.AppendLine($"⚠ Delete results returned code: {ret}");
                    report = sb.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                report = "Exception in DeleteAnalysisResults:\r\n" + ex;
                return false;
            }
        }

        #endregion

        #region Analysis Status and Results

        /// <summary>
        /// Checks if analysis results are available
        /// </summary>
        public bool HasAnalysisResults(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                // Try to get any results - if we can, analysis has been run
                int numberResults = 0;
                string[] loadCase = Array.Empty<string>();
                string[] stepType = Array.Empty<string>();
                double[] stepNum = Array.Empty<double>();

                // Try to get joint displacements for any point as a test
                int nPoints = 0;
                string[] ptNames = Array.Empty<string>();
                _SapModel.PointObj.GetNameList(ref nPoints, ref ptNames);

                if (nPoints > 0)
                {
                    string[] obj = Array.Empty<string>();
                    string[] elm = Array.Empty<string>();
                    string[] pointElm = Array.Empty<string>();
                    double[] u1 = Array.Empty<double>();
                    double[] u2 = Array.Empty<double>();
                    double[] u3 = Array.Empty<double>();
                    double[] r1 = Array.Empty<double>();
                    double[] r2 = Array.Empty<double>();
                    double[] r3 = Array.Empty<double>();

                    int ret = _SapModel.Results.JointDispl(
                        ptNames[0], eItemTypeElm.ObjectElm,
                        ref numberResults, ref obj, ref elm,
                        ref loadCase, ref stepType, ref stepNum,
                        ref u1, ref u2, ref u3, ref r1, ref r2, ref r3);

                    if (ret == 0 && numberResults > 0)
                    {
                        sb.AppendLine("✓ Analysis results are available");
                        sb.AppendLine($"  Found results for {numberResults} load cases");
                        report = sb.ToString();
                        return true;
                    }
                }

                sb.AppendLine("✗ No analysis results found");
                sb.AppendLine("  Run analysis first");
                report = sb.ToString();
                return false;
            }
            catch (Exception ex)
            {
                sb.AppendLine("✗ Could not check for analysis results");
                sb.AppendLine("  Analysis likely not run yet");
                report = sb.ToString();
                return false;
            }
        }

        /// <summary>
        /// Gets a summary of analysis results
        /// </summary>
        public bool GetAnalysisSummary(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                sb.AppendLine("=== ANALYSIS SUMMARY ===");

                // Get load cases
                int nCases = 0;
                string[] caseNames = Array.Empty<string>();
                _SapModel.LoadCases.GetNameList(ref nCases, ref caseNames);

                if (nCases > 0)
                {
                    sb.AppendLine($"\nLoad Cases Analyzed: {nCases}");
                    foreach (var caseName in caseNames)
                    {
                        sb.AppendLine($"  • {caseName}");
                    }
                }

                // Get load combinations
                int nCombos = 0;
                string[] comboNames = Array.Empty<string>();
                _SapModel.RespCombo.GetNameList(ref nCombos, ref comboNames);

                if (nCombos > 0)
                {
                    sb.AppendLine($"\nLoad Combinations: {nCombos}");
                    // Show first 5 only to avoid clutter
                    for (int i = 0; i < Math.Min(5, nCombos); i++)
                    {
                        sb.AppendLine($"  • {comboNames[i]}");
                    }
                    if (nCombos > 5)
                        sb.AppendLine($"  ... and {nCombos - 5} more");
                }

                // Get model statistics
                int nPoints = 0, nFrames = 0, nAreas = 0;
                string[] dummy = Array.Empty<string>();
                _SapModel.PointObj.GetNameList(ref nPoints, ref dummy);
                _SapModel.FrameObj.GetNameList(ref nFrames, ref dummy);
                _SapModel.AreaObj.GetNameList(ref nAreas, ref dummy);

                sb.AppendLine($"\nModel Statistics:");
                sb.AppendLine($"  Joints: {nPoints}");
                sb.AppendLine($"  Frame Elements: {nFrames}");
                sb.AppendLine($"  Area Elements: {nAreas}");

                // Try to get mesh element count
                try
                {
                    int nMeshElements = _SapModel.AreaElm.Count();
                    if (nMeshElements > 0)
                        sb.AppendLine($"  Mesh Elements: {nMeshElements}");
                }
                catch { }

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in GetAnalysisSummary:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Gets basic reaction results at supports
        /// </summary>
        public bool GetBaseReactions(string loadCaseName, out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                sb.AppendLine($"=== BASE REACTIONS - {loadCaseName} ===\r\n");

                // Get all points
                int nPoints = 0;
                string[] ptNames = Array.Empty<string>();
                _SapModel.PointObj.GetNameList(ref nPoints, ref ptNames);

                double totalFx = 0, totalFy = 0, totalFz = 0;
                double totalMx = 0, totalMy = 0, totalMz = 0;
                int supportCount = 0;

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
                                    supportCount++;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (supportCount > 0)
                {
                    sb.AppendLine($"Total Base Reactions ({supportCount} supports):");
                    sb.AppendLine($"  ΣFx = {totalFx:0.00} N");
                    sb.AppendLine($"  ΣFy = {totalFy:0.00} N");
                    sb.AppendLine($"  ΣFz = {totalFz:0.00} N");
                    sb.AppendLine($"  ΣMx = {totalMx:0.00} N·mm");
                    sb.AppendLine($"  ΣMy = {totalMy:0.00} N·mm");
                    sb.AppendLine($"  ΣMz = {totalMz:0.00} N·mm");

                    // Convert to more readable units
                    sb.AppendLine($"\nIn kN and kN·m:");
                    sb.AppendLine($"  ΣFx = {totalFx / 1000:0.00} kN");
                    sb.AppendLine($"  ΣFy = {totalFy / 1000:0.00} kN");
                    sb.AppendLine($"  ΣFz = {totalFz / 1000:0.00} kN");
                    sb.AppendLine($"  ΣMx = {totalMx / 1e6:0.00} kN·m");
                    sb.AppendLine($"  ΣMy = {totalMy / 1e6:0.00} kN·m");
                    sb.AppendLine($"  ΣMz = {totalMz / 1e6:0.00} kN·m");
                }
                else
                {
                    sb.AppendLine("No support reactions found.");
                    sb.AppendLine("Check that:");
                    sb.AppendLine("  - Boundary conditions are applied");
                    sb.AppendLine("  - Analysis has been run");
                    sb.AppendLine($"  - Load case '{loadCaseName}' exists");
                }

                report = sb.ToString();
                return supportCount > 0;
            }
            catch (Exception ex)
            {
                report = "Exception in GetBaseReactions:\r\n" + ex;
                return false;
            }
        }

        #endregion

        #region Analysis Configuration

        /// <summary>
        /// Sets which load cases should be run
        /// </summary>
        public bool ConfigureRunCases(string[] casesToRun, bool runAll, out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                if (runAll)
                {
                    // Get all cases
                    int nCases = 0;
                    string[] allCases = Array.Empty<string>();
                    _SapModel.LoadCases.GetNameList(ref nCases, ref allCases);

                    foreach (var caseName in allCases)
                    {
                        _SapModel.Analyze.SetRunCaseFlag(caseName, true, true);
                    }

                    // Get all combos
                    int nCombos = 0;
                    string[] allCombos = Array.Empty<string>();
                    _SapModel.RespCombo.GetNameList(ref nCombos, ref allCombos);

                    foreach (var comboName in allCombos)
                    {
                        _SapModel.Analyze.SetRunCaseFlag(comboName, true, true);
                    }

                    sb.AppendLine($"✓ Configured to run all cases ({nCases} cases + {nCombos} combos)");
                }
                else if (casesToRun != null && casesToRun.Length > 0)
                {
                    foreach (var caseName in casesToRun)
                    {
                        _SapModel.Analyze.SetRunCaseFlag(caseName, true, true);
                    }

                    sb.AppendLine($"✓ Configured to run {casesToRun.Length} specific cases");
                }
                else
                {
                    sb.AppendLine("⚠ No cases specified to run");
                }

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in ConfigureRunCases:\r\n" + ex;
                return false;
            }
        }

        #endregion
    }
}