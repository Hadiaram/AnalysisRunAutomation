using System;
using System.Collections.Generic;
using System.Linq;
using ETABSv1;

namespace ETABS_Plugin
{
    public class DiaphragmManager
    {
        private readonly cSapModel _SapModel;

        public DiaphragmManager(cSapModel sapModel) => _SapModel = sapModel;

        #region Diaphragm Analysis

        /// <summary>
        /// Checks if an area object is at a specific elevation
        /// </summary>
        private bool IsAreaAtElevation(string areaName, double targetElevation, double tolerance)
        {
            try
            {
                int nPoints = 0;
                string[] pointNames = Array.Empty<string>();
                _SapModel.AreaObj.GetPoints(areaName, ref nPoints, ref pointNames);

                if (nPoints == 0) return false;

                foreach (var point in pointNames)
                {
                    double x = 0, y = 0, z = 0;
                    _SapModel.PointObj.GetCoordCartesian(point, ref x, ref y, ref z);

                    if (Math.Abs(z - targetElevation) <= tolerance)
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tallies area types at a specific story elevation
        /// </summary>
        private void TallyAreaTypeAtStory(string areaName, double storyElevation, double tolMm,
                                          ref int openingCount, ref int wallCount, ref double totalWallThickness, ref int slabLikeCount)
        {
            if (!IsAreaAtElevation(areaName, storyElevation, tolMm)) return;

            // Get property name
            string propName = string.Empty;
            int propRet = _SapModel.AreaObj.GetProperty(areaName, ref propName);
            if (propRet != 0 || string.IsNullOrEmpty(propName))
            {
                // Treat as an opening if no property
                openingCount++;
                return;
            }

            // Try to get wall properties
            eWallPropType wallType = eWallPropType.Specified;
            eShellType wallShell = eShellType.ShellThin;
            string wallMat = string.Empty;
            double wallThk = 0.0;
            int wColor = 0;
            string wNotes = string.Empty;
            string wGUID = string.Empty;

            int wRet = _SapModel.PropArea.GetWall(propName, ref wallType, ref wallShell, ref wallMat, ref wallThk, ref wColor, ref wNotes, ref wGUID);
            if (wRet == 0)
            {
                wallCount++;
                totalWallThickness += wallThk;
                return;
            }

            // Try to get slab properties
            eSlabType slabType = eSlabType.Slab;
            eShellType slabShell = eShellType.ShellThin;
            string slabMat = string.Empty;
            double slabThk = 0.0;
            int sColor = 0;
            string sNotes = string.Empty;
            string sGUID = string.Empty;

            int sRet = _SapModel.PropArea.GetSlab(propName, ref slabType, ref slabShell, ref slabMat, ref slabThk, ref sColor, ref sNotes, ref sGUID);
            if (sRet == 0)
            {
                slabLikeCount++;
                return;
            }
        }

        /// <summary>
        /// Analyzes a story to determine if diaphragm should be rigid or semi-rigid
        /// Returns true for semi-rigid, false for rigid
        /// </summary>
        private bool AnalyzeDiaphragmOption(string storyName, double storyElevation, double tolMm)
        {
            try
            {
                int nAreas = 0;
                string[] areaNames = Array.Empty<string>();
                _SapModel.AreaObj.GetNameList(ref nAreas, ref areaNames);

                if (nAreas == 0) return true; // Default to semi-rigid

                int openingCount = 0;
                int wallCount = 0;
                double totalWallThickness = 0.0;
                int slabLikeCount = 0;

                foreach (var a in areaNames)
                {
                    TallyAreaTypeAtStory(a, storyElevation, tolMm, ref openingCount, ref wallCount, ref totalWallThickness, ref slabLikeCount);
                }

                double openRatio = (slabLikeCount + wallCount) > 0 ? (double)openingCount / (slabLikeCount + wallCount) : 0.0;
                double avgWallThk = wallCount > 0 ? totalWallThickness / wallCount : 0.0;

                bool heavilyPerforated = openRatio > 0.15;       // >15% openings
                bool thinWalls = avgWallThk > 0 && avgWallThk < 200.0;  // <200 mm avg
                bool fewWalls = wallCount < 3;

                if (heavilyPerforated || thinWalls || fewWalls)
                    return true; // semi-rigid

                // Conservative default: semi-rigid
                return true;
            }
            catch
            {
                return true; // semi-rigid on error
            }
        }

        #endregion

        #region Diaphragm Creation and Assignment

        /// <summary>
        /// Creates diaphragms for all stories and assigns points
        /// </summary>
        public bool CreateAndAssignDiaphragms(out string report, double elevationTolerance = 50.0)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                // Ensure model is unlocked
                bool isLocked = _SapModel.GetModelIsLocked();
                if (isLocked) _SapModel.SetModelIsLocked(false);

                // Work in N-mm-°C
                eUnits prev = _SapModel.GetPresentUnits();
                _SapModel.SetPresentUnits(eUnits.N_mm_C);

                try
                {
                    // Get all stories with full parameter list
                    int nStories = 0;
                    string[] storyNames = Array.Empty<string>();
                    double[] elevations = Array.Empty<double>();
                    double[] storyHeights = Array.Empty<double>();
                    bool[] isMasterStory = Array.Empty<bool>();
                    string[] similarToStory = Array.Empty<string>();
                    bool[] spliceAbove = Array.Empty<bool>();
                    double[] spliceHeight = Array.Empty<double>();

                    int rs = _SapModel.Story.GetStories(
                        ref nStories,
                        ref storyNames,
                        ref elevations,
                        ref storyHeights,
                        ref isMasterStory,
                        ref similarToStory,
                        ref spliceAbove,
                        ref spliceHeight);

                    if (rs != 0 || nStories == 0)
                    {
                        report = "No stories found in model.";
                        return false;
                    }

                    sb.AppendLine($"Found {nStories} stories in model.");

                    // Get all points
                    int nPts = 0;
                    string[] ptNames = Array.Empty<string>();
                    rs = _SapModel.PointObj.GetNameList(ref nPts, ref ptNames);

                    if (rs != 0 || nPts == 0)
                    {
                        report = "No points found in model.";
                        return false;
                    }

                    sb.AppendLine($"Found {nPts} points in model.");

                    // Create diaphragms and assign points for each story
                    int totalAssigned = 0;
                    for (int i = 0; i < nStories; i++)
                    {
                        string sName = storyNames[i];
                        double zStory = elevations[i];
                        string diaphName = $"DIAPH_{sName.Replace(" ", "_")}";

                        // Analyze to determine rigid vs semi-rigid (true = semi-rigid)
                        bool isSemiRigid = AnalyzeDiaphragmOption(sName, zStory, elevationTolerance);

                        // Delete if exists, then create diaphragm
                        try { _SapModel.Diaphragm.Delete(diaphName); } catch { }

                        int rd = _SapModel.Diaphragm.SetDiaphragm(diaphName, isSemiRigid);
                        if (rd != 0)
                        {
                            sb.AppendLine($"⚠ Failed to define diaphragm {diaphName}");
                            continue;
                        }

                        // Assign all points at this elevation to the diaphragm
                        int assigned = 0;
                        foreach (var p in ptNames)
                        {
                            double x = 0, y = 0, z = 0;
                            _SapModel.PointObj.GetCoordCartesian(p, ref x, ref y, ref z);

                            if (Math.Abs(z - zStory) <= elevationTolerance)
                            {
                                int ra = _SapModel.PointObj.SetDiaphragm(p, eDiaphragmOption.DefinedDiaphragm, diaphName);
                                if (ra == 0) assigned++;
                            }
                        }

                        totalAssigned += assigned;
                        string optStr = isSemiRigid ? "Semi-Rigid" : "Rigid";
                        sb.AppendLine($"✓ {diaphName} ({optStr}): {assigned} joints @ Z={zStory:0.0} mm");
                    }

                    sb.AppendLine($"\nTotal joints assigned: {totalAssigned}");
                    report = sb.ToString();
                    return true;
                }
                finally
                {
                    // Restore units
                    _SapModel.SetPresentUnits(prev);
                }
            }
            catch (Exception ex)
            {
                report = "Exception in CreateAndAssignDiaphragms:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Creates diaphragms for all stories (without point assignment)
        /// </summary>
        public bool CreateDiaphragmsForStories(out string report, double elevationTolerance = 50.0)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                // Get all stories
                int nStories = 0;
                string[] storyNames = Array.Empty<string>();
                double[] elevations = Array.Empty<double>();
                double[] storyHeights = Array.Empty<double>();
                bool[] isMasterStory = Array.Empty<bool>();
                string[] similarToStory = Array.Empty<string>();
                bool[] spliceAbove = Array.Empty<bool>();
                double[] spliceHeight = Array.Empty<double>();

                int rs = _SapModel.Story.GetStories(
                    ref nStories,
                    ref storyNames,
                    ref elevations,
                    ref storyHeights,
                    ref isMasterStory,
                    ref similarToStory,
                    ref spliceAbove,
                    ref spliceHeight);

                if (rs != 0 || nStories == 0)
                {
                    report = "No stories found in model.";
                    return false;
                }

                // Create a diaphragm for each story
                for (int i = 0; i < nStories; i++)
                {
                    string sName = storyNames[i];
                    double zStory = elevations[i];
                    string diaphName = $"DIAPH_{sName.Replace(" ", "_")}";

                    bool isSemiRigid = AnalyzeDiaphragmOption(sName, zStory, elevationTolerance);

                    // Delete if exists
                    try { _SapModel.Diaphragm.Delete(diaphName); } catch { }

                    // Create diaphragm
                    int rd = _SapModel.Diaphragm.SetDiaphragm(diaphName, isSemiRigid);

                    string optStr = isSemiRigid ? "Semi-Rigid" : "Rigid";
                    if (rd == 0)
                        sb.AppendLine($"✓ Created {diaphName} ({optStr}) @ Z={zStory:0.0} mm");
                    else
                        sb.AppendLine($"✗ Failed to create {diaphName}");
                }

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in CreateDiaphragmsForStories:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Removes all diaphragm assignments from points
        /// </summary>
        public bool ClearAllDiaphragmAssignments(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                int nPts = 0;
                string[] ptNames = Array.Empty<string>();
                _SapModel.PointObj.GetNameList(ref nPts, ref ptNames);

                int cleared = 0;
                foreach (var p in ptNames)
                {
                    int r = _SapModel.PointObj.SetDiaphragm(p, eDiaphragmOption.Disconnect, "");
                    if (r == 0) cleared++;
                }

                sb.AppendLine($"Cleared diaphragm assignment from {cleared} joints.");
                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in ClearAllDiaphragmAssignments:\r\n" + ex;
                return false;
            }
        }

        #endregion
    }
}