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
                // Setup all cases for output
                SetupAllCasesForOutput();

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

        /// <summary>
        /// Extracts wall/shear wall elements with dimensions and location
        /// </summary>
        public bool ExtractWallElements(out string csvData, out string report)
        {
            var sb = new StringBuilder();
            var reportSb = new StringBuilder();

            try
            {
                reportSb.AppendLine("Extracting wall element information...\r\n");

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

                // CSV Header
                sb.AppendLine("Name,Label,Story,PropertyName,WallType,Thickness(mm),PierLabel,SpandrelLabel,WallFunction,CentroidX,CentroidY,CentroidZ,MinX,MaxX,MinY,MaxY");

                // Extract data for each area object
                int wallCount = 0;
                int shearWallCount = 0;
                int spandrelCount = 0;
                int gravityWallCount = 0;

                for (int i = 0; i < numberNames; i++)
                {
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
                        ret = _SapModel.PointObj.GetCoordCartesian(points[j], ref x, ref y, ref z);
                        if (ret == 0)
                        {
                            sumX += x;
                            sumY += y;
                            sumZ += z;
                            minX = Math.Min(minX, x);
                            maxX = Math.Max(maxX, x);
                            minY = Math.Min(minY, y);
                            maxY = Math.Max(maxY, y);
                        }
                    }

                    // Calculate centroid
                    double centroidX = sumX / numPoints;
                    double centroidY = sumY / numPoints;
                    double centroidZ = sumZ / numPoints;

                    // Convert thickness to mm
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
                        $"\"{pierLabel}\",\"{spandrelLabel}\",\"{wallFunction}\"," +
                        $"{centroidX:0.0000},{centroidY:0.0000},{centroidZ:0.0000}," +
                        $"{minX:0.0000},{maxX:0.0000},{minY:0.0000},{maxY:0.0000}");
                    wallCount++;
                }

                reportSb.AppendLine($"✓ Successfully extracted {wallCount} wall element(s)");
                reportSb.AppendLine();
                reportSb.AppendLine("Wall Classification Breakdown:");
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

                    ret = _SapModel.PointObj.GetCoordCartesian(point1, ref x1, ref y1, ref z1);
                    if (ret != 0) continue;

                    ret = _SapModel.PointObj.GetCoordCartesian(point2, ref x2, ref y2, ref z2);
                    if (ret != 0) continue;

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

                    // Convert dimensions to mm
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
                // Setup all cases for output
                SetupAllCasesForOutput();

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
                // Setup all cases for output
                SetupAllCasesForOutput();

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
                // Setup all cases for output
                SetupAllCasesForOutput();

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
                // Setup all cases for output
                SetupAllCasesForOutput();

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
        /// Extracts quantities summary using database tables
        /// </summary>
        public bool ExtractQuantitiesSummary(out string csvData, out string report)
        {
            try
            {
                cDatabaseTables dbTables = _SapModel.DatabaseTables;

                // Comprehensive list of common ETABS database table names
                string[] tableKeysToTry = new string[]
                {
                    // Material tables
                    "Material List 2 - By Object Type",
                    "Material List - By Object Type",
                    "Material List 2",
                    "Material List 1 - By Object Type",
                    "Material List 2 - Material Properties",
                    "Material Properties 02 - Basic Mechanical Properties",
                    "Material Properties 03a - Steel Data",
                    "Material Properties 03b - Concrete Data",

                    // Object summary tables
                    "Objects and Elements - Summary",
                    "Objects and Elements - Frames",
                    "Objects and Elements - Areas",

                    // Connectivity tables
                    "Connectivity - Frame",
                    "Connectivity - Area",

                    // Section properties
                    "Frame Section Properties 01 - General",
                    "Area Section Properties",

                    // Loads
                    "Load Pattern Definitions",
                    "Load Case Definitions",
                    "Load Combination Definitions",

                    // Analysis results
                    "Modal Participating Mass Ratios",
                    "Modal Periods And Frequencies",
                    "Base Reactions",

                    // Design results
                    "Concrete Frame Design Summary",
                    "Steel Frame Design Summary"
                };

                // Find all available tables
                var availableTables = new List<(string tableName, int recordCount, string[] fields, string[] data)>();

                foreach (string tableKey in tableKeysToTry)
                {
                    string[] fieldKeyList = null;
                    string groupName = "";
                    int tableVersion = 0;
                    string[] fieldsKeysIncluded = null;
                    int numRecords = 0;
                    string[] tableData = null;

                    int ret = dbTables.GetTableForDisplayArray(tableKey, ref fieldKeyList, groupName,
                        ref tableVersion, ref fieldsKeysIncluded, ref numRecords, ref tableData);

                    if (ret == 0 && numRecords > 0 && fieldsKeysIncluded != null && tableData != null)
                    {
                        availableTables.Add((tableKey, numRecords, fieldsKeysIncluded, tableData));
                    }
                }

                if (availableTables.Count == 0)
                {
                    csvData = "";
                    report = "ERROR: No database tables with data were found.\n\n" +
                            "Possible reasons:\n" +
                            "1. The model has no objects (frames, areas, etc.)\n" +
                            "2. Materials are not assigned to objects\n" +
                            "3. Analysis has not been run\n\n" +
                            $"Tried {tableKeysToTry.Length} common table names.\n\n" +
                            "Note: You can view available tables in Display > Show Tables";
                    return false;
                }

                // Extract the first available table with the most records
                var selectedTable = availableTables.OrderByDescending(t => t.recordCount).First();

                var csv = new StringBuilder();
                csv.AppendLine($"# Table: {selectedTable.tableName}");
                csv.AppendLine($"# Records: {selectedTable.recordCount}");
                csv.AppendLine($"# Note: {availableTables.Count} total table(s) available in model");
                csv.AppendLine();

                // Add header row
                csv.AppendLine(string.Join(",", selectedTable.fields));

                // Add data rows
                int numFields = selectedTable.fields.Length;
                for (int i = 0; i < selectedTable.recordCount; i++)
                {
                    var rowData = new List<string>();
                    for (int j = 0; j < numFields; j++)
                    {
                        int index = i * numFields + j;
                        if (index < selectedTable.data.Length)
                        {
                            // Escape commas in data
                            string value = selectedTable.data[index];
                            if (value.Contains(","))
                            {
                                value = $"\"{value}\"";
                            }
                            rowData.Add(value);
                        }
                    }
                    csv.AppendLine(string.Join(",", rowData));
                }

                csvData = csv.ToString();

                var reportSb = new StringBuilder();
                reportSb.AppendLine($"✓ Successfully extracted '{selectedTable.tableName}'");
                reportSb.AppendLine($"  Records: {selectedTable.recordCount}, Fields: {numFields}");
                reportSb.AppendLine();
                reportSb.AppendLine($"Found {availableTables.Count} available table(s) in total:");
                foreach (var table in availableTables.OrderByDescending(t => t.recordCount))
                {
                    reportSb.AppendLine($"  - {table.tableName} ({table.recordCount} records)");
                }

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

                // Check available database tables
                sb.AppendLine("--- AVAILABLE DATABASE TABLES ---");
                sb.AppendLine("  NOTE: Database tables are generated automatically by ETABS");
                sb.AppendLine("  They should be available once the model has objects/materials");
                sb.AppendLine();
                try
                {
                    // Use comprehensive list
                    string[] tableKeys = new string[]
                    {
                        "Material List 2 - By Object Type",
                        "Material List - By Object Type",
                        "Material List 2",
                        "Material List 1 - By Object Type",
                        "Material Properties 02 - Basic Mechanical Properties",
                        "Objects and Elements - Summary",
                        "Objects and Elements - Frames",
                        "Objects and Elements - Areas",
                        "Connectivity - Frame",
                        "Connectivity - Area",
                        "Frame Section Properties 01 - General",
                        "Area Section Properties",
                        "Load Pattern Definitions",
                        "Modal Periods And Frequencies",
                        "Base Reactions"
                    };

                    int availableCount = 0;
                    foreach (string tableKey in tableKeys)
                    {
                        string[] fkl = null;
                        int tv = 0;
                        string[] fki = null;
                        int nr = 0;
                        string[] td = null;

                        int ret = _SapModel.DatabaseTables.GetTableForDisplayArray(tableKey, ref fkl, "",
                            ref tv, ref fki, ref nr, ref td);

                        if (ret == 0 && nr > 0)
                        {
                            sb.AppendLine($"  ✓ {tableKey} ({nr} records)");
                            availableCount++;
                        }
                    }

                    if (availableCount > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"  Total: {availableCount} table(s) available");
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.AppendLine("  If ALL tables show 'not available', this is usually because:");
                        sb.AppendLine("  - ETABS table names may differ in this version");
                        sb.AppendLine("  - You can view actual table names via Display > Show Tables in ETABS");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  Error checking tables: {ex.Message}");
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
        public bool ExtractAllData(string outputFolder, out string report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== EXTRACTING ALL DATA ===\n");

            int successCount = 0;
            int failCount = 0;
            int skipCount = 0;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            try
            {
                // 1. Base Reactions
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
                sb.AppendLine("7. Wall Elements...");
                if (ExtractWallElements(out csvData, out result))
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

                // 14. Quantities Summary (might not work due to table names)
                sb.AppendLine("14. Quantities Summary...");
                if (ExtractQuantitiesSummary(out csvData, out result))
                {
                    string filePath = Path.Combine(outputFolder, $"QuantitiesSummary_{timestamp}.csv");
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
                    sb.AppendLine($"   ⊘ Skipped (database table names may differ in this ETABS version)");
                    skipCount++;
                }

                sb.AppendLine();
                sb.AppendLine("=== SUMMARY ===");
                sb.AppendLine($"✓ Success: {successCount} files");
                sb.AppendLine($"✗ Failed: {failCount} extractions");
                sb.AppendLine($"⊘ Skipped: {skipCount} (not applicable)");
                sb.AppendLine($"\nAll files saved to: {outputFolder}");

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
