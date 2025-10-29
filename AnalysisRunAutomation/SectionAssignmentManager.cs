using System;
using System.Collections.Generic;
using System.Linq;
using ETABSv1;

namespace ETABS_Plugin
{
    public class SectionAssignmentManager
    {
        private readonly cSapModel _SapModel;

        public SectionAssignmentManager(cSapModel sapModel) => _SapModel = sapModel;

        #region Frame Section Assignment

        /// <summary>
        /// Assigns sections to all frame objects based on their orientation and story
        /// </summary>
        public bool AssignFrameSectionsIntelligently(out string report)
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
                    // Get all frame objects
                    int nFrames = 0;
                    string[] frameNames = Array.Empty<string>();
                    int ret = _SapModel.FrameObj.GetNameList(ref nFrames, ref frameNames);

                    if (ret != 0 || nFrames == 0)
                    {
                        report = "No frame objects found.";
                        return false;
                    }

                    sb.AppendLine($"Found {nFrames} frame objects.");

                    int columnsAssigned = 0;
                    int beamsAssigned = 0;
                    int errors = 0;

                    foreach (var frameName in frameNames)
                    {
                        // Get frame end points
                        string pt1 = "", pt2 = "";
                        _SapModel.FrameObj.GetPoints(frameName, ref pt1, ref pt2);

                        // Get coordinates of both points
                        double x1 = 0, y1 = 0, z1 = 0;
                        double x2 = 0, y2 = 0, z2 = 0;
                        _SapModel.PointObj.GetCoordCartesian(pt1, ref x1, ref y1, ref z1);
                        _SapModel.PointObj.GetCoordCartesian(pt2, ref x2, ref y2, ref z2);

                        // Determine if vertical (column) or horizontal (beam)
                        double deltaZ = Math.Abs(z2 - z1);
                        double deltaXY = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

                        string sectionName;
                        if (deltaZ > deltaXY) // More vertical than horizontal = column
                        {
                            sectionName = "COL-400x400"; // Default column
                            columnsAssigned++;
                        }
                        else // More horizontal = beam
                        {
                            sectionName = "BEAM-300x600"; // Default beam
                            beamsAssigned++;
                        }

                        // Assign the section
                        int r = _SapModel.FrameObj.SetSection(frameName, sectionName);
                        if (r != 0) errors++;
                    }

                    sb.AppendLine($"✓ Assigned sections: {columnsAssigned} columns, {beamsAssigned} beams");
                    if (errors > 0) sb.AppendLine($"⚠ {errors} assignment errors");

