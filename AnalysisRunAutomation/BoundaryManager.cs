using System;
using ETABSv1;

namespace ETABS_Plugin
{
    public class BoundaryManager
    {
        private readonly cSapModel _SapModel;

        public BoundaryManager(cSapModel sapModel) => _SapModel = sapModel;

        // Apply fixed restraints to all joints at the lowest elevation (within tolMm)
        public bool ApplyFixedSupportsAtBase(double tolMm, out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                // 0) Ensure model is unlocked
                bool isLocked = _SapModel.GetModelIsLocked();
                if (isLocked) _SapModel.SetModelIsLocked(false);

                // 1) Work in N-mm-°C so mm coords/tolerance are correct
                eUnits prev = _SapModel.GetPresentUnits();
                _SapModel.SetPresentUnits(eUnits.N_mm_C);

                try
                {
                    // 2) Gather all point names
                    int n = 0; string[] ptNames = Array.Empty<string>();
                    int ret = _SapModel.PointObj.GetNameList(ref n, ref ptNames);
                    if (ret != 0 || n == 0 || ptNames == null || ptNames.Length == 0)
                    {
                        report = "No point objects found.";
                        return false;
                    }

                    // 3) Find minimum Z
                    double minZ = double.PositiveInfinity;
                    foreach (var p in ptNames)
                    {
                        double x = 0, y = 0, z = 0;
                        _SapModel.PointObj.GetCoordCartesian(p, ref x, ref y, ref z);
                        if (z < minZ) minZ = z;
                    }
                    double zCut = minZ + tolMm;
                    sb.AppendLine($"Base Z ≈ {minZ:0.###} mm (tolerance ±{tolMm} mm)");

                    // 4) Fixed restraint array (UX, UY, UZ, RX, RY, RZ)
                    bool[] fix = new[] { true, true, true, true, true, true };

                    // 5) Apply to all points at base
                    int count = 0, errors = 0;
                    foreach (var p in ptNames)
                    {
                        double x = 0, y = 0, z = 0;
                        _SapModel.PointObj.GetCoordCartesian(p, ref x, ref y, ref z);
                        if (z <= zCut)
                        {
                            int r = _SapModel.PointObj.SetRestraint(p, ref fix);
                            if (r == 0) count++; else errors++;
                        }
                    }
                    sb.AppendLine($"Applied fixed BCs to {count} joints at base." + (errors > 0 ? $" (errors: {errors})" : ""));
                    report = sb.ToString();
                    return errors == 0;
                }
                finally
                {
                    // 6) Restore units
                    _SapModel.SetPresentUnits(prev);
                }
            }
            catch (Exception ex)
            {
                report = "Exception in ApplyFixedSupportsAtBase:\r\n" + ex;
                return false;
            }
        }

        // Optional: clear all restraints (useful for reruns during exploration)
        public bool ClearAllJointRestraints(out string report)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                bool[] free = new[] { false, false, false, false, false, false };

                int n = 0; string[] ptNames = Array.Empty<string>();
                _SapModel.PointObj.GetNameList(ref n, ref ptNames);
                int count = 0, errors = 0;
                foreach (var p in ptNames)
                {
                    int r = _SapModel.PointObj.SetRestraint(p, ref free);
                    if (r == 0) count++; else errors++;
                }
                sb.AppendLine($"Cleared restraints on {count} joints." + (errors > 0 ? $" (errors: {errors})" : ""));
                report = sb.ToString();
                return errors == 0;
            }
            catch (Exception ex)
            {
                report = "Exception in ClearAllJointRestraints:\r\n" + ex;
                return false;
            }
        }
    }
}
