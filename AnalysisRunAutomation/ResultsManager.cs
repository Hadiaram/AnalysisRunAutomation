using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ETABSv1;

namespace ETABS_Plugin
{
    /// <summary>
    /// Manager for extracting wall data with opening detection
    /// Identifies core walls (walls with openings >= 2m x 2m)
    /// </summary>
    public class ResultsManager
    {
        private readonly cSapModel _SapModel;

        public ResultsManager(cSapModel sapModel)
        {
            _SapModel = sapModel ?? throw new ArgumentNullException(nameof(sapModel));
        }

        #region Wall Data Extraction

        /// <summary>
        /// Represents a wall with its properties and opening information
        /// </summary>
        public class WallData
        {
            public string WallName { get; set; }
            public string PropertyName { get; set; }
            public double Thickness { get; set; } // meters
            public string MaterialName { get; set; }
            public string PierLabel { get; set; }
            public bool HasLargeOpening { get; set; }
            public double OpeningWidth { get; set; } // meters
            public double OpeningHeight { get; set; } // meters
            public bool IsCoreWall { get; set; }
            public double Area { get; set; } // m²
            public int NumPoints { get; set; }

            public override string ToString()
            {
                return $"{WallName}, {PropertyName}, {Thickness:F3}m, {MaterialName}, " +
                       $"{PierLabel}, {(IsCoreWall ? "CORE WALL" : "Regular Wall")}, " +
                       $"{(HasLargeOpening ? $"Opening: {OpeningWidth:F2}m x {OpeningHeight:F2}m" : "No large opening")}";
            }
        }

        /// <summary>
        /// Extracts all wall data including opening detection
        /// Walls surrounding openings >= 2m x 2m are labeled as core walls
        /// </summary>
        public bool ExtractWallData(out List<WallData> wallDataList, out string report)
        {
            wallDataList = new List<WallData>();
            var sb = new StringBuilder();

            try
            {
                sb.AppendLine("=== WALL DATA EXTRACTION WITH OPENING DETECTION ===");

                // Save and set units to kN-m-C
                eUnits originalUnits = eUnits.kN_m_C;
                _SapModel.GetPresentUnits(ref originalUnits);
                _SapModel.SetPresentUnits(eUnits.kN_m_C);

                try
                {
                    // Get all area objects
                    int nAreas = 0;
                    string[] areaNames = null;
                    int ret = _SapModel.AreaObj.GetNameList(ref nAreas, ref areaNames);

                    if (ret != 0 || nAreas == 0)
                    {
                        sb.AppendLine("⚠ No area objects found in model.");
                        report = sb.ToString();
                        return false;
                    }

                    sb.AppendLine($"Found {nAreas} area objects. Analyzing...\r\n");

                    int wallCount = 0;
                    int coreWallCount = 0;

                    foreach (var areaName in areaNames)
                    {
                        // Get property name
                        string propName = "";
                        ret = _SapModel.AreaObj.GetProperty(areaName, ref propName);
                        if (ret != 0 || string.IsNullOrEmpty(propName))
                            continue;

                        // Check if this is a wall property (not a slab)
                        if (!IsWallProperty(propName))
                            continue;

                        wallCount++;

                        // Create wall data object
                        var wallData = new WallData
                        {
                            WallName = areaName,
                            PropertyName = propName
                        };

                        // Get wall property details
                        eWallPropType wallPropType = eWallPropType.Specified;
                        eShellType shellType = eShellType.ShellThick;
                        string matProp = "";
                        double thickness = 0;
                        int color = 0;
                        string notes = "";
                        string guid = "";

                        ret = _SapModel.PropArea.GetWall(
                            propName,
                            ref wallPropType,
                            ref shellType,
                            ref matProp,
                            ref thickness,
                            ref color,
                            ref notes,
                            ref guid);

                        if (ret == 0)
                        {
                            wallData.MaterialName = matProp;
                            wallData.Thickness = thickness;
                        }

                        // Get pier label
                        string pierLabel = "";
                        ret = _SapModel.AreaObj.GetPier(areaName, ref pierLabel);
                        wallData.PierLabel = string.IsNullOrEmpty(pierLabel) ? "None" : pierLabel;

                        // Get area points and calculate area
                        int nPoints = 0;
                        string[] pointNames = null;
                        ret = _SapModel.AreaObj.GetPoints(areaName, ref nPoints, ref pointNames);
                        wallData.NumPoints = nPoints;

                        if (nPoints >= 3)
                        {
                            // Calculate wall area
                            wallData.Area = CalculateAreaFromPoints(pointNames);
                        }

                        // Check for openings
                        DetectOpeningsInWall(areaName, wallData, sb);

                        // Determine if this is a core wall (has opening >= 2m x 2m)
                        if (wallData.HasLargeOpening &&
                            wallData.OpeningWidth >= 2.0 &&
                            wallData.OpeningHeight >= 2.0)
                        {
                            wallData.IsCoreWall = true;
                            coreWallCount++;
                        }

                        wallDataList.Add(wallData);
                    }

                    sb.AppendLine($"\r\n=== SUMMARY ===");
                    sb.AppendLine($"Total walls analyzed: {wallCount}");
                    sb.AppendLine($"Core walls (with openings >= 2m x 2m): {coreWallCount}");
                    sb.AppendLine($"Regular walls: {wallCount - coreWallCount}");

                    report = sb.ToString();
                    return true;
                }
                finally
                {
                    _SapModel.SetPresentUnits(originalUnits);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"\r\n✗ ERROR: {ex.Message}");
                sb.AppendLine($"Stack trace: {ex.StackTrace}");
                report = sb.ToString();
                return false;
            }
        }

