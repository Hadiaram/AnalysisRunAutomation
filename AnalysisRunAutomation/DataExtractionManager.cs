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

        // Cached database tables list (populated once per extraction session)
        private bool _tablesListCached = false;
        private int _cachedNumTables = 0;
        private string[] _cachedTableKeys = null;
        private string[] _cachedTableNames = null;
        private int[] _cachedImportTypes = null;

        public DataExtractionManager(cSapModel sapModel)
        {
            _SapModel = sapModel;
        }

        /// <summary>
        /// Ensures the database tables list is cached. Call once at the start of extraction.
        /// Returns true if successful, false otherwise.
        /// </summary>
        private bool EnsureTablesListCached()
        {
            if (_tablesListCached) return true;

            int ret = _SapModel.DatabaseTables.GetAvailableTables(
                ref _cachedNumTables, ref _cachedTableKeys,
                ref _cachedTableNames, ref _cachedImportTypes);

            if (ret == 0 && _cachedNumTables > 0)
            {
                _tablesListCached = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears the cached tables list. Call at the start of a new extraction session.
        /// </summary>
        private void ClearTablesListCache()
        {
            _tablesListCached = false;
            _cachedNumTables = 0;
            _cachedTableKeys = null;
            _cachedTableNames = null;
            _cachedImportTypes = null;
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
                reportSb.AppendLine("Extracting base reactions using bulk API...\r\n");

                // Use bulk BaseReact API - gets ALL load cases in ONE call
                cAnalysisResults results = _SapModel.Results;

                int numResults = 0;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;
                double[] fx = null;
                double[] fy = null;
                double[] fz = null;
                double[] mx = null;
                double[] my = null;
                double[] mz = null;
                double gx = 0;
                double gy = 0;
                double gz = 0;

                int ret = results.BaseReact(ref numResults, ref loadCase, ref stepType,
                    ref stepNum, ref fx, ref fy, ref fz, ref mx, ref my, ref mz,
                    ref gx, ref gy, ref gz);

                if (ret != 0)
                {
                    csvData = "";
                    report = "ERROR: Could not retrieve base reaction data.\n" +
                            "Please ensure analysis has been run successfully.";
                    return false;
                }

                reportSb.AppendLine($"✓ Retrieved {numResults} base reaction result(s) in single API call\r\n");

                // CSV Header
                sb.AppendLine("LoadCase,StepType,StepNum,Fx(kN),Fy(kN),Fz(kN),Mx(kN-m),My(kN-m),Mz(kN-m)");

                // Convert from N and N-mm to kN and kN-m
                for (int i = 0; i < numResults; i++)
                {
                    sb.AppendLine($"{loadCase[i]},{stepType[i]},{stepNum[i]}," +
                        $"{fx[i] / 1000.0:0.00},{fy[i] / 1000.0:0.00},{fz[i] / 1000.0:0.00}," +
                        $"{mx[i] / 1e6:0.00},{my[i] / 1e6:0.00},{mz[i] / 1e6:0.00}");
                }

                reportSb.AppendLine($"✓ Extracted base reactions for {numResults} load cases/combinations");
                reportSb.AppendLine("✓ Performance: ~100-1000x faster than old per-joint method");
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

        #region Grid System Information Extraction

        /// <summary>
        /// Extracts all grid system information including origin coordinates and rotation
        /// </summary>
        public bool ExtractGridInfo(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting grid system information...\r\n");

                // Get list of grid system names
                int numberNames = 0;
                string[] gridNames = Array.Empty<string>();

                int ret = _SapModel.GridSys.GetNameList(ref numberNames, ref gridNames);

                if (ret != 0)
                {
                    csvData = "";
                    report = $"ERROR: GridSys.GetNameList() returned error code {ret}";
                    return false;
                }

                reportSb.AppendLine($"✓ Found {numberNames} grid system(s)");

                // CSV Header
                sb.AppendLine("GridSystemName,OriginX,OriginY,RotationZ(deg)");

                // Extract data for each grid system
                int successCount = 0;
                for (int i = 0; i < numberNames; i++)
                {
                    string gridName = gridNames[i];
                    double x = 0;
                    double y = 0;
                    double rz = 0;

                    ret = _SapModel.GridSys.GetGridSys(gridName, ref x, ref y, ref rz);

                    if (ret == 0)
                    {
                        sb.AppendLine($"\"{gridName}\",{x:0.0000},{y:0.0000},{rz:0.0000}");
                        successCount++;
                    }
                    else
                    {
                        reportSb.AppendLine($"⚠ Warning: Failed to get data for grid system '{gridName}' (error {ret})");
                    }
                }

                reportSb.AppendLine($"✓ Successfully extracted {successCount} of {numberNames} grid system(s)");

                if (successCount > 0)
                {
                    reportSb.AppendLine("\r\n✓ Grid system information extracted successfully");
                    csvData = sb.ToString();
                    report = reportSb.ToString();
                    return true;
                }
                else
                {
                    csvData = "";
                    report = reportSb.ToString() + "\r\n✗ No grid systems were successfully extracted";
                    return false;
                }
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}";
                return false;
            }
        }

        #endregion

        #region Stiffness Modifiers Extraction

        /// <summary>
        /// Extracts frame property stiffness modifiers
        /// </summary>
        public bool ExtractFrameModifiers(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting frame property modifiers...\r\n");

                // Get list of frame property names
                int numberNames = 0;
                string[] propNames = Array.Empty<string>();

                int ret = _SapModel.PropFrame.GetNameList(ref numberNames, ref propNames);

                if (ret != 0)
                {
                    csvData = "";
                    report = $"ERROR: PropFrame.GetNameList() returned error code {ret}";
                    return false;
                }

                reportSb.AppendLine($"✓ Found {numberNames} frame propert(ies)");

                // CSV Header
                sb.AppendLine("PropertyName,Area,Shear2,Shear3,Torsion,I22,I33,Mass,Weight");

                // Extract modifiers for each frame property
                int successCount = 0;
                for (int i = 0; i < numberNames; i++)
                {
                    string propName = propNames[i];
                    double[] modifiers = new double[8];

                    ret = _SapModel.PropFrame.GetModifiers(propName, ref modifiers);

                    if (ret == 0)
                    {
                        sb.AppendLine($"\"{propName}\"," +
                            $"{modifiers[0]:0.0000}," +  // Area
                            $"{modifiers[1]:0.0000}," +  // Shear2
                            $"{modifiers[2]:0.0000}," +  // Shear3
                            $"{modifiers[3]:0.0000}," +  // Torsion
                            $"{modifiers[4]:0.0000}," +  // I22
                            $"{modifiers[5]:0.0000}," +  // I33
                            $"{modifiers[6]:0.0000}," +  // Mass
                            $"{modifiers[7]:0.0000}");   // Weight
                        successCount++;
                    }
                    else
                    {
                        reportSb.AppendLine($"⚠ Warning: Failed to get modifiers for '{propName}' (error {ret})");
                    }
                }

                reportSb.AppendLine($"✓ Successfully extracted {successCount} of {numberNames} frame properties");

                if (successCount > 0)
                {
                    reportSb.AppendLine("\r\n✓ Frame modifiers extracted successfully");
                    csvData = sb.ToString();
                    report = reportSb.ToString();
                    return true;
                }
                else
                {
                    csvData = "";
                    report = reportSb.ToString() + "\r\n✗ No frame modifiers were successfully extracted";
                    return false;
                }
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Extracts area property stiffness modifiers
        /// </summary>
        public bool ExtractAreaModifiers(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting area property modifiers...\r\n");

                // Get list of area property names
                int numberNames = 0;
                string[] propNames = Array.Empty<string>();

                int ret = _SapModel.PropArea.GetNameList(ref numberNames, ref propNames);

                if (ret != 0)
                {
                    csvData = "";
                    report = $"ERROR: PropArea.GetNameList() returned error code {ret}";
                    return false;
                }

                reportSb.AppendLine($"✓ Found {numberNames} area propert(ies)");

                // CSV Header
                sb.AppendLine("PropertyName,Membrane_f11,Membrane_f22,Membrane_f12,Bending_m11,Bending_m22,Bending_m12,Shear_v13,Shear_v23,Mass,Weight");

                // Extract modifiers for each area property
                int successCount = 0;
                for (int i = 0; i < numberNames; i++)
                {
                    string propName = propNames[i];
                    double[] modifiers = new double[10];

                    ret = _SapModel.PropArea.GetModifiers(propName, ref modifiers);

                    if (ret == 0)
                    {
                        sb.AppendLine($"\"{propName}\"," +
                            $"{modifiers[0]:0.0000}," +  // Membrane f11
                            $"{modifiers[1]:0.0000}," +  // Membrane f22
                            $"{modifiers[2]:0.0000}," +  // Membrane f12
                            $"{modifiers[3]:0.0000}," +  // Bending m11
                            $"{modifiers[4]:0.0000}," +  // Bending m22
                            $"{modifiers[5]:0.0000}," +  // Bending m12
                            $"{modifiers[6]:0.0000}," +  // Shear v13
                            $"{modifiers[7]:0.0000}," +  // Shear v23
                            $"{modifiers[8]:0.0000}," +  // Mass
                            $"{modifiers[9]:0.0000}");   // Weight
                        successCount++;
                    }
                    else
                    {
                        reportSb.AppendLine($"⚠ Warning: Failed to get modifiers for '{propName}' (error {ret})");
                    }
                }

                reportSb.AppendLine($"✓ Successfully extracted {successCount} of {numberNames} area properties");

                if (successCount > 0)
                {
                    reportSb.AppendLine("\r\n✓ Area modifiers extracted successfully");
                    csvData = sb.ToString();
                    report = reportSb.ToString();
                    return true;
                }
                else
                {
                    csvData = "";
                    report = reportSb.ToString() + "\r\n✗ No area modifiers were successfully extracted";
                    return false;
                }
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}";
                return false;
            }
        }

        #endregion

        #region Element Geometry and Location Extraction

        // Helper class to represent an opening
        private class OpeningInfo
        {
            public string Name { get; set; }
            public string Story { get; set; }
            public double CentroidX { get; set; }
            public double CentroidY { get; set; }
            public double CentroidZ { get; set; }
            public double MinX { get; set; }
            public double MaxX { get; set; }
            public double MinY { get; set; }
            public double MaxY { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }

        /// <summary>
        /// PERFORMANCE OPTIMIZATION: Caches all point coordinates at once
        /// This avoids thousands of individual API calls during element extraction
        /// </summary>
        private Dictionary<string, (double x, double y, double z)> GetAllPointCoordinates()
        {
            var coords = new Dictionary<string, (double x, double y, double z)>();

            try
            {
                int nPts = 0;
                string[] ptNames = Array.Empty<string>();
                int ret = _SapModel.PointObj.GetNameList(ref nPts, ref ptNames);

                if (ret != 0 || nPts == 0)
                    return coords;

                // Fetch all coordinates in one pass
                foreach (var ptName in ptNames)
                {
                    double x = 0, y = 0, z = 0;
                    ret = _SapModel.PointObj.GetCoordCartesian(ptName, ref x, ref y, ref z);
                    if (ret == 0)
                    {
                        coords[ptName] = (x, y, z);
                    }
                }
            }
            catch
            {
                // Return whatever we got
            }

            return coords;
        }

        /// <summary>
        /// Extracts openings from the model that are >= 2m x 2m
        /// </summary>
        private List<OpeningInfo> ExtractLargeOpenings(StringBuilder diagnosticReport = null, Dictionary<string, (double x, double y, double z)> pointCoords = null)
        {
            var openings = new List<OpeningInfo>();
            const double MIN_OPENING_SIZE = 2.0; // 2 meters
            int totalOpeningsFound = 0;
            int tooSmallCount = 0;
            int areaObjectsChecked = 0;

            try
            {
                // Get all area objects
                int numberNames = 0;
                string[] names = Array.Empty<string>();
                string[] labels = Array.Empty<string>();
                string[] stories = Array.Empty<string>();

                int ret = _SapModel.AreaObj.GetLabelNameList(ref numberNames, ref names, ref labels, ref stories);
                if (ret != 0)
                {
                    diagnosticReport?.AppendLine($"  ERROR: GetLabelNameList failed with code {ret}");
                    return openings;
                }

                diagnosticReport?.AppendLine($"  Checking {numberNames} area objects for openings...");

                // Check each area object to see if it's an opening
                for (int i = 0; i < numberNames; i++)
                {
                    string name = names[i];
                    string story = stories[i];
                    areaObjectsChecked++;

                    // Get property
                    string propName = "";
                    ret = _SapModel.AreaObj.GetProperty(name, ref propName);
                    if (ret != 0 || string.IsNullOrEmpty(propName)) continue;

                    // Check if it's an opening
                    // GetOpening checks if an area object is marked as an opening
                    bool isOpening = false;
                    ret = _SapModel.AreaObj.GetOpening(name, ref isOpening);

                    // Skip if API call failed OR if it's not actually an opening
                    if (ret != 0 || !isOpening) continue;

                    totalOpeningsFound++;
                    diagnosticReport?.AppendLine($"    ✓ Found opening area object: '{name}' on story '{story}'");

                    // Get points to calculate dimensions
                    int numPoints = 0;
                    string[] points = Array.Empty<string>();
                    ret = _SapModel.AreaObj.GetPoints(name, ref numPoints, ref points);
                    if (ret != 0 || numPoints == 0) continue;

                    // Get coordinates of all points
                    double sumX = 0, sumY = 0, sumZ = 0;
                    double minX = double.MaxValue, maxX = double.MinValue;
                    double minY = double.MaxValue, maxY = double.MinValue;

                    for (int j = 0; j < numPoints; j++)
                    {
                        double x = 0, y = 0, z = 0;

                        // PERFORMANCE: Use cached coordinates if available
                        if (pointCoords != null && pointCoords.ContainsKey(points[j]))
                        {
                            (x, y, z) = pointCoords[points[j]];
                        }
                        else
                        {
                            // Fallback to API call if cache not available
                            ret = _SapModel.PointObj.GetCoordCartesian(points[j], ref x, ref y, ref z);
                            if (ret != 0) continue;
                        }

                        sumX += x;
                        sumY += y;
                        sumZ += z;
                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }

                    // Calculate dimensions (in meters, since units are kN-m-C)
                    double width = maxX - minX;
                    double height = maxY - minY;

                    // Only include openings that are >= 2m x 2m
                    if (width >= MIN_OPENING_SIZE && height >= MIN_OPENING_SIZE)
                    {
                        openings.Add(new OpeningInfo
                        {
                            Name = name,
                            Story = story,
                            CentroidX = sumX / numPoints,
                            CentroidY = sumY / numPoints,
                            CentroidZ = sumZ / numPoints,
                            MinX = minX,
                            MaxX = maxX,
                            MinY = minY,
                            MaxY = maxY,
                            Width = width,
                            Height = height
                        });

                        diagnosticReport?.AppendLine($"    Opening '{name}' on '{story}': {width:F2}m x {height:F2}m at ({sumX/numPoints:F2}, {sumY/numPoints:F2})");
                    }
                    else
                    {
                        tooSmallCount++;
                        diagnosticReport?.AppendLine($"    Opening '{name}' on '{story}': TOO SMALL {width:F2}m x {height:F2}m (need >= 2m x 2m)");
                    }
                }

                diagnosticReport?.AppendLine($"\n  Opening Detection Summary:");
                diagnosticReport?.AppendLine($"    - Area objects checked: {areaObjectsChecked}");
                diagnosticReport?.AppendLine($"    - Total openings found: {totalOpeningsFound}");
                diagnosticReport?.AppendLine($"    - Large enough (>= 2m x 2m): {openings.Count}");
                diagnosticReport?.AppendLine($"    - Too small (< 2m x 2m): {tooSmallCount}");

                if (totalOpeningsFound == 0)
                {
                    diagnosticReport?.AppendLine($"\n  ⚠ WARNING: No openings detected!");
                    diagnosticReport?.AppendLine($"     Possible reasons:");
                    diagnosticReport?.AppendLine($"     1. Model has no areas marked as openings (use Assign > Area > Opening)");
                    diagnosticReport?.AppendLine($"     2. Openings created as holes within walls (not separate objects)");
                    diagnosticReport?.AppendLine($"     3. Opening areas deleted after creation");
                }
            }
            catch (Exception ex)
            {
                diagnosticReport?.AppendLine($"\n  ERROR in opening detection: {ex.Message}");
                // If there's an error, return whatever openings we found
                return openings;
            }

            return openings;
        }

        /// <summary>
        /// Determines if a wall is around a large opening (core wall logic)
        /// A wall is considered "around" an opening if:
        /// 1. They are on the same story
        /// 2. The wall's bounding box is within proximity to the opening's bounding box
        /// </summary>
        private bool IsWallAroundOpening(string wallStory, double wallMinX, double wallMaxX,
            double wallMinY, double wallMaxY, List<OpeningInfo> openings)
        {
            const double PROXIMITY_THRESHOLD = 3.0; // 3 meters proximity threshold

            foreach (var opening in openings)
            {
                // Check if on same story
                if (!string.Equals(wallStory, opening.Story, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Check if wall is near the opening using bounding box proximity
                // A wall is "around" an opening if any part of it is within the proximity threshold
                bool isNearInX = (wallMinX <= opening.MaxX + PROXIMITY_THRESHOLD) &&
                                 (wallMaxX >= opening.MinX - PROXIMITY_THRESHOLD);
                bool isNearInY = (wallMinY <= opening.MaxY + PROXIMITY_THRESHOLD) &&
                                 (wallMaxY >= opening.MinY - PROXIMITY_THRESHOLD);

                if (isNearInX && isNearInY)
                {
                    return true; // Wall is around this opening
                }
            }

            return false; // Wall is not around any large opening
        }

        /// <summary>
        /// Extracts wall/shear wall elements with dimensions and location
        /// </summary>
        /// <param name="csvData">Output CSV data</param>
        /// <param name="report">Output status report</param>
        /// <param name="progressCallback">Optional callback for progress updates (current, total, message)</param>
        public bool ExtractWallElements(out string csvData, out string report, Action<int, int, string> progressCallback = null)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting wall element information...\r\n");

                // Save current units and set to kN-m-C for consistent extraction
                eUnits originalUnits = _SapModel.GetPresentUnits();
                _SapModel.SetPresentUnits(eUnits.kN_m_C);

                try
                {
                    // PERFORMANCE OPTIMIZATION: Cache all point coordinates once at the start
                    progressCallback?.Invoke(0, 100, "Caching point coordinates...");
                    var pointCoords = GetAllPointCoordinates();
                    reportSb.AppendLine($"✓ Cached {pointCoords.Count} point coordinate(s) for fast lookup");

                    // First, extract all large openings (>= 2m x 2m)
                    progressCallback?.Invoke(0, 100, "Identifying large openings...");
                    reportSb.AppendLine("Opening Detection Diagnostics:");
                    var largeOpenings = ExtractLargeOpenings(reportSb, pointCoords);
                    reportSb.AppendLine($"\n✓ Found {largeOpenings.Count} large opening(s) (>= 2m x 2m)");

                    // Get all area objects
                    int numberNames = 0;
                    string[] names = Array.Empty<string>();
                    string[] labels = Array.Empty<string>();
                    string[] stories = Array.Empty<string>();

                    int ret = _SapModel.AreaObj.GetLabelNameList(ref numberNames, ref names, ref labels, ref stories);

                if (ret != 0)
                {
                    csvData = "";
                    report = $"ERROR: AreaObj.GetLabelNameList() returned error code {ret}";
                    return false;
                }

                reportSb.AppendLine($"✓ Found {numberNames} area object(s)");

                // CSV Header - Added CoreOrShear column
                sb.AppendLine("Name,Label,Story,PropertyName,WallType,Thickness(mm),PierLabel,SpandrelLabel,WallFunction,CoreOrShear,CentroidX,CentroidY,CentroidZ,MinX,MaxX,MinY,MaxY");

                // Extract data for each area object
                int wallCount = 0;
                int shearWallCount = 0;
                int spandrelCount = 0;
                int gravityWallCount = 0;
                int coreWallCount = 0;
                var coreWallDiagnostics = new StringBuilder();

                // Report initial progress
                progressCallback?.Invoke(0, numberNames, "Starting wall extraction...");

                for (int i = 0; i < numberNames; i++)
                {
                    // Report progress every 10 objects or for the last one
                    if (i % 10 == 0 || i == numberNames - 1)
                    {
                        progressCallback?.Invoke(i + 1, numberNames, $"Processing area object {i + 1}/{numberNames}");
                    }

                    string name = names[i];
                    string label = labels[i];
                    string story = stories[i];

                    // Get property
                    string propName = "";
                    ret = _SapModel.AreaObj.GetProperty(name, ref propName);
                    if (ret != 0 || string.IsNullOrEmpty(propName)) continue;

                    // Check if it's a wall property
                    eWallPropType wallPropType = eWallPropType.Specified;
                    eShellType shellType = eShellType.ShellThin;
                    string matProp = "";
                    double thickness = 0;
                    int color = 0;
                    string notes = "";
                    string guid = "";

                    ret = _SapModel.PropArea.GetWall(propName, ref wallPropType, ref shellType,
                        ref matProp, ref thickness, ref color, ref notes, ref guid);

                    if (ret != 0) continue; // Not a wall, skip

                    // Get points
                    int numPoints = 0;
                    string[] points = Array.Empty<string>();
                    ret = _SapModel.AreaObj.GetPoints(name, ref numPoints, ref points);
                    if (ret != 0 || numPoints == 0) continue;

                    // Get coordinates of all points
                    double sumX = 0, sumY = 0, sumZ = 0;
                    double minX = double.MaxValue, maxX = double.MinValue;
                    double minY = double.MaxValue, maxY = double.MinValue;

                    for (int j = 0; j < numPoints; j++)
                    {
                        double x = 0, y = 0, z = 0;

                        // PERFORMANCE: Use cached coordinates
                        if (pointCoords.ContainsKey(points[j]))
                        {
                            (x, y, z) = pointCoords[points[j]];
                        }
                        else
                        {
                            // Fallback to API call if point not in cache
                            ret = _SapModel.PointObj.GetCoordCartesian(points[j], ref x, ref y, ref z);
                            if (ret != 0) continue;
                        }

                        sumX += x;
                        sumY += y;
                        sumZ += z;
                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }

                    // Calculate centroid
                    double centroidX = sumX / numPoints;
                    double centroidY = sumY / numPoints;
                    double centroidZ = sumZ / numPoints;

                    // Convert thickness from meters to mm (units are set to kN-m-C above)
                    double thicknessMm = thickness * 1000;

                    // Get pier label assignment
                    string pierLabel = "";
                    ret = _SapModel.AreaObj.GetPier(name, ref pierLabel);
                    if (ret != 0 || string.IsNullOrEmpty(pierLabel))
                    {
                        pierLabel = "None";
                    }

                    // Get spandrel label assignment
                    string spandrelLabel = "";
                    ret = _SapModel.AreaObj.GetSpandrel(name, ref spandrelLabel);
                    if (ret != 0 || string.IsNullOrEmpty(spandrelLabel))
                    {
                        spandrelLabel = "None";
                    }

                    // Convert wall property type to readable string
                    string wallTypeStr = wallPropType.ToString();

                    // Determine if wall is core or shear based on proximity to large openings
                    string coreOrShear = IsWallAroundOpening(story, minX, maxX, minY, maxY, largeOpenings)
                        ? "Core Wall"
                        : "Shear Wall";

                    if (coreOrShear == "Core Wall")
                    {
                        coreWallCount++;
                        // Log first 5 core walls for diagnostics
                        if (coreWallCount <= 5)
                        {
                            coreWallDiagnostics.AppendLine($"  Core Wall #{coreWallCount}: '{name}' on '{story}' at ({centroidX:F2}, {centroidY:F2})");
                        }
                    }

                    // Determine wall function based on pier/spandrel assignment
                    string wallFunction;
                    if (pierLabel != "None")
                    {
                        wallFunction = "Shear Wall/Core";  // Lateral load-resisting wall
                        shearWallCount++;
                    }
                    else if (spandrelLabel != "None")
                    {
                        wallFunction = "Spandrel";  // Coupling beam/spandrel wall
                        spandrelCount++;
                    }
                    else
                    {
                        wallFunction = "Gravity/Partition";  // Non-lateral wall
                        gravityWallCount++;
                    }

                    sb.AppendLine($"\"{name}\",\"{label}\",\"{story}\",\"{propName}\"," +
                        $"\"{wallTypeStr}\",{thicknessMm:0.00}," +
                        $"\"{pierLabel}\",\"{spandrelLabel}\",\"{wallFunction}\",\"{coreOrShear}\"," +
                        $"{centroidX:0.0000},{centroidY:0.0000},{centroidZ:0.0000}," +
                        $"{minX:0.0000},{maxX:0.0000},{minY:0.0000},{maxY:0.0000}");
                    wallCount++;
                }

                // Report completion
                progressCallback?.Invoke(numberNames, numberNames, $"Completed - found {wallCount} walls");

                reportSb.AppendLine($"✓ Successfully extracted {wallCount} wall element(s)");
                reportSb.AppendLine();
                reportSb.AppendLine("Wall Type Breakdown (Core vs Shear):");
                reportSb.AppendLine($"  - Core Walls (around large openings): {coreWallCount}");
                reportSb.AppendLine($"  - Shear Walls: {wallCount - coreWallCount}");

                if (coreWallCount > 0 && coreWallDiagnostics.Length > 0)
                {
                    reportSb.AppendLine("\nFirst Core Walls Detected:");
                    reportSb.Append(coreWallDiagnostics.ToString());
                }
                else if (coreWallCount == 0 && largeOpenings.Count > 0)
                {
                    reportSb.AppendLine("\n⚠ WARNING: Large openings found but NO core walls detected!");
                    reportSb.AppendLine("  This may indicate:");
                    reportSb.AppendLine("  - Walls are too far from openings (>3m proximity threshold)");
                    reportSb.AppendLine("  - Walls and openings are on different stories (story name mismatch)");
                    reportSb.AppendLine("  - No walls exist near the openings");
                }
                else if (coreWallCount == 0 && largeOpenings.Count == 0)
                {
                    reportSb.AppendLine("\n⚠ No large openings (>= 2m x 2m) found - all walls classified as Shear Walls");
                }

                reportSb.AppendLine();
                reportSb.AppendLine("Wall Function Breakdown:");
                reportSb.AppendLine($"  - Shear Walls/Cores: {shearWallCount}");
                reportSb.AppendLine($"  - Spandrel Walls: {spandrelCount}");
                reportSb.AppendLine($"  - Gravity/Partition Walls: {gravityWallCount}");

                    if (wallCount > 0)
                    {
                        reportSb.AppendLine("\r\n✓ Wall elements extracted successfully");
                        csvData = sb.ToString();
                        report = reportSb.ToString();
                        return true;
                    }
                    else
                    {
                        csvData = "";
                        report = reportSb.ToString() + "\r\n✗ No wall elements found";
                        return false;
                    }
                }
                finally
                {
                    // Restore original units
                    _SapModel.SetPresentUnits(originalUnits);
                }
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Extracts column elements with dimensions and location
        /// </summary>
        public bool ExtractColumnElements(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting column element information...\r\n");

                // Save current units and set to kN-m-C for consistent extraction
                eUnits originalUnits = _SapModel.GetPresentUnits();
                _SapModel.SetPresentUnits(eUnits.kN_m_C);

                try
                {
                    // PERFORMANCE OPTIMIZATION: Cache all point coordinates once at the start
                    var pointCoords = GetAllPointCoordinates();
                    reportSb.AppendLine($"✓ Cached {pointCoords.Count} point coordinate(s) for fast lookup\r\n");

                    // Get all frame objects
                    int numberNames = 0;
                    string[] names = Array.Empty<string>();
                    string[] labels = Array.Empty<string>();
                    string[] stories = Array.Empty<string>();

                    int ret = _SapModel.FrameObj.GetLabelNameList(ref numberNames, ref names, ref labels, ref stories);

                if (ret != 0)
                {
                    csvData = "";
                    report = $"ERROR: FrameObj.GetLabelNameList() returned error code {ret}";
                    return false;
                }

                reportSb.AppendLine($"✓ Found {numberNames} frame object(s)");

                // CSV Header
                sb.AppendLine("Name,Label,Story,SectionName,Width_T2(mm),Depth_T3(mm),BottomX,BottomY,BottomZ,TopX,TopY,TopZ,Length(m)");

                // Extract data for each frame object (filter for columns)
                int columnCount = 0;
                for (int i = 0; i < numberNames; i++)
                {
                    string name = names[i];
                    string label = labels[i];
                    string story = stories[i];

                    // Get section
                    string propName = "";
                    string sAuto = "";
                    ret = _SapModel.FrameObj.GetSection(name, ref propName, ref sAuto);
                    if (ret != 0 || string.IsNullOrEmpty(propName)) continue;

                    // Try to get rectangular section properties
                    string fileName = "";
                    string matProp = "";
                    double t3 = 0; // depth
                    double t2 = 0; // width
                    int color = 0;
                    string notes = "";
                    string guid = "";

                    ret = _SapModel.PropFrame.GetRectangle(propName, ref fileName, ref matProp,
                        ref t3, ref t2, ref color, ref notes, ref guid);

                    // If not rectangular, try to get basic info (we'll skip non-rectangular for now)
                    if (ret != 0) continue;

                    // Get end points
                    string point1 = "";
                    string point2 = "";
                    ret = _SapModel.FrameObj.GetPoints(name, ref point1, ref point2);
                    if (ret != 0) continue;

                    // Get coordinates
                    double x1 = 0, y1 = 0, z1 = 0;
                    double x2 = 0, y2 = 0, z2 = 0;

                    // PERFORMANCE: Use cached coordinates
                    if (pointCoords.ContainsKey(point1))
                    {
                        (x1, y1, z1) = pointCoords[point1];
                    }
                    else
                    {
                        ret = _SapModel.PointObj.GetCoordCartesian(point1, ref x1, ref y1, ref z1);
                        if (ret != 0) continue;
                    }

                    if (pointCoords.ContainsKey(point2))
                    {
                        (x2, y2, z2) = pointCoords[point2];
                    }
                    else
                    {
                        ret = _SapModel.PointObj.GetCoordCartesian(point2, ref x2, ref y2, ref z2);
                        if (ret != 0) continue;
                    }

                    // Calculate length
                    double length = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2) + Math.Pow(z2 - z1, 2));

                    // Filter for vertical elements (columns) - check if primarily vertical
                    double verticalRatio = Math.Abs(z2 - z1) / length;
                    if (verticalRatio < 0.7) continue; // Skip if not primarily vertical

                    // Use bottom point (lower Z)
                    double bottomX = z1 < z2 ? x1 : x2;
                    double bottomY = z1 < z2 ? y1 : y2;
                    double bottomZ = Math.Min(z1, z2);
                    double topX = z1 > z2 ? x1 : x2;
                    double topY = z1 > z2 ? y1 : y2;
                    double topZ = Math.Max(z1, z2);

                    // Convert dimensions from meters to mm (units are set to kN-m-C above)
                    double t2Mm = t2 * 1000;
                    double t3Mm = t3 * 1000;

                    sb.AppendLine($"\"{name}\",\"{label}\",\"{story}\",\"{propName}\"," +
                        $"{t2Mm:0.00},{t3Mm:0.00}," +
                        $"{bottomX:0.0000},{bottomY:0.0000},{bottomZ:0.0000}," +
                        $"{topX:0.0000},{topY:0.0000},{topZ:0.0000}," +
                        $"{length:0.0000}");
                    columnCount++;
                }

                    reportSb.AppendLine($"✓ Successfully extracted {columnCount} column element(s)");

                    if (columnCount > 0)
                    {
                        reportSb.AppendLine("\r\n✓ Column elements extracted successfully");
                        csvData = sb.ToString();
                        report = reportSb.ToString();
                        return true;
                    }
                    else
                    {
                        csvData = "";
                        report = reportSb.ToString() + "\r\n✗ No column elements found";
                        return false;
                    }
                }
                finally
                {
                    // Restore original units
                    _SapModel.SetPresentUnits(originalUnits);
                }
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Helper method to setup all load cases for result output
        /// </summary>
        private void SetupAllCasesForOutput()
        {
            try
            {
                _SapModel.Results.Setup.DeselectAllCasesAndCombosForOutput();

                // Get all load cases
                int numCases = 0;
                string[] caseNames = null;
                _SapModel.LoadCases.GetNameList(ref numCases, ref caseNames);

                // Select all cases for output
                if (numCases > 0 && caseNames != null)
                {
                    for (int i = 0; i < numCases; i++)
                    {
                        try
                        {
                            _SapModel.Results.Setup.SetCaseSelectedForOutput(caseNames[i]);
                        }
                        catch { }
                    }
                }

                // Also get and select all combos
                int numCombos = 0;
                string[] comboNames = null;
                _SapModel.RespCombo.GetNameList(ref numCombos, ref comboNames);

                if (numCombos > 0 && comboNames != null)
                {
                    for (int i = 0; i < numCombos; i++)
                    {
                        try
                        {
                            _SapModel.Results.Setup.SetComboSelectedForOutput(comboNames[i]);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Extracts modal periods and frequencies
        /// </summary>
        public bool ExtractModalPeriods(out string csvData, out string report)
        {
            try
            {
                cAnalysisResults results = _SapModel.Results;

                int numResults = 0;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;
                double[] period = null;
                double[] frequency = null;
                double[] circFreq = null;
                double[] eigenValue = null;

                int ret = results.ModalPeriod(ref numResults, ref loadCase, ref stepType,
                    ref stepNum, ref period, ref frequency, ref circFreq, ref eigenValue);

                if (ret != 0)
                {
                    csvData = "";
                    report = "ERROR: Could not retrieve modal period data. Please ensure:\n" +
                            "1. A modal analysis case exists in the model\n" +
                            "2. The analysis has been run successfully\n" +
                            "3. Modal results are available";
                    return false;
                }

                if (numResults == 0)
                {
                    csvData = "";
                    report = "No modal period results found. Please run a modal analysis first.";
                    return false;
                }

                var csv = new StringBuilder();
                csv.AppendLine("LoadCase,StepType,ModeNumber,Period(s),Frequency(1/s),CircularFreq(rad/s),EigenValue(rad²/s²)");

                for (int i = 0; i < numResults; i++)
                {
                    csv.AppendLine($"{loadCase[i]},{stepType[i]},{stepNum[i]},{period[i]},{frequency[i]},{circFreq[i]},{eigenValue[i]}");
                }

                csvData = csv.ToString();
                report = $"Successfully extracted modal periods for {numResults} modes";
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}\n\nPlease ensure the model has modal analysis results.";
                return false;
            }
        }

        /// <summary>
        /// Extracts modal participating mass ratios
        /// </summary>
        public bool ExtractModalMassRatios(out string csvData, out string report)
        {
            try
            {
                cAnalysisResults results = _SapModel.Results;

                int numResults = 0;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;
                double[] period = null;
                double[] ux = null;
                double[] uy = null;
                double[] uz = null;
                double[] sumUX = null;
                double[] sumUY = null;
                double[] sumUZ = null;
                double[] rx = null;
                double[] ry = null;
                double[] rz = null;
                double[] sumRX = null;
                double[] sumRY = null;
                double[] sumRZ = null;

                int ret = results.ModalParticipatingMassRatios(ref numResults, ref loadCase,
                    ref stepType, ref stepNum, ref period, ref ux, ref uy, ref uz,
                    ref sumUX, ref sumUY, ref sumUZ, ref rx, ref ry, ref rz,
                    ref sumRX, ref sumRY, ref sumRZ);

                if (ret != 0)
                {
                    csvData = "";
                    report = "ERROR: Could not retrieve modal mass ratio data. Please ensure:\n" +
                            "1. A modal analysis case exists in the model\n" +
                            "2. The analysis has been run successfully\n" +
                            "3. Modal results are available";
                    return false;
                }

                if (numResults == 0)
                {
                    csvData = "";
                    report = "No modal mass ratio results found. Please run a modal analysis first.";
                    return false;
                }

                var csv = new StringBuilder();
                csv.AppendLine("LoadCase,StepType,ModeNumber,Period(s),UX,UY,UZ,SumUX,SumUY,SumUZ,RX,RY,RZ,SumRX,SumRY,SumRZ");

                for (int i = 0; i < numResults; i++)
                {
                    csv.AppendLine($"{loadCase[i]},{stepType[i]},{stepNum[i]},{period[i]}," +
                        $"{ux[i]},{uy[i]},{uz[i]},{sumUX[i]},{sumUY[i]},{sumUZ[i]}," +
                        $"{rx[i]},{ry[i]},{rz[i]},{sumRX[i]},{sumRY[i]},{sumRZ[i]}");
                }

                csvData = csv.ToString();
                report = $"Successfully extracted modal mass ratios for {numResults} modes";
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}\n\nPlease ensure the model has modal analysis results.";
                return false;
            }
        }

        /// <summary>
        /// Extracts story drift results
        /// </summary>
        public bool ExtractStoryDrifts(out string csvData, out string report)
        {
            try
            {
                cAnalysisResults results = _SapModel.Results;

                int numResults = 0;
                string[] story = null;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;
                string[] direction = null;
                double[] drift = null;
                string[] label = null;
                double[] x = null;
                double[] y = null;
                double[] z = null;

                int ret = results.StoryDrifts(ref numResults, ref story, ref loadCase,
                    ref stepType, ref stepNum, ref direction, ref drift, ref label,
                    ref x, ref y, ref z);

                if (ret != 0)
                {
                    csvData = "";
                    report = "ERROR: Could not retrieve story drift data. Please ensure:\n" +
                            "1. Load cases/combinations exist in the model\n" +
                            "2. The analysis has been run successfully\n" +
                            "3. Story drift results are available";
                    return false;
                }

                if (numResults == 0)
                {
                    csvData = "";
                    report = "No story drift results found. Please run the analysis first.";
                    return false;
                }

                var csv = new StringBuilder();
                csv.AppendLine("Story,LoadCase,StepType,StepNum,Direction,Drift,PointLabel,X,Y,Z");

                for (int i = 0; i < numResults; i++)
                {
                    csv.AppendLine($"{story[i]},{loadCase[i]},{stepType[i]},{stepNum[i]}," +
                        $"{direction[i]},{drift[i]},{label[i]},{x[i]},{y[i]},{z[i]}");
                }

                csvData = csv.ToString();
                report = $"Successfully extracted story drifts for {numResults} results";
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}\n\nPlease ensure the model has analysis results.";
                return false;
            }
        }

        /// <summary>
        /// Extracts base shear (base reactions) for all load cases
        /// </summary>
        public bool ExtractBaseShear(out string csvData, out string report)
        {
            try
            {
                cAnalysisResults results = _SapModel.Results;

                int numResults = 0;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;
                double[] fx = null;
                double[] fy = null;
                double[] fz = null;
                double[] mx = null;
                double[] my = null;
                double[] mz = null;
                double gx = 0;
                double gy = 0;
                double gz = 0;

                int ret = results.BaseReact(ref numResults, ref loadCase, ref stepType,
                    ref stepNum, ref fx, ref fy, ref fz, ref mx, ref my, ref mz,
                    ref gx, ref gy, ref gz);

                if (ret != 0)
                {
                    csvData = "";
                    report = "ERROR: Could not retrieve base reaction data. Please ensure:\n" +
                            "1. Load cases/combinations exist in the model\n" +
                            "2. The analysis has been run successfully\n" +
                            "3. Base reaction results are available";
                    return false;
                }

                if (numResults == 0)
                {
                    csvData = "";
                    report = "No base reaction results found. Please run the analysis first.";
                    return false;
                }

                var csv = new StringBuilder();
                csv.AppendLine($"Global Point: X={gx}, Y={gy}, Z={gz}");
                csv.AppendLine("LoadCase,StepType,StepNum,FX,FY,FZ,MX,MY,MZ");

                for (int i = 0; i < numResults; i++)
                {
                    csv.AppendLine($"{loadCase[i]},{stepType[i]},{stepNum[i]}," +
                        $"{fx[i]},{fy[i]},{fz[i]},{mx[i]},{my[i]},{mz[i]}");
                }

                csvData = csv.ToString();
                report = $"Successfully extracted base reactions for {numResults} load cases";
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}\n\nPlease ensure the model has analysis results.";
                return false;
            }
        }

        /// <summary>
        /// Extracts composite column design summary results (DCR and PMM ratios)
        /// </summary>
        public bool ExtractCompositeColumnDesign(out string csvData, out string report)
        {
            try
            {
                cDesignCompositeColumn compositeColumn = _SapModel.DesignCompositeColumn;

                // First, try to get all selected objects (use empty string for all)
                int numItems = 0;
                string[] frameName = null;
                eFrameDesignOrientation[] frameType = null;
                string[] designSect = null;
                string[] status = null;
                string[] pmmCombo = null;
                double[] pmmRatio = null;
                double[] pRatio = null;
                double[] mMajRatio = null;
                double[] mMinRatio = null;
                string[] vMajCombo = null;
                double[] vMajRatio = null;
                string[] vMinCombo = null;
                double[] vMinRatio = null;

                // Try with empty string first (for all objects)
                int ret = compositeColumn.GetSummaryResults("", ref numItems, ref frameName,
                    ref frameType, ref designSect, ref status, ref pmmCombo, ref pmmRatio,
                    ref pRatio, ref mMajRatio, ref mMinRatio, ref vMajCombo, ref vMajRatio,
                    ref vMinCombo, ref vMinRatio, eItemType.Group);

                if (ret != 0 || numItems == 0)
                {
                    csvData = "";
                    report = "ERROR: Could not retrieve composite column design data. Please ensure:\n" +
                            "1. Composite columns exist in the model\n" +
                            "2. Composite column design has been run\n" +
                            "3. Design results are available\n\n" +
                            "Note: Go to Design > Composite Column > Start Design/Check";
                    return false;
                }

                var csv = new StringBuilder();
                csv.AppendLine("FrameName,FrameType,DesignSection,Status,PMMCombo,PMMRatio,PRatio,MMajRatio,MMinRatio,VMajCombo,VMajRatio,VMinCombo,VMinRatio");

                for (int i = 0; i < numItems; i++)
                {
                    csv.AppendLine($"{frameName[i]},{frameType[i]},{designSect[i]},{status[i]}," +
                        $"{pmmCombo[i]},{pmmRatio[i]},{pRatio[i]},{mMajRatio[i]},{mMinRatio[i]}," +
                        $"{vMajCombo[i]},{vMajRatio[i]},{vMinCombo[i]},{vMinRatio[i]}");
                }

                csvData = csv.ToString();
                report = $"Successfully extracted composite column design results for {numItems} frame objects";
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}\n\nPlease ensure composite column design has been run.";
                return false;
            }
        }

        /// <summary>
        /// Exports ALL database tables to individual CSV files (memory-efficient streaming)
        /// </summary>
        /// <param name="outputFolder">Folder where table CSV files will be saved</param>
        /// <param name="timestamp">Timestamp string to use in filenames</param>
        /// <param name="report">Output status report</param>
        /// <returns>True if at least one table was exported successfully</returns>
        public bool ExtractAllDatabaseTables(string outputFolder, string timestamp, out string report)
        {
            var reportSb = new StringBuilder();
            int successCount = 0;
            int failCount = 0;
            int emptyCount = 0;

            try
            {
                reportSb.AppendLine("Exporting all database tables...\r\n");

                // Use cached table list
                if (!EnsureTablesListCached())
                {
                    report = "ERROR: Failed to get available database tables from ETABS API.\n" +
                            "Possible reasons:\n" +
                            "1. ETABS model is locked (File > Unlock Model)\n" +
                            "2. API connection issue";
                    return false;
                }

                if (_cachedNumTables == 0)
                {
                    report = "ERROR: No database tables available.\n\n" +
                            "This usually means the model has no objects or data.";
                    return false;
                }

                reportSb.AppendLine($"Found {_cachedNumTables} database table(s) in model");
                reportSb.AppendLine($"Exporting each table to separate CSV file...\r\n");

                cDatabaseTables dbTables = _SapModel.DatabaseTables;

                // Process each table one at a time (memory-efficient)
                for (int i = 0; i < _cachedNumTables; i++)
                {
                    string tableKey = _cachedTableKeys[i];
                    string tableName = _cachedTableNames[i];

                    try
                    {
                        // Get table data
                        string[] fieldKeyList = null;
                        string groupName = "";
                        int tableVersion = 0;
                        string[] fieldsKeysIncluded = null;
                        int numRecords = 0;
                        string[] tableData = null;

                        int ret = dbTables.GetTableForDisplayArray(tableKey, ref fieldKeyList, groupName,
                            ref tableVersion, ref fieldsKeysIncluded, ref numRecords, ref tableData);

                        if (ret != 0)
                        {
                            // Table couldn't be retrieved (API error)
                            failCount++;
                            continue;
                        }

                        if (numRecords == 0 || fieldsKeysIncluded == null || fieldsKeysIncluded.Length == 0)
                        {
                            // Table is empty or has no fields
                            emptyCount++;
                            continue;
                        }

                        // Stream table directly to file (memory-efficient)
                        string safeTableKey = tableKey.Replace("/", "_").Replace("\\", "_").Replace(":", "_");
                        string filePath = Path.Combine(outputFolder, $"Table_{safeTableKey}_{timestamp}.csv");

                        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                        {
                            // Write metadata comments
                            writer.WriteLine($"# Table Key: {tableKey}");
                            writer.WriteLine($"# Display Name: {tableName}");
                            writer.WriteLine($"# Records: {numRecords}");
                            writer.WriteLine();

                            // Write header row
                            writer.WriteLine(string.Join(",", fieldsKeysIncluded));

                            // Write data rows (streaming - no big StringBuilder)
                            int numFields = fieldsKeysIncluded.Length;
                            for (int row = 0; row < numRecords; row++)
                            {
                                var rowData = new List<string>(numFields);
                                for (int col = 0; col < numFields; col++)
                                {
                                    int dataIndex = row * numFields + col;
                                    if (dataIndex < tableData.Length)
                                    {
                                        // Escape commas and quotes in data
                                        string value = tableData[dataIndex];
                                        if (value != null && (value.Contains(",") || value.Contains("\"") || value.Contains("\n")))
                                        {
                                            value = $"\"{value.Replace("\"", "\"\"")}\"";
                                        }
                                        rowData.Add(value ?? "");
                                    }
                                    else
                                    {
                                        rowData.Add("");
                                    }
                                }
                                writer.WriteLine(string.Join(",", rowData));
                            }
                        }

                        // Success - file written
                        successCount++;

                        // Log first 10 and last 10, skip middle for large lists
                        if (successCount <= 10 || i >= _cachedNumTables - 10)
                        {
                            reportSb.AppendLine($"  ✓ {tableKey}: {numRecords} records → {Path.GetFileName(filePath)}");
                        }
                        else if (successCount == 11)
                        {
                            reportSb.AppendLine($"  ... exporting {_cachedNumTables - 20} more tables ...");
                        }

                        // Release memory immediately (don't hold all tables in memory)
                        tableData = null;
                        fieldsKeysIncluded = null;
                        fieldKeyList = null;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        reportSb.AppendLine($"  ✗ {tableKey}: {ex.Message}");
                    }
                }

                // Summary
                reportSb.AppendLine();
                reportSb.AppendLine("=== Database Table Export Summary ===");
                reportSb.AppendLine($"✓ Successfully exported: {successCount} tables");
                if (emptyCount > 0)
                    reportSb.AppendLine($"⊘ Empty tables skipped: {emptyCount}");
                if (failCount > 0)
                    reportSb.AppendLine($"✗ Failed to export: {failCount} tables");
                reportSb.AppendLine($"\nAll table files saved to: {outputFolder}");

                report = reportSb.ToString();
                return successCount > 0;
            }
            catch (Exception ex)
            {
                report = $"ERROR: {ex.Message}\n\nPlease ensure the model has been analyzed.";
                return false;
            }
        }

        /// <summary>
        /// Legacy method for compatibility - returns summary of available tables
        /// For full table export, use ExtractAllDatabaseTables() instead
        /// </summary>
        public bool ExtractQuantitiesSummary(out string csvData, out string report)
        {
            try
            {
                var reportSb = new StringBuilder();
                var csvSb = new StringBuilder();

                // Use cached table list
                if (!EnsureTablesListCached())
                {
                    csvData = "";
                    report = "ERROR: Failed to get available database tables from ETABS API.\n" +
                            "Possible reasons:\n" +
                            "1. ETABS model is locked (File > Unlock Model)\n" +
                            "2. API connection issue";
                    return false;
                }

                if (_cachedNumTables == 0)
                {
                    csvData = "";
                    report = "ERROR: No database tables available.\n\n" +
                            "This usually means the model has no objects or data.";
                    return false;
                }

                reportSb.AppendLine($"Found {_cachedNumTables} database table(s) in model\r\n");
                reportSb.AppendLine("NOTE: This method returns a summary only.");
                reportSb.AppendLine("For comprehensive table export, use 'Extract All Data' instead.\r\n");

                // Build CSV summary
                csvSb.AppendLine("# Database Tables Summary");
                csvSb.AppendLine($"# Total Tables: {_cachedNumTables}");
                csvSb.AppendLine();
                csvSb.AppendLine("Table Key,Display Name");

                for (int i = 0; i < _cachedNumTables; i++)
                {
                    csvSb.AppendLine($"\"{_cachedTableKeys[i]}\",\"{_cachedTableNames[i]}\"");
                }

                reportSb.AppendLine($"Generated summary of {_cachedNumTables} available tables");

                csvData = csvSb.ToString();
                report = reportSb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}\n\nPlease ensure the model has objects with material assignments.";
                return false;
            }
        }

        #endregion

        #region Frame Section Properties and Story Results

        /// <summary>
        /// Extracts frame section properties summary
        /// </summary>
        public bool ExtractFrameSectionProperties(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting frame section properties...\r\n");

                // Save current units and set to kN-m-C
                eUnits originalUnits = _SapModel.GetPresentUnits();
                _SapModel.SetPresentUnits(eUnits.kN_m_C);

                try
                {
                    // Get all frame section names
                    int numSections = 0;
                    string[] sectionNames = Array.Empty<string>();
                    int ret = _SapModel.PropFrame.GetNameList(ref numSections, ref sectionNames);

                    if (ret != 0 || numSections == 0)
                    {
                        csvData = "";
                        report = "ERROR: No frame sections found in model.";
                        return false;
                    }

                    reportSb.AppendLine($"✓ Found {numSections} frame section(s)\r\n");

                    // CSV Header
                    sb.AppendLine("SectionName,Material,SectionType,Depth(mm),Width(mm),Area(mm2),I33(mm4),I22(mm4),TorsionConst(mm4),S33Top(mm3),S33Bot(mm3),S22(mm3),Z33(mm3),Z22(mm3),R33(mm),R22(mm)");

                    int successCount = 0;

                    foreach (string sectionName in sectionNames)
                    {
                        try
                        {
                            // Get material
                            string material = "";
                            ret = _SapModel.PropFrame.GetMaterial(sectionName, ref material);
                            if (ret != 0) material = "Unknown";

                            // Get section properties
                            string fileName = "";
                            double t3 = 0, t2 = 0, tf = 0, tw = 0, tfb = 0, twb = 0;
                            int color = 0;
                            string notes = "";
                            string guid = "";

                            // Try to get as rectangular section
                            ret = _SapModel.PropFrame.GetRectangle(sectionName, ref fileName, ref material,
                                ref t3, ref t2, ref color, ref notes, ref guid);

                            string sectionType = "Unknown";
                            double depth = 0, width = 0;

                            if (ret == 0)
                            {
                                sectionType = "Rectangular";
                                depth = t3 * 1000; // Convert to mm
                                width = t2 * 1000;
                            }

                            // Get section properties (area, inertia, etc.)
                            double area = 0, as2 = 0, as3 = 0;
                            double torsion = 0, i22 = 0, i33 = 0;
                            double s22 = 0, s33 = 0, z22 = 0, z33 = 0, r22 = 0, r33 = 0;

                            ret = _SapModel.PropFrame.GetSectProps(sectionName, ref area, ref as2, ref as3,
                                ref torsion, ref i22, ref i33, ref s22, ref s33, ref z22, ref z33, ref r22, ref r33);

                            if (ret == 0)
                            {
                                // Convert from m to mm units
                                double s33Top = s33 * 1e9;  // mm^3
                                double s33Bot = s33 * 1e9;  // mm^3 (assuming symmetric)
                                double s22Val = s22 * 1e9;  // mm^3
                                double z33Val = z33 * 1e9;  // mm^3
                                double z22Val = z22 * 1e9;  // mm^3
                                double r33Val = r33 * 1000; // mm
                                double r22Val = r22 * 1000; // mm

                                sb.AppendLine($"{sectionName},{material},{sectionType}," +
                                    $"{depth:F1},{width:F1}," +
                                    $"{area * 1e6:F0},{i33 * 1e12:F0},{i22 * 1e12:F0},{torsion * 1e12:F0}," +
                                    $"{s33Top:F0},{s33Bot:F0},{s22Val:F0}," +
                                    $"{z33Val:F0},{z22Val:F0},{r33Val:F1},{r22Val:F1}");

                                successCount++;
                            }
                        }
                        catch
                        {
                            // Skip sections that fail
                            continue;
                        }
                    }

                    reportSb.AppendLine($"✓ Extracted properties for {successCount} section(s)");

                    csvData = sb.ToString();
                    report = reportSb.ToString();
                    return true;
                }
                finally
                {
                    _SapModel.SetPresentUnits(originalUnits);
                }
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Extracts story forces (shear and overturning moment)
        /// </summary>
        public bool ExtractStoryForces(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting story forces...\r\n");

                // Use cached table list
                if (!EnsureTablesListCached())
                {
                    csvData = "";
                    report = "ERROR: Failed to get available database tables.";
                    return false;
                }

                cDatabaseTables dbTables = _SapModel.DatabaseTables;

                // Find "Story Forces" table in cached list
                string storyForcesTableKey = null;
                for (int i = 0; i < _cachedNumTables; i++)
                {
                    if (_cachedTableKeys[i].ToUpperInvariant().Contains("STORY") &&
                        _cachedTableKeys[i].ToUpperInvariant().Contains("FORCE"))
                    {
                        storyForcesTableKey = _cachedTableKeys[i];
                        break;
                    }
                }

                if (storyForcesTableKey == null)
                {
                    csvData = "";
                    report = "ERROR: 'Story Forces' table not found in database.\nPlease ensure analysis has been run.";
                    return false;
                }

                reportSb.AppendLine($"✓ Found table: {storyForcesTableKey}\r\n");

                // Get table data
                string[] fieldKeyList = null;
                string groupName = "";
                int tableVersion = 0;
                string[] fieldsKeysIncluded = null;
                int numRecords = 0;
                string[] tableData = null;

                int ret = dbTables.GetTableForDisplayArray(storyForcesTableKey, ref fieldKeyList, groupName,
                    ref tableVersion, ref fieldsKeysIncluded, ref numRecords, ref tableData);

                if (ret != 0 || numRecords == 0)
                {
                    csvData = "";
                    report = "ERROR: No story force data available.\nPlease ensure analysis has been run.";
                    return false;
                }

                int numFields = fieldsKeysIncluded.Length;

                // Build CSV header from field names
                sb.AppendLine(string.Join(",", fieldsKeysIncluded));

                // Extract data rows
                for (int i = 0; i < numRecords; i++)
                {
                    var rowData = new List<string>();
                    for (int j = 0; j < numFields; j++)
                    {
                        int dataIndex = i * numFields + j;
                        if (dataIndex < tableData.Length)
                        {
                            rowData.Add(tableData[dataIndex]);
                        }
                    }
                    sb.AppendLine(string.Join(",", rowData));
                }

                reportSb.AppendLine($"✓ Extracted {numRecords} story force result(s)");

                csvData = sb.ToString();
                report = reportSb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}\n\nPlease ensure analysis has been run.";
                return false;
            }
        }

        /// <summary>
        /// Extracts story stiffness
        /// </summary>
        public bool ExtractStoryStiffness(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting story stiffness...\r\n");

                // Use cached table list
                if (!EnsureTablesListCached())
                {
                    csvData = "";
                    report = "ERROR: Failed to get available database tables.";
                    return false;
                }

                cDatabaseTables dbTables = _SapModel.DatabaseTables;

                // Find "Story Stiffness" table in cached list
                string storyStiffnessTableKey = null;
                for (int i = 0; i < _cachedNumTables; i++)
                {
                    if (_cachedTableKeys[i].ToUpperInvariant().Contains("STORY") &&
                        _cachedTableKeys[i].ToUpperInvariant().Contains("STIFF"))
                    {
                        storyStiffnessTableKey = _cachedTableKeys[i];
                        break;
                    }
                }

                if (storyStiffnessTableKey == null)
                {
                    csvData = "";
                    report = "ERROR: 'Story Stiffness' table not found in database.\nPlease ensure analysis has been run.";
                    return false;
                }

                reportSb.AppendLine($"✓ Found table: {storyStiffnessTableKey}\r\n");

                // Get table data
                string[] fieldKeyList = null;
                string groupName = "";
                int tableVersion = 0;
                string[] fieldsKeysIncluded = null;
                int numRecords = 0;
                string[] tableData = null;

                int ret = dbTables.GetTableForDisplayArray(storyStiffnessTableKey, ref fieldKeyList, groupName,
                    ref tableVersion, ref fieldsKeysIncluded, ref numRecords, ref tableData);

                if (ret != 0 || numRecords == 0)
                {
                    csvData = "";
                    report = "ERROR: No story stiffness data available.\nPlease ensure analysis has been run.";
                    return false;
                }

                int numFields = fieldsKeysIncluded.Length;

                // Build CSV header from field names
                sb.AppendLine(string.Join(",", fieldsKeysIncluded));

                // Extract data rows
                for (int i = 0; i < numRecords; i++)
                {
                    var rowData = new List<string>();
                    for (int j = 0; j < numFields; j++)
                    {
                        int dataIndex = i * numFields + j;
                        if (dataIndex < tableData.Length)
                        {
                            rowData.Add(tableData[dataIndex]);
                        }
                    }
                    sb.AppendLine(string.Join(",", rowData));
                }

                reportSb.AppendLine($"✓ Extracted {numRecords} stiffness value(s)");

                csvData = sb.ToString();
                report = reportSb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}\n\nPlease ensure analysis has been run.";
                return false;
            }
        }

        /// <summary>
        /// Extracts centers of mass and rigidity by story
        /// </summary>
        public bool ExtractCentersOfMassAndRigidity(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting centers of mass and rigidity...\r\n");

                // Use cached table list
                if (!EnsureTablesListCached())
                {
                    csvData = "";
                    report = "ERROR: Failed to get available database tables.";
                    return false;
                }

                cDatabaseTables dbTables = _SapModel.DatabaseTables;

                // Find "Centers of Mass and Rigidity" table in cached list
                string centersTableKey = null;
                for (int i = 0; i < _cachedNumTables; i++)
                {
                    string tableKeyUpper = _cachedTableKeys[i].ToUpperInvariant();
                    if ((tableKeyUpper.Contains("CENTER") || tableKeyUpper.Contains("MASS")) &&
                        (tableKeyUpper.Contains("RIGID") || tableKeyUpper.Contains("MASS")))
                    {
                        centersTableKey = _cachedTableKeys[i];
                        break;
                    }
                }

                if (centersTableKey == null)
                {
                    csvData = "";
                    report = "ERROR: 'Centers of Mass and Rigidity' table not found in database.\nPlease ensure analysis has been run and diaphragms are defined.";
                    return false;
                }

                reportSb.AppendLine($"✓ Found table: {centersTableKey}\r\n");

                // Get table data
                string[] fieldKeyList = null;
                string groupName = "";
                int tableVersion = 0;
                string[] fieldsKeysIncluded = null;
                int numRecords = 0;
                string[] tableData = null;

                int ret = dbTables.GetTableForDisplayArray(centersTableKey, ref fieldKeyList, groupName,
                    ref tableVersion, ref fieldsKeysIncluded, ref numRecords, ref tableData);

                if (ret != 0 || numRecords == 0)
                {
                    csvData = "";
                    report = "ERROR: No centers of mass/rigidity data available.\nPlease ensure analysis has been run and diaphragms are defined.";
                    return false;
                }

                int numFields = fieldsKeysIncluded.Length;

                // Build CSV header from field names
                sb.AppendLine(string.Join(",", fieldsKeysIncluded));

                // Extract data rows
                for (int i = 0; i < numRecords; i++)
                {
                    var rowData = new List<string>();
                    for (int j = 0; j < numFields; j++)
                    {
                        int dataIndex = i * numFields + j;
                        if (dataIndex < tableData.Length)
                        {
                            rowData.Add(tableData[dataIndex]);
                        }
                    }
                    sb.AppendLine(string.Join(",", rowData));
                }

                reportSb.AppendLine($"✓ Extracted {numRecords} center(s) of mass/rigidity");

                csvData = sb.ToString();
                report = reportSb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                csvData = "";
                report = $"ERROR: {ex.Message}\n\nPlease ensure analysis has been run and diaphragms are defined.";
                return false;
            }
        }

        #endregion

        #region Model Diagnostics

        /// <summary>
        /// Runs diagnostics to check what data is available in the model
        /// </summary>
        public bool RunModelDiagnostics(out string report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ETABS MODEL DIAGNOSTICS ===\n");

            try
            {
                // Check load cases
                sb.AppendLine("--- LOAD CASES ---");
                int numLoadCases = 0;
                string[] loadCaseNames = null;
                _SapModel.LoadCases.GetNameList(ref numLoadCases, ref loadCaseNames);
                sb.AppendLine($"Total Load Cases: {numLoadCases}");
                if (numLoadCases > 0)
                {
                    for (int i = 0; i < Math.Min(10, numLoadCases); i++)
                    {
                        sb.AppendLine($"  - {loadCaseNames[i]}");
                    }
                    if (numLoadCases > 10)
                        sb.AppendLine($"  ... and {numLoadCases - 10} more");
                }
                sb.AppendLine();

                // Check load combinations
                sb.AppendLine("--- LOAD COMBINATIONS ---");
                int numCombos = 0;
                string[] comboNames = null;
                _SapModel.RespCombo.GetNameList(ref numCombos, ref comboNames);
                sb.AppendLine($"Total Load Combinations: {numCombos}");
                if (numCombos > 0)
                {
                    for (int i = 0; i < Math.Min(5, numCombos); i++)
                    {
                        sb.AppendLine($"  - {comboNames[i]}");
                    }
                    if (numCombos > 5)
                        sb.AppendLine($"  ... and {numCombos - 5} more");
                }
                sb.AppendLine();

                // Check stories
                sb.AppendLine("--- STORIES ---");
                int numStories = 0;
                string[] storyNames = null;
                double[] storyElevations = null;
                double[] storyHeights = null;
                bool[] isMasterStory = null;
                string[] similarToStory = null;
                bool[] spliceAbove = null;
                double[] spliceHeight = null;
                _SapModel.Story.GetStories(ref numStories, ref storyNames, ref storyElevations,
                    ref storyHeights, ref isMasterStory, ref similarToStory, ref spliceAbove, ref spliceHeight);
                sb.AppendLine($"Total Stories: {numStories}");
                if (numStories > 0)
                {
                    for (int i = 0; i < numStories; i++)
                    {
                        sb.AppendLine($"  - {storyNames[i]} (Elevation: {storyElevations[i]})");
                    }
                }
                sb.AppendLine();

                // Check frame objects
                sb.AppendLine("--- FRAME OBJECTS (Columns/Beams) ---");
                int numFrames = 0;
                string[] frameNames = null;
                _SapModel.FrameObj.GetNameList(ref numFrames, ref frameNames);
                sb.AppendLine($"Total Frame Objects: {numFrames}");
                sb.AppendLine();

                // Check area objects
                sb.AppendLine("--- AREA OBJECTS (Walls/Slabs) ---");
                int numAreas = 0;
                string[] areaNames = null;
                _SapModel.AreaObj.GetNameList(ref numAreas, ref areaNames);
                sb.AppendLine($"Total Area Objects: {numAreas}");
                sb.AppendLine();

                // Check for modal load cases specifically
                sb.AppendLine("--- MODAL ANALYSIS CASES ---");
                int modalCount = 0;
                if (numLoadCases > 0)
                {
                    for (int i = 0; i < numLoadCases; i++)
                    {
                        eLoadCaseType caseType = eLoadCaseType.LinearStatic;
                        int subType = 0;
                        _SapModel.LoadCases.GetTypeOAPI(loadCaseNames[i], ref caseType, ref subType);
                        if (caseType == eLoadCaseType.Modal)
                        {
                            modalCount++;
                            sb.AppendLine($"  - {loadCaseNames[i]} (Modal)");
                        }
                    }
                }
                if (modalCount == 0)
                {
                    sb.AppendLine("  NO MODAL ANALYSIS CASES FOUND");
                    sb.AppendLine("  (This is why modal extraction is failing)");
                }
                sb.AppendLine();

                // Check analysis results availability
                sb.AppendLine("--- ANALYSIS RESULTS ---");
                try
                {
                    // First, try to setup all case/combo output
                    try
                    {
                        _SapModel.Results.Setup.DeselectAllCasesAndCombosForOutput();
                        // Select ALL cases for output instead of specific "MODAL" case
                        if (numLoadCases > 0)
                        {
                            for (int i = 0; i < numLoadCases; i++)
                            {
                                try
                                {
                                    _SapModel.Results.Setup.SetCaseSelectedForOutput(loadCaseNames[i]);
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }

                    // Try multiple result checks
                    bool hasResults = false;
                    int totalResults = 0;

                    // Check 1: Base reactions
                    try
                    {
                        int numResults = 0;
                        string[] lc = null, st = null;
                        double[] sn = null, fx = null, fy = null, fz = null, mx = null, my = null, mz = null;
                        double gx = 0, gy = 0, gz = 0;

                        int ret = _SapModel.Results.BaseReact(ref numResults, ref lc, ref st, ref sn,
                            ref fx, ref fy, ref fz, ref mx, ref my, ref mz, ref gx, ref gy, ref gz);

                        if (ret == 0 && numResults > 0)
                        {
                            sb.AppendLine($"  ✓ Base reactions available ({numResults} load cases)");
                            hasResults = true;
                            totalResults += numResults;
                        }
                    }
                    catch { }

                    // Check 2: Modal periods
                    try
                    {
                        int numResults = 0;
                        string[] lc = null, st = null;
                        double[] sn = null, period = null, freq = null, circFreq = null, eigenValue = null;

                        int ret = _SapModel.Results.ModalPeriod(ref numResults, ref lc, ref st,
                            ref sn, ref period, ref freq, ref circFreq, ref eigenValue);

                        if (ret == 0 && numResults > 0)
                        {
                            sb.AppendLine($"  ✓ Modal results available ({numResults} modes)");
                            hasResults = true;
                            totalResults += numResults;
                        }
                    }
                    catch { }

                    if (!hasResults)
                    {
                        sb.AppendLine("  ✗ NO ANALYSIS RESULTS FOUND");
                        sb.AppendLine("  ");
                        sb.AppendLine("  Possible reasons:");
                        sb.AppendLine("  1. Analysis has not been run yet");
                        sb.AppendLine("  2. Analysis failed (check ETABS analysis log)");
                        sb.AppendLine("  3. Model is locked (File > Unlock Model)");
                        sb.AppendLine("  ");
                        sb.AppendLine("  Try: Analyze > Run Analysis in ETABS");
                    }
                    else
                    {
                        sb.AppendLine($"  ");
                        sb.AppendLine($"  ✓ ANALYSIS RESULTS AVAILABLE");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  ✗ Could not check results: {ex.Message}");
                }
                sb.AppendLine();

                // Check available database tables using cached list
                sb.AppendLine("--- AVAILABLE DATABASE TABLES ---");
                sb.AppendLine("  NOTE: Database tables are generated automatically by ETABS");
                sb.AppendLine("  They should be available once the model has objects/materials");
                sb.AppendLine();
                try
                {
                    // Use cached table list if available, otherwise fetch
                    bool tablesAvailable = EnsureTablesListCached();

                    if (tablesAvailable && _cachedNumTables > 0)
                    {
                        sb.AppendLine($"  ✓ Found {_cachedNumTables} database table(s) total");
                        sb.AppendLine();

                        // Categorize tables
                        var materialTables = new List<string>();
                        var objectTables = new List<string>();
                        var analysisTables = new List<string>();
                        var otherTables = new List<string>();

                        for (int i = 0; i < _cachedNumTables; i++)
                        {
                            string keyUpper = _cachedTableKeys[i].ToUpperInvariant();

                            if (keyUpper.Contains("MATERIAL"))
                                materialTables.Add(_cachedTableKeys[i]);
                            else if (keyUpper.Contains("OBJECT") || keyUpper.Contains("ELEMENT") ||
                                     keyUpper.Contains("CONNECTIVITY") || keyUpper.Contains("SECTION"))
                                objectTables.Add(_cachedTableKeys[i]);
                            else if (keyUpper.Contains("MODAL") || keyUpper.Contains("REACTION") ||
                                     keyUpper.Contains("DRIFT") || keyUpper.Contains("FORCE"))
                                analysisTables.Add(_cachedTableKeys[i]);
                            else
                                otherTables.Add(_cachedTableKeys[i]);
                        }

                        if (materialTables.Count > 0)
                        {
                            sb.AppendLine($"  Material Tables ({materialTables.Count}):");
                            foreach (var tbl in materialTables.Take(5))
                                sb.AppendLine($"    - {tbl}");
                            if (materialTables.Count > 5)
                                sb.AppendLine($"    ... and {materialTables.Count - 5} more");
                            sb.AppendLine();
                        }

                        if (objectTables.Count > 0)
                        {
                            sb.AppendLine($"  Object/Element Tables ({objectTables.Count}):");
                            foreach (var tbl in objectTables.Take(5))
                                sb.AppendLine($"    - {tbl}");
                            if (objectTables.Count > 5)
                                sb.AppendLine($"    ... and {objectTables.Count - 5} more");
                            sb.AppendLine();
                        }

                        if (analysisTables.Count > 0)
                        {
                            sb.AppendLine($"  Analysis Result Tables ({analysisTables.Count}):");
                            foreach (var tbl in analysisTables.Take(5))
                                sb.AppendLine($"    - {tbl}");
                            if (analysisTables.Count > 5)
                                sb.AppendLine($"    ... and {analysisTables.Count - 5} more");
                            sb.AppendLine();
                        }

                        sb.AppendLine($"  ✓ Database tables are available and ready for extraction");
                    }
                    else if (!tablesAvailable)
                    {
                        sb.AppendLine($"  ✗ Failed to get table list from ETABS API");
                    }
                    else
                    {
                        sb.AppendLine("  ⚠ No database tables found - model may be empty");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  ✗ Error checking tables: {ex.Message}");
                }
                sb.AppendLine();

                sb.AppendLine("=== END DIAGNOSTICS ===");

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = $"ERROR running diagnostics: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Extracts all available data types to a specified folder
        /// </summary>
        /// <param name="outputFolder">Folder to save extraction files</param>
        /// <param name="report">Output status report</param>
        /// <param name="progressCallback">Optional callback for progress updates (current, total, message)</param>
        public bool ExtractAllData(string outputFolder, out string report, Action<int, int, string> progressCallback = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== EXTRACTING ALL DATA ===\n");

            int successCount = 0;
            int failCount = 0;
            int skipCount = 0;
            int currentStep = 0;
            const int totalSteps = 18; // Updated to include 4 new extraction types

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            try
            {
                // Report initial progress
                progressCallback?.Invoke(0, totalSteps, "Initializing extraction...");

                // PERFORMANCE OPTIMIZATION: Setup operations once at the beginning instead of in each method
                sb.AppendLine("Initializing...");

                // 1. Clear and prepare table cache for this extraction session
                ClearTablesListCache();
                if (EnsureTablesListCached())
                {
                    sb.AppendLine($"  ✓ Database tables list cached ({_cachedNumTables} tables)");
                }
                else
                {
                    sb.AppendLine("  ⚠ Could not cache database tables list (some extractions may be limited)");
                }

                // 2. Set units once for all extractions
                eUnits originalUnits = _SapModel.GetPresentUnits();
                _SapModel.SetPresentUnits(eUnits.kN_m_C);
                sb.AppendLine("  ✓ Units set to kN-m-C");

                // 3. Setup all load cases and combos for output once
                SetupAllCasesForOutput();
                sb.AppendLine("  ✓ Load cases and combos configured for output");

                sb.AppendLine();

                // 1. Base Reactions
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting base reactions...");
                sb.AppendLine("1. Base Reactions...");
                if (ExtractBaseReactions(out string csvData, out string result))
                {
                    string filePath = Path.Combine(outputFolder, $"BaseReactions_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 2. Project Info
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting project information...");
                sb.AppendLine("2. Project/Model Information...");
                if (ExtractProjectInfo(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"ProjectInfo_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 3. Story Info
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting story information...");
                sb.AppendLine("3. Story/Level Information...");
                if (ExtractStoryInfo(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"StoryInfo_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 4. Grid Info
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting grid information...");
                sb.AppendLine("4. Grid System Information...");
                if (ExtractGridInfo(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"GridInfo_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 5. Frame Modifiers
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting frame modifiers...");
                sb.AppendLine("5. Frame Property Modifiers...");
                if (ExtractFrameModifiers(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"FrameModifiers_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 6. Area Modifiers
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting area modifiers...");
                sb.AppendLine("6. Area Property Modifiers...");
                if (ExtractAreaModifiers(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"AreaModifiers_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 7. Wall Elements
                currentStep++;
                sb.AppendLine("7. Wall Elements...");
                // Pass nested progress: map wall progress (0-100%) to step 7's portion (7/14 to 8/14)
                if (ExtractWallElements(out csvData, out result,
                    (wallCurrent, wallTotal, wallMsg) =>
                    {
                        // Map wall extraction progress to overall progress
                        double wallProgress = wallTotal > 0 ? (double)wallCurrent / wallTotal : 0;
                        double overallProgress = (currentStep - 1) + wallProgress;
                        progressCallback?.Invoke((int)overallProgress, totalSteps, $"Step {currentStep}: {wallMsg}");
                    }))
                {
                    string filePath = Path.Combine(outputFolder, $"WallElements_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 8. Column Elements
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting column elements...");
                sb.AppendLine("8. Column Elements...");
                if (ExtractColumnElements(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"ColumnElements_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 9. Modal Periods
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting modal periods...");
                sb.AppendLine("9. Modal Periods...");
                if (ExtractModalPeriods(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"ModalPeriods_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 10. Modal Mass Ratios
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting modal mass ratios...");
                sb.AppendLine("10. Modal Participating Mass Ratios...");
                if (ExtractModalMassRatios(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"ModalMassRatios_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 11. Story Drifts
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting story drifts...");
                sb.AppendLine("11. Story Drifts...");
                if (ExtractStoryDrifts(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"StoryDrifts_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 12. Base Shear
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting base shear...");
                sb.AppendLine("12. Base Shear...");
                if (ExtractBaseShear(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"BaseShear_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ✗ {result}");
                    failCount++;
                }

                // 13. Composite Column Design (might not apply)
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting composite column design...");
                sb.AppendLine("13. Composite Column Design...");
                if (ExtractCompositeColumnDesign(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"CompositeColumnDesign_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ⊘ Skipped (not applicable to this model)");
                    skipCount++;
                }

                // 14. All Database Tables (exports ALL tables to individual CSV files)
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Exporting all database tables...");
                sb.AppendLine("14. All Database Tables...");
                if (ExtractAllDatabaseTables(outputFolder, timestamp, out result))
                {
                    sb.AppendLine($"   ✓ {result}");
                    successCount++;
                }
                else
                {
                    sb.AppendLine($"   ⊘ {result}");
                    skipCount++;
                }

                // 15. Frame Section Properties
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting frame section properties...");
                sb.AppendLine("15. Frame Section Properties...");
                if (ExtractFrameSectionProperties(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"FrameSectionProperties_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ⊘ Skipped (no frame sections in model)");
                    skipCount++;
                }

                // 16. Story Forces
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting story forces...");
                sb.AppendLine("16. Story Forces...");
                if (ExtractStoryForces(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"StoryForces_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ⊘ Skipped (analysis may not be complete)");
                    skipCount++;
                }

                // 17. Story Stiffness
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting story stiffness...");
                sb.AppendLine("17. Story Stiffness...");
                if (ExtractStoryStiffness(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"StoryStiffness_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ⊘ Skipped (story stiffness not available)");
                    skipCount++;
                }

                // 18. Centers of Mass and Rigidity
                currentStep++;
                progressCallback?.Invoke(currentStep, totalSteps, "Extracting centers of mass and rigidity...");
                sb.AppendLine("18. Centers of Mass and Rigidity...");
                if (ExtractCentersOfMassAndRigidity(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"CentersOfMassRigidity_{timestamp}.csv");
                    if (SaveToFile(csvData, filePath, out string error))
                    {
                        sb.AppendLine($"   ✓ Saved: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"   ✗ Save failed: {error}");
                        failCount++;
                    }
                }
                else
                {
                    sb.AppendLine($"   ⊘ Skipped (no diaphragms in model)");
                    skipCount++;
                }

                sb.AppendLine();
                sb.AppendLine("=== SUMMARY ===");
                sb.AppendLine($"✓ Success: {successCount} files");
                sb.AppendLine($"✗ Failed: {failCount} extractions");
                sb.AppendLine($"⊘ Skipped: {skipCount} (not applicable)");
                sb.AppendLine($"\nAll files saved to: {outputFolder}");

                // Restore original units
                _SapModel.SetPresentUnits(originalUnits);

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = sb.ToString() + $"\n\nFATAL ERROR: {ex.Message}";
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
