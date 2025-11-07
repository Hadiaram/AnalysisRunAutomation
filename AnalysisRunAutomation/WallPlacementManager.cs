using System;
using ETABSv1;

namespace ETABS_Plugin
{
    public class WallPlacementManager
    {
        private cSapModel _SapModel;

        public WallPlacementManager(cSapModel sapModel)
        {
            _SapModel = sapModel;
        }

        /// <summary>
        /// Places a wall in the ETABS model with specified parameters
        /// </summary>
        public bool PlaceWall(
            double x1, double y1,
            double x2, double y2,
            double x3, double y3,
            double x4, double y4,
            double elevation,
            string materialName,
            double thickness,
            double betaAngle,
            string pierLabel,
            out string report)
        {
            try
            {
                report = "Starting wall placement...\r\n";

                // Set units to kN-m-C
                int ret = _SapModel.SetPresentUnits(eUnits.kN_m_C);
                if (!CheckResult(ret, "SetPresentUnits", ref report))
                    return false;

                // Create wall property
                string wallProp = $"WALL_{(int)(thickness * 1000)}mm_{materialName}";
                ret = _SapModel.PropArea.SetWall(
                    Name: wallProp,
                    WallPropType: eWallPropType.Specified,
                    ShellType: eShellType.ShellThick,
                    MatProp: materialName,
                    Thickness: thickness,
                    color: -1,
                    notes: "",
                    GUID: ""
                );

                if (!CheckResult(ret, "PropArea.SetWall", ref report))
                    return false;

                report += $"Wall property created: {wallProp}\r\n";

                // Create points
                string p1 = "", p2 = "", p3 = "", p4 = "";

                ret = _SapModel.PointObj.AddCartesian(x1, y1, elevation, ref p1);
                if (!CheckResult(ret, "PointObj.AddCartesian p1", ref report))
                    return false;

                ret = _SapModel.PointObj.AddCartesian(x2, y2, elevation, ref p2);
                if (!CheckResult(ret, "PointObj.AddCartesian p2", ref report))
                    return false;

                ret = _SapModel.PointObj.AddCartesian(x3, y3, elevation, ref p3);
                if (!CheckResult(ret, "PointObj.AddCartesian p3", ref report))
                    return false;

                ret = _SapModel.PointObj.AddCartesian(x4, y4, elevation, ref p4);
                if (!CheckResult(ret, "PointObj.AddCartesian p4", ref report))
                    return false;

                report += $"Points created: {p1}, {p2}, {p3}, {p4}\r\n";

                // Create wall area object
                string[] loop = new[] { p1, p2, p3, p4 };
                string wallName = "";

                ret = _SapModel.AreaObj.AddByPoint(
                    NumberPoints: loop.Length,
                    Point: ref loop,
                    Name: ref wallName,
                    PropName: wallProp,
                    UserName: ""
                );

                if (!CheckResult(ret, "AreaObj.AddByPoint", ref report))
                    return false;

                report += $"Wall area created: {wallName}\r\n";

                // Set local axes orientation
                ret = _SapModel.AreaObj.SetLocalAxes(wallName, betaAngle);
                if (!CheckResult(ret, "AreaObj.SetLocalAxes", ref report))
                    return false;

                report += $"Local axes rotated by {betaAngle}°\r\n";

                // Set pier label if provided
                if (!string.IsNullOrEmpty(pierLabel))
                {
                    ret = _SapModel.AreaObj.SetPier(wallName, pierLabel);
                    if (!CheckResult(ret, "AreaObj.SetPier", ref report))
                        return false;

                    report += $"Pier label assigned: {pierLabel}\r\n";
                }

                report += $"\r\n✓ Wall placed successfully!\r\n";
                report += $"  - Name: {wallName}\r\n";
                report += $"  - Property: {wallProp}\r\n";
                report += $"  - Thickness: {thickness * 1000} mm\r\n";
                report += $"  - Elevation: {elevation} m\r\n";

                return true;
            }
            catch (Exception ex)
            {
                report = $"ERROR: {ex.Message}\r\n";
                return false;
            }
        }

        /// <summary>
        /// Gets all available concrete material names from the model
        /// </summary>
        public bool GetConcreteMaterials(out string[] materials, out string error)
        {
            try
            {
                int numNames = 0;
                string[] names = null;

                int ret = _SapModel.PropMaterial.GetNameList(ref numNames, ref names);
                if (ret != 0)
                {
                    error = "Failed to get material list";
                    materials = new string[0];
                    return false;
                }

                // Filter for concrete materials
                var concreteList = new System.Collections.Generic.List<string>();
                for (int i = 0; i < numNames; i++)
                {
                    eMatType matType = eMatType.Steel;
                    int color = 0;
                    string notes = "";
                    string guid = "";

                    ret = _SapModel.PropMaterial.GetMaterial(names[i], ref matType, ref color, ref notes, ref guid);
                    if (ret == 0 && matType == eMatType.Concrete)
                    {
                        concreteList.Add(names[i]);
                    }
                }

                materials = concreteList.ToArray();
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                materials = new string[0];
                return false;
            }
        }

        private bool CheckResult(int ret, string operation, ref string report)
        {
            if (ret != 0)
            {
                report += $"✗ {operation} failed (return code: {ret})\r\n";
                return false;
            }
            return true;
        }
    }
}
