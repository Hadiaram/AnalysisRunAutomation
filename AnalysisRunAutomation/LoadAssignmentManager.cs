using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ETABSv1;

namespace ETABS_Plugin
{
    public class LoadAssignmentManager
    {
        private readonly cSapModel _SapModel;

        public LoadAssignmentManager(cSapModel sapModel) => _SapModel = sapModel;

        // Map friendly direction strings to ETABS int code
        private static int DirCode(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir)) return 10; // Gravity default
            switch (dir.Trim().ToUpperInvariant())
            {
                case "LOCAL1": case "L1": return 1;
                case "LOCAL2": case "L2": return 2;
                case "LOCAL3": case "L3": return 3;
                case "X": case "GX": case "GLOBALX": return 4;
                case "Y": case "GY": case "GLOBALY": return 5;
                case "Z": case "GZ": case "GLOBALZ": return 6;
                case "GRAVITY": case "G": default: return 10;
            }
        }

        #region Frame Load Assignment

        /// <summary>
        /// Applies uniform distributed load (UDL) to a frame (N/mm).
        /// </summary>
        public bool ApplyFrameUDL(string frameName, string loadPattern, double value, string direction, out string report)
        {
            var sb = new StringBuilder();
            try
            {
                int myType = 1;                 // 1 = uniform force
                int dir = DirCode(direction);   // ETABS expects an int code
                double dist1 = 0.0, dist2 = 1.0;
                double val1 = value, val2 = value;

                // Signature: (Name, LoadPat, MyType, Dir, Dist1, Dist2, Val1, Val2, CSys, Replace)
                int ret = _SapModel.FrameObj.SetLoadDistributed(
                    frameName, loadPattern, myType, dir,
                    dist1, dist2, val1, val2,
                    "Global", true
                );

                if (ret == 0)
                {
                    sb.AppendLine($"✓ Applied {value} N/mm UDL to {frameName} ({direction})");
                    report = sb.ToString();
                    return true;
                }
                sb.AppendLine($"⚠ Failed to apply UDL to {frameName}, error code: {ret}");
                report = sb.ToString();
                return false;
            }
            catch (Exception ex)
            {
                report = "Exception in ApplyFrameUDL:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Applies a point load to a frame (N) at a relative distance (0–1).
        /// </summary>
        public bool ApplyFramePointLoad(string frameName, string loadPattern, double value, double relativeDistance, string direction, out string report)
        {
            var sb = new StringBuilder();
            try
            {
                int myType = 1;               // 1 = point force
                int dir = DirCode(direction);

                // Signature: (Name, LoadPat, MyType, Dir, Dist, Value, CSys, Replace)
                int ret = _SapModel.FrameObj.SetLoadPoint(
                    frameName, loadPattern, myType, dir,
                    relativeDistance, value,
                    "Global", true
                );

                if (ret == 0)
                {
                    sb.AppendLine($"✓ Applied {value} N point load at {relativeDistance:P0} on {frameName} ({direction})");
                    report = sb.ToString();
                    return true;
                }
                sb.AppendLine($"⚠ Failed to apply point load to {frameName}, error code: {ret}");
                report = sb.ToString();
                return false;
            }
            catch (Exception ex)
            {
                report = "Exception in ApplyFramePointLoad:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Applies UDL to all beams (frames with dominant horizontal span).
        /// </summary>
        public bool ApplyUDLToAllBeams(string loadPattern, double value, out string report)
        {
            var sb = new StringBuilder();
            try
            {
                if (_SapModel.GetModelIsLocked()) _SapModel.SetModelIsLocked(false);

                eUnits prev = _SapModel.GetPresentUnits();
                _SapModel.SetPresentUnits(eUnits.N_mm_C);

                try
                {
                    int nFrames = 0;
                    string[] frameNames = Array.Empty<string>();
                    _SapModel.FrameObj.GetNameList(ref nFrames, ref frameNames);

                    int applied = 0, errors = 0;

                    foreach (var frameName in frameNames)
                    {
                        string pt1 = "", pt2 = "";
                        _SapModel.FrameObj.GetPoints(frameName, ref pt1, ref pt2);

                        double x1 = 0, y1 = 0, z1 = 0;
                        double x2 = 0, y2 = 0, z2 = 0;
                        _SapModel.PointObj.GetCoordCartesian(pt1, ref x1, ref y1, ref z1);
                        _SapModel.PointObj.GetCoordCartesian(pt2, ref x2, ref y2, ref z2);

                        double deltaZ = Math.Abs(z2 - z1);
                        double deltaXY = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

                        if (deltaXY > deltaZ) // beam-ish
                        {
                            int dir = DirCode("Gravity");
                            int ret = _SapModel.FrameObj.SetLoadDistributed(
                                frameName, loadPattern, 1, dir,
                                0.0, 1.0, value, value,
                                "Global", true
                            );

                            if (ret == 0) applied++; else errors++;
                        }
                    }

                    sb.AppendLine($"✓ Applied {value} N/mm UDL to {applied} beams");
                    if (errors > 0) sb.AppendLine($"⚠ {errors} errors");
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
                report = "Exception in ApplyUDLToAllBeams:\r\n" + ex;
                return false;
            }
        }

        #endregion

        #region Area Load Assignment

        /// <summary>
        /// Applies uniform surface load to an area (N/mm²).
        /// </summary>
        public bool ApplyAreaUniformLoad(string areaName, string loadPattern, double value, out string report)
        {
            var sb = new StringBuilder();
            try
            {
                // Signature: (Name, LoadPat, Value, CSys, Replace)
                int ret = _SapModel.AreaObj.SetLoadUniform(
                    areaName, loadPattern, value,
                    10,          // Dir: 10 = Gravity
                    true,        // Replace
                    "Global"     // CSys
                );


                if (ret == 0)
                {
                    sb.AppendLine($"✓ Applied {value} N/mm² uniform load to {areaName}");
                    report = sb.ToString();
                    return true;
                }
                sb.AppendLine($"⚠ Failed to apply uniform load to {areaName}, error code: {ret}");
                report = sb.ToString();
                return false;
            }
            catch (Exception ex)
            {
                report = "Exception in ApplyAreaUniformLoad:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Applies uniform load to all horizontal areas (slabs) at a given elevation.
        /// </summary>
        public bool ApplyLoadToSlabsAtElevation(double elevation, string loadPattern, double value, double tolerance, out string report)
        {
            var sb = new StringBuilder();
            try
            {
                int nAreas = 0;
                string[] areaNames = Array.Empty<string>();
                _SapModel.AreaObj.GetNameList(ref nAreas, ref areaNames);

                int applied = 0, errors = 0;

                foreach (var areaName in areaNames)
                {
                    int nPoints = 0;
                    string[] pointNames = Array.Empty<string>();
                    _SapModel.AreaObj.GetPoints(areaName, ref nPoints, ref pointNames);

                    if (nPoints < 3) continue;

                    double x1 = 0, y1 = 0, z1 = 0;
                    _SapModel.PointObj.GetCoordCartesian(pointNames[0], ref x1, ref y1, ref z1);

                    if (Math.Abs(z1 - elevation) <= tolerance)
                    {
                        double x2 = 0, y2 = 0, z2 = 0;
                        double x3 = 0, y3 = 0, z3 = 0;
                        _SapModel.PointObj.GetCoordCartesian(pointNames[1], ref x2, ref y2, ref z2);
                        _SapModel.PointObj.GetCoordCartesian(pointNames[2], ref x3, ref y3, ref z3);

                        double v1x = x2 - x1, v1y = y2 - y1, v1z = z2 - z1;
                        double v2x = x3 - x1, v2y = y3 - y1, v2z = z3 - z1;

                        double nx = v1y * v2z - v1z * v2y;
                        double ny = v1z * v2x - v1x * v2z;
                        double nz = v1x * v2y - v1y * v2x;
                        double nlen = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                        double nzUnit = (nlen > 0) ? nz / nlen : 0.0;

                        if (Math.Abs(nzUnit) > 0.7) // horizontal
                        {
                            int ret = _SapModel.AreaObj.SetLoadUniform(
                                areaName, loadPattern, value,
                                10,          // Dir: Gravity
                                true,
                                "Global"
                            );

                            if (ret == 0) applied++; else errors++;
                        }
                    }
                }

                sb.AppendLine($"✓ Applied {value} N/mm² load to {applied} slabs at Z={elevation:0.0}mm");
                if (errors > 0) sb.AppendLine($"⚠ {errors} errors");

                report = sb.ToString();
                return errors == 0;
            }
            catch (Exception ex)
            {
                report = "Exception in ApplyLoadToSlabsAtElevation:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Applies uniform load to all slabs in the model.
        /// </summary>
        public bool ApplyLoadToAllSlabs(string loadPattern, double value, out string report)
        {
            var sb = new StringBuilder();
            try
            {
                if (_SapModel.GetModelIsLocked()) _SapModel.SetModelIsLocked(false);

                eUnits prev = _SapModel.GetPresentUnits();
                _SapModel.SetPresentUnits(eUnits.N_mm_C);

                try
                {
                    int nAreas = 0;
                    string[] areaNames = Array.Empty<string>();
                    _SapModel.AreaObj.GetNameList(ref nAreas, ref areaNames);

                    int applied = 0, errors = 0;

                    foreach (var areaName in areaNames)
                    {
                        int nPoints = 0;
                        string[] pointNames = Array.Empty<string>();
                        _SapModel.AreaObj.GetPoints(areaName, ref nPoints, ref pointNames);
                        if (nPoints < 3) continue;

                        double x1 = 0, y1 = 0, z1 = 0;
                        double x2 = 0, y2 = 0, z2 = 0;
                        double x3 = 0, y3 = 0, z3 = 0;
                        _SapModel.PointObj.GetCoordCartesian(pointNames[0], ref x1, ref y1, ref z1);
                        _SapModel.PointObj.GetCoordCartesian(pointNames[1], ref x2, ref y2, ref z2);
                        _SapModel.PointObj.GetCoordCartesian(pointNames[2], ref x3, ref y3, ref z3);

                        double v1x = x2 - x1, v1y = y2 - y1, v1z = z2 - z1;
                        double v2x = x3 - x1, v2y = y3 - y1, v2z = z3 - z1;

                        double nx = v1y * v2z - v1z * v2y;
                        double ny = v1z * v2x - v1x * v2z;
                        double nz = v1x * v2y - v1y * v2x;
                        double nlen = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                        double nzUnit = (nlen > 0) ? nz / nlen : 0.0;

                        if (Math.Abs(nzUnit) > 0.7) // slab
                        {
                            int ret = _SapModel.AreaObj.SetLoadUniform(
                                areaName, loadPattern, value,
                                10,          // Dir: Gravity
                                true,
                                "Global"
                            );

                            if (ret == 0) applied++; else errors++;
                        }
                    }

                    sb.AppendLine($"✓ Applied {value} N/mm² ({value * 1e6:0.00} kN/m²) load to {applied} slabs");
                    if (errors > 0) sb.AppendLine($"⚠ {errors} errors");

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
                report = "Exception in ApplyLoadToAllSlabs:\r\n" + ex;
                return false;
            }
        }

        #endregion

        #region Temperature Loads

        /// <summary>
        /// Applies a uniform temperature change to a frame (°C).
        /// </summary>
        public bool ApplyFrameTemperatureLoad(string frameName, string loadPattern, double deltaT, out string report)
        {
            var sb = new StringBuilder();
            try
            {
                // Signature: (Name, LoadPat, MyType, DeltaT, CSys, Replace)
                int ret = _SapModel.FrameObj.SetLoadTemperature(
                    frameName, loadPattern,
                    1, deltaT, "Global", true
                );

                if (ret == 0)
                {
                    sb.AppendLine($"✓ Applied ΔT={deltaT}°C to frame {frameName}");
                    report = sb.ToString();
                    return true;
                }
                sb.AppendLine($"⚠ Failed to apply temperature load, error code: {ret}");
                report = sb.ToString();
                return false;
            }
            catch (Exception ex)
            {
                report = "Exception in ApplyFrameTemperatureLoad:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Applies a uniform temperature change to an area (°C).
        /// </summary>
        public bool ApplyAreaTemperatureLoad(string areaName, string loadPattern, double deltaT, out string report)
        {
            var sb = new StringBuilder();
            try
            {
                // Signature: (Name, LoadPat, MyType, DeltaT, CSys, Replace)
                int ret = _SapModel.AreaObj.SetLoadTemperature(
                    areaName, loadPattern,
                    1, deltaT, "Global", true
                );

                if (ret == 0)
                {
                    sb.AppendLine($"✓ Applied ΔT={deltaT}°C to area {areaName}");
                    report = sb.ToString();
                    return true;
                }
                sb.AppendLine($"⚠ Failed to apply temperature load, error code: {ret}");
                report = sb.ToString();
                return false;
            }
            catch (Exception ex)
            {
                report = "Exception in ApplyAreaTemperatureLoad:\r\n" + ex;
                return false;
            }
        }

        #endregion

        #region Utility Methods (summary / clear)

        /// <summary>
        /// Summarizes load assignments using database tables (version-robust).
        /// </summary>
        public bool GetLoadAssignmentSummary(out string report)
        {
            var sb = new StringBuilder();
            try
            {
                int nTables = 0;
                string[] tableKeys = Array.Empty<string>();
                string[] tableNames = Array.Empty<string>();
                int[] importTypes = Array.Empty<int>();
                int ret = _SapModel.DatabaseTables.GetAvailableTables(ref nTables, ref tableKeys, ref tableNames, ref importTypes);
                if (ret != 0) { report = "Could not read database tables."; return false; }

                string frameTable = null, areaTable = null;
                for (int i = 0; i < nTables; i++)
                {
                    var keyU = tableKeys[i].ToUpperInvariant();
                    if (frameTable == null && keyU.Contains("FRAME") && keyU.Contains("DISTR")) frameTable = tableKeys[i];
                    if (areaTable == null && keyU.Contains("AREA") && keyU.Contains("UNIFORM")) areaTable = tableKeys[i];
                }

                Dictionary<string, int> CountByPattern(string tableKey)
                {
                    var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    if (string.IsNullOrEmpty(tableKey)) return counts;

                    int ver = 0, numRecs = 0;
                    string[] fieldKeysFilter = Array.Empty<string>(); // required ref param #2
                    string[] fieldKeysInData = Array.Empty<string>();
                    string[] tableData = Array.Empty<string>();

                    // Signature: (TableKey, ref FieldKeys, GroupName, ref TableVersion, ref FieldKeysInData, ref NumberRecords, ref TableData)
                    int r = _SapModel.DatabaseTables.GetTableForDisplayArray(
                        tableKey,
                        ref fieldKeysFilter,
                        "",
                        ref ver,
                        ref fieldKeysInData,
                        ref numRecs,
                        ref tableData
                    );
                    if (r != 0 || numRecs <= 0 || fieldKeysInData == null || tableData == null) return counts;

                    int patCol = -1;
                    for (int c = 0; c < fieldKeysInData.Length; c++)
                    {
                        var fk = fieldKeysInData[c]?.ToUpperInvariant() ?? "";
                        if (fk.Contains("LOAD") && fk.Contains("PAT")) { patCol = c; break; }
                    }
                    if (patCol < 0) return counts;

                    int cols = fieldKeysInData.Length;
                    for (int row = 0; row < numRecs; row++)
                    {
                        string pat = tableData[row * cols + patCol];
                        if (string.IsNullOrWhiteSpace(pat)) pat = "<none>";
                        counts[pat] = counts.TryGetValue(pat, out var cnt) ? cnt + 1 : 1;
                    }
                    return counts;
                }

                var frameCounts = CountByPattern(frameTable);
                var areaCounts = CountByPattern(areaTable);

                sb.AppendLine("=== LOAD ASSIGNMENT SUMMARY ===");
                sb.AppendLine("\nFrame Loads - Distributed:");
                if (frameCounts.Count == 0) sb.AppendLine("  (none)");
                foreach (var kv in frameCounts.OrderByDescending(k => k.Value))
                    sb.AppendLine($"  {kv.Key}: {kv.Value}");

                sb.AppendLine("\nArea Loads - Uniform:");
                if (areaCounts.Count == 0) sb.AppendLine("  (none)");
                foreach (var kv in areaCounts.OrderByDescending(k => k.Value))
                    sb.AppendLine($"  {kv.Key}: {kv.Value}");

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in GetLoadAssignmentSummary:\r\n" + ex;
                return false;
            }
        }

        /// <summary>
        /// Clears all distributed/point loads on frames and uniform loads on areas for a specific load pattern.
        /// </summary>
        public bool ClearLoadsForPattern(string loadPattern, out string report)
        {
            var sb = new StringBuilder();
            try
            {
                int clearedFrames = 0;
                int clearedAreas = 0;

                int nFrames = 0;
                string[] frameNames = Array.Empty<string>();
                _SapModel.FrameObj.GetNameList(ref nFrames, ref frameNames);

                foreach (var frame in frameNames)
                {
                    int r1 = _SapModel.FrameObj.DeleteLoadDistributed(frame, loadPattern);
                    int r2 = 0;
                    try { r2 = _SapModel.FrameObj.DeleteLoadPoint(frame, loadPattern); } catch { /* some versions don't expose this */ }
                    if (r1 == 0 || r2 == 0) clearedFrames++;
                }

                int nAreas = 0;
                string[] areaNames = Array.Empty<string>();
                _SapModel.AreaObj.GetNameList(ref nAreas, ref areaNames);

                foreach (var area in areaNames)
                {
                    int ret = _SapModel.AreaObj.DeleteLoadUniform(area, loadPattern);
                    if (ret == 0) clearedAreas++;
                }

                sb.AppendLine($"✓ Cleared loads for pattern '{loadPattern}'");
                sb.AppendLine($"  Frames (any load types): {clearedFrames}");
                sb.AppendLine($"  Areas (uniform): {clearedAreas}");

                report = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = "Exception in ClearLoadsForPattern:\r\n" + ex;
                return false;
            }
        }

        #endregion
    }
}