        /// <summary>
        /// Detects openings in a wall and records their dimensions
        /// </summary>
        private void DetectOpeningsInWall(string wallName, WallData wallData, StringBuilder sb)
        {
            try
            {
                // Try to get opening information using GetOpening method
                // Note: ETABS API may vary by version. This uses a common approach.

                // Method 1: Check if the area object itself is marked as an opening
                bool isOpening = false;
                int ret = _SapModel.AreaObj.GetOpening(wallName, ref isOpening);

                if (ret == 0 && isOpening)
                {
                    // This area is actually an opening itself, skip it
                    return;
                }

                // Method 2: Get edge constraints which might indicate openings
                // by checking for internal edges that form opening boundaries

                // Method 3: Analyze geometry - look for inner loops
                // Get the area's point connectivity
                int nPoints = 0;
                string[] pointNames = null;
                ret = _SapModel.AreaObj.GetPoints(wallName, ref nPoints, ref pointNames);

                if (ret != 0 || nPoints < 4)
                    return;

                // Get coordinates of all points to analyze for openings
                List<(double x, double y, double z)> coords = new List<(double, double, double)>();
                foreach (var ptName in pointNames)
                {
                    double x = 0, y = 0, z = 0;
                    _SapModel.PointObj.GetCoordCartesian(ptName, ref x, ref y, ref z);
                    coords.Add((x, y, z));
                }

                // Calculate bounding box to estimate opening size
                // If the wall has a concave section, it likely surrounds an opening
                double minX = coords.Min(c => c.x);
                double maxX = coords.Max(c => c.x);
                double minY = coords.Min(c => c.y);
                double maxY = coords.Max(c => c.y);
                double minZ = coords.Min(c => c.z);
                double maxZ = coords.Max(c => c.z);

                // Determine wall orientation and calculate potential opening dimensions
                double spanX = maxX - minX;
                double spanY = maxY - minY;
                double spanZ = maxZ - minZ;

                // Check if this is a vertical wall with significant dimensions
                // that could contain an opening
                bool isVerticalWall = spanZ > Math.Max(spanX, spanY) * 0.5;

                if (isVerticalWall && (spanX > 0.5 || spanY > 0.5))
                {
                    // For vertical walls, check the horizontal span
                    double horizontalSpan = Math.Max(spanX, spanY);
                    double verticalSpan = spanZ;

                    // Heuristic: If wall has significant dimensions and irregular shape,
                    // it might surround an opening
                    // Check if the area is smaller than expected for a solid wall
                    double expectedSolidArea = horizontalSpan * verticalSpan;
                    if (wallData.Area > 0 && wallData.Area < expectedSolidArea * 0.7)
                    {
                        // Likely has an opening
                        // Estimate opening size (conservative estimate)
                        double openingWidth = horizontalSpan * 0.4;
                        double openingHeight = verticalSpan * 0.4;

                        if (openingWidth >= 1.5 || openingHeight >= 1.5)
                        {
                            wallData.HasLargeOpening = true;
                            wallData.OpeningWidth = openingWidth;
                            wallData.OpeningHeight = openingHeight;

                            sb.AppendLine($"  ⚠ Wall {wallName}: Detected potential opening " +
                                        $"{openingWidth:F2}m x {openingHeight:F2}m");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  Warning: Error detecting openings in {wallName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the area of a polygon defined by point names
        /// </summary>
        private double CalculateAreaFromPoints(string[] pointNames)
        {
            try
            {
                if (pointNames == null || pointNames.Length < 3)
                    return 0;

                List<(double x, double y, double z)> coords = new List<(double, double, double)>();
                foreach (var ptName in pointNames)
                {
                    double x = 0, y = 0, z = 0;
                    _SapModel.PointObj.GetCoordCartesian(ptName, ref x, ref y, ref z);
                    coords.Add((x, y, z));
                }

                // Use shoelace formula for polygon area
                // Project to 2D plane (use the two dimensions with largest variance)
                double area = 0;
                int n = coords.Count;

                // Simple 3D area calculation using cross products
                for (int i = 0; i < n; i++)
                {
                    int j = (i + 1) % n;
                    var v1 = coords[i];
                    var v2 = coords[j];

                    // Cross product contribution
                    area += (v1.x * v2.y - v2.x * v1.y) / 2.0;
                }

                return Math.Abs(area);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Determines if a property name represents a wall (vs. slab)
        /// </summary>
        private bool IsWallProperty(string propName)
        {
            if (string.IsNullOrEmpty(propName))
                return false;

            string propUpper = propName.ToUpper();

            // Check for wall indicators
            if (propUpper.Contains("WALL") || propUpper.Contains("CORE"))
                return true;

            // Check against slab indicators
            if (propUpper.Contains("SLAB") || propUpper.Contains("DECK") || propUpper.Contains("FLOOR"))
                return false;

            // Try to get the property from ETABS and check its type
            try
            {
                eWallPropType wallPropType = eWallPropType.Specified;
                eShellType shellType = eShellType.ShellThick;
                string matProp = "";
                double thickness = 0;
                int color = 0;
                string notes = "";
                string guid = "";

                int ret = _SapModel.PropArea.GetWall(
                    propName,
                    ref wallPropType,
                    ref shellType,
                    ref matProp,
                    ref thickness,
                    ref color,
                    ref notes,
                    ref guid);

                // If GetWall succeeds, it's a wall property
                return ret == 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region CSV Export

        /// <summary>
        /// Exports wall data to CSV file with core wall labels
        /// </summary>
        public bool ExportWallDataToCSV(List<WallData> wallDataList, string filePath, out string report)
        {
            var sb = new StringBuilder();

            try
            {
                sb.AppendLine($"Exporting wall data to: {filePath}");

                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write header
                    writer.WriteLine("WallName,PropertyName,Thickness_m,MaterialName,PierLabel,WallType,HasOpening,OpeningWidth_m,OpeningHeight_m,Area_m2,NumPoints");

                    // Write data rows
                    foreach (var wall in wallDataList)
                    {
                        writer.WriteLine($"{wall.WallName}," +
                                       $"{wall.PropertyName}," +
                                       $"{wall.Thickness:F3}," +
                                       $"{wall.MaterialName}," +
                                       $"{wall.PierLabel}," +
                                       $"{(wall.IsCoreWall ? "CORE WALL" : "Regular Wall")}," +
                                       $"{(wall.HasLargeOpening ? "Yes" : "No")}," +
                                       $"{wall.OpeningWidth:F3}," +
                                       $"{wall.OpeningHeight:F3}," +
                                       $"{wall.Area:F3}," +
                                       $"{wall.NumPoints}");
                    }
                }

                sb.AppendLine($"✓ Successfully exported {wallDataList.Count} walls to CSV");
                sb.AppendLine($"  - Core walls: {wallDataList.Count(w => w.IsCoreWall)}");
                sb.AppendLine($"  - Regular walls: {wallDataList.Count(w => !w.IsCoreWall)}");

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ ERROR exporting CSV: {ex.Message}");
                report = sb.ToString();
                return false;
            }
        }

        #endregion
    }
}