                    report = sb.ToString();
                    return errors == 0;
                }
                finally
                {
                    _SapModel.SetPresentUnits(prev);
                }
            }
            catch (Exception ex)
            {
                report = "Exception in AssignFrameSectionsIntelligently:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Assigns a specific section to all frames at a given story elevation
        /// </summary>
        public bool AssignFrameSectionByStory(string storyName, string sectionName, bool columnsOnly, out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                // Get story elevation
                int nStories = 0;
                string[] storyNames = Array.Empty<string>();
                double[] elevations = Array.Empty<double>();
                double[] storyHeights = Array.Empty<double>();
                bool[] isMasterStory = Array.Empty<bool>();
                string[] similarToStory = Array.Empty<string>();
                bool[] spliceAbove = Array.Empty<bool>();
                double[] spliceHeight = Array.Empty<double>();

                _SapModel.Story.GetStories(ref nStories, ref storyNames, ref elevations,
                    ref storyHeights, ref isMasterStory, ref similarToStory, ref spliceAbove, ref spliceHeight);

                double targetZ = -1;
                for (int i = 0; i < nStories; i++)
                {
                    if (storyNames[i] == storyName)
                    {
                        targetZ = elevations[i];
                        break;
                    }
                }

                if (targetZ < 0)
                {
                    report = $"Story '{storyName}' not found.";
                    return false;
                }

                // Get all frames
                int nFrames = 0;
                string[] frameNames = Array.Empty<string>();
                _SapModel.FrameObj.GetNameList(ref nFrames, ref frameNames);

                int assigned = 0;
                foreach (var frameName in frameNames)
                {
                    // Check if frame is at this story
                    string pt1 = "", pt2 = "";
                    _SapModel.FrameObj.GetPoints(frameName, ref pt1, ref pt2);

                    double x1 = 0, y1 = 0, z1 = 0;
                    _SapModel.PointObj.GetCoordCartesian(pt1, ref x1, ref y1, ref z1);

                    if (Math.Abs(z1 - targetZ) < 100) // Within 100mm tolerance
                    {
                        if (columnsOnly)
                        {
                            // Check if vertical
                            double x2 = 0, y2 = 0, z2 = 0;
                            _SapModel.PointObj.GetCoordCartesian(pt2, ref x2, ref y2, ref z2);
                            double deltaZ = Math.Abs(z2 - z1);
                            double deltaXY = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

                            if (deltaZ > deltaXY)
                            {
                                _SapModel.FrameObj.SetSection(frameName, sectionName);
                                assigned++;
                            }
                        }
                        else
                        {
                            _SapModel.FrameObj.SetSection(frameName, sectionName);
                            assigned++;
                        }
                    }
                }

                sb.AppendLine($"✓ Assigned {sectionName} to {assigned} frames at {storyName}");
                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in AssignFrameSectionByStory:\r\n" + ex;
                return false;
            }
        }

        #endregion

        #region Area Section Assignment

        /// <summary>
        /// Assigns area properties to all area objects based on their type
        /// </summary>
        public bool AssignAreaPropertiesIntelligently(out string report)
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
                    // Get all area objects
                    int nAreas = 0;
                    string[] areaNames = Array.Empty<string>();
                    int ret = _SapModel.AreaObj.GetNameList(ref nAreas, ref areaNames);

                    if (ret != 0 || nAreas == 0)
                    {
                        report = "No area objects found.";
                        return false;
                    }

                    sb.AppendLine($"Found {nAreas} area objects.");

                    int slabsAssigned = 0;
                    int wallsAssigned = 0;
                    int errors = 0;

                    foreach (var areaName in areaNames)
                    {
                        // Get area corner points to determine orientation
                        int nPoints = 0;
                        string[] pointNames = Array.Empty<string>();
                        _SapModel.AreaObj.GetPoints(areaName, ref nPoints, ref pointNames);

                        if (nPoints < 3) continue;

                        // Get coordinates of first 3 points to determine if horizontal or vertical
                        double x1 = 0, y1 = 0, z1 = 0;
                        double x2 = 0, y2 = 0, z2 = 0;
                        double x3 = 0, y3 = 0, z3 = 0;
                        _SapModel.PointObj.GetCoordCartesian(pointNames[0], ref x1, ref y1, ref z1);
                        _SapModel.PointObj.GetCoordCartesian(pointNames[1], ref x2, ref y2, ref z2);
                        _SapModel.PointObj.GetCoordCartesian(pointNames[2], ref x3, ref y3, ref z3);

                        // Calculate normal vector using cross product
                        double v1x = x2 - x1, v1y = y2 - y1, v1z = z2 - z1;
                        double v2x = x3 - x1, v2y = y3 - y1, v2z = z3 - z1;

                        double nx = v1y * v2z - v1z * v2y;
                        double ny = v1z * v2x - v1x * v2z;
                        double nz = v1x * v2y - v1y * v2x;

                        double normalLength = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                        if (normalLength > 0)
                        {
                            nz = nz / normalLength; // Normalize Z component
                        }

                        string propertyName;
                        if (Math.Abs(nz) > 0.7) // More horizontal than vertical = slab
                        {
                            propertyName = "SLAB-200MM"; // Default slab
                            slabsAssigned++;
                        }
                        else // More vertical = wall
                        {
                            propertyName = "WALL-200MM"; // Default wall
                            wallsAssigned++;
                        }

                        // Assign the property
                        int r = _SapModel.AreaObj.SetProperty(areaName, propertyName);
                        if (r != 0) errors++;
                    }

                    sb.AppendLine($"✓ Assigned properties: {slabsAssigned} slabs, {wallsAssigned} walls");
                    if (errors > 0) sb.AppendLine($"⚠ {errors} assignment errors");

                    report = sb.ToString();
                    return errors == 0;
                }
                finally
                {
                    _SapModel.SetPresentUnits(prev);
                }
            }
            catch (Exception ex)
            {
                report = "Exception in AssignAreaPropertiesIntelligently:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Assigns a specific property to all areas at a given elevation (typically slabs)
        /// </summary>
        public bool AssignAreaPropertyByElevation(double elevation, string propertyName, double tolerance, out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                int nAreas = 0;
                string[] areaNames = Array.Empty<string>();
                _SapModel.AreaObj.GetNameList(ref nAreas, ref areaNames);

                int assigned = 0;
                foreach (var areaName in areaNames)
                {
                    int nPoints = 0;
                    string[] pointNames = Array.Empty<string>();
                    _SapModel.AreaObj.GetPoints(areaName, ref nPoints, ref pointNames);

                    if (nPoints == 0) continue;

                    // Check if any point is at target elevation
                    bool atElevation = false;
                    foreach (var pt in pointNames)
                    {
                        double x = 0, y = 0, z = 0;
                        _SapModel.PointObj.GetCoordCartesian(pt, ref x, ref y, ref z);
                        if (Math.Abs(z - elevation) <= tolerance)
                        {
                            atElevation = true;
                            break;
                        }
                    }

                    if (atElevation)
                    {
                        _SapModel.AreaObj.SetProperty(areaName, propertyName);
                        assigned++;
                    }
                }

                sb.AppendLine($"✓ Assigned {propertyName} to {assigned} areas at Z={elevation:0.0}mm");
                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in AssignAreaPropertyByElevation:\r\n" + ex;
                return false;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets a summary of current section assignments
        /// </summary>
        public bool GetAssignmentSummary(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                // Frame summary
                int nFrames = 0;
                string[] frameNames = Array.Empty<string>();
                _SapModel.FrameObj.GetNameList(ref nFrames, ref frameNames);

                var frameSections = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var frame in frameNames)
                {
                    string prop = string.Empty;
                    string sAuto = string.Empty;

                    // NOTE: 3-arg signature required in your build
                    int ret = _SapModel.FrameObj.GetSection(frame, ref prop, ref sAuto);
                    if (ret != 0) continue;

                    // If using Auto Select List, group under AUTO:<listname>
                    string key = !string.IsNullOrEmpty(sAuto)
                               ? $"AUTO:{sAuto}"
                               : (string.IsNullOrEmpty(prop) ? "<none>" : prop);

                    frameSections[key] = frameSections.TryGetValue(key, out var cnt) ? cnt + 1 : 1;
                }

                sb.AppendLine("=== FRAME SECTIONS ===");
                foreach (var kvp in frameSections.OrderByDescending(x => x.Value))
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value} frame(s)");

                // Area summary
                int nAreas = 0;
                string[] areaNames = Array.Empty<string>();
                _SapModel.AreaObj.GetNameList(ref nAreas, ref areaNames);

                var areaProps = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var area in areaNames)
                {
                    string prop = string.Empty;
                    int ret = _SapModel.AreaObj.GetProperty(area, ref prop);
                    if (ret != 0) continue;

                    string key = string.IsNullOrEmpty(prop) ? "<none>" : prop;
                    areaProps[key] = areaProps.TryGetValue(key, out var cnt) ? cnt + 1 : 1;
                }

                sb.AppendLine();
                sb.AppendLine("=== AREA PROPERTIES ===");
                foreach (var kvp in areaProps.OrderByDescending(x => x.Value))
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value} area(s)");

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in GetAssignmentSummary:\r\n" + ex;
                return false;
            }
        }

        #endregion
    }
}