using System;
using System.Collections.Generic;

namespace ETABS_Plugin.Models
{
    /// <summary>
    /// Base reaction results - total reactions at all supports
    /// </summary>
    public class BaseReactionResult
    {
        public string LoadCase { get; set; }
        public string StepType { get; set; }
        public double StepNum { get; set; }
        public double FX { get; set; }  // kN - Global X direction
        public double FY { get; set; }  // kN - Global Y direction
        public double FZ { get; set; }  // kN - Global Z direction (vertical)
        public double MX { get; set; }  // kN·m - Moment about global X
        public double MY { get; set; }  // kN·m - Moment about global Y
        public double MZ { get; set; }  // kN·m - Moment about global Z (torsion)
        public double OriginX { get; set; }  // m - Point about which moments are computed
        public double OriginY { get; set; }
        public double OriginZ { get; set; }
    }

    /// <summary>
    /// Story drift results - lateral deflections between stories
    /// </summary>
    public class StoryDriftResult
    {
        public string Story { get; set; }
        public string LoadCase { get; set; }
        public string StepType { get; set; }
        public double StepNum { get; set; }
        public string Direction { get; set; }  // "X" or "Y"
        public double Drift { get; set; }  // Drift ratio (unitless, e.g., 0.002 = 0.2%)
        public string Label { get; set; }  // Joint where max drift occurs
        public double X { get; set; }  // Absolute displacement at that joint (m)
        public double Y { get; set; }
        public double Z { get; set; }
    }

    /// <summary>
    /// Joint displacement results
    /// </summary>
    public class JointDisplacementResult
    {
        public string Joint { get; set; }
        public string LoadCase { get; set; }
        public string StepType { get; set; }
        public double StepNum { get; set; }
        public double U1 { get; set; }  // m - Translation in global X
        public double U2 { get; set; }  // m - Translation in global Y
        public double U3 { get; set; }  // m - Translation in global Z
        public double R1 { get; set; }  // rad - Rotation about global X
        public double R2 { get; set; }  // rad - Rotation about global Y
        public double R3 { get; set; }  // rad - Rotation about global Z
    }

    /// <summary>
    /// Pier force results - integrated wall forces per pier label
    /// Critical for shear wall design
    /// </summary>
    public class PierForceResult
    {
        public string Story { get; set; }
        public string PierName { get; set; }
        public string LoadCase { get; set; }
        public string Location { get; set; }  // "Top" or "Bottom" of pier segment
        public double P { get; set; }   // kN - Axial force (compression positive typically)
        public double V2 { get; set; }  // kN - Shear in local 2 direction
        public double V3 { get; set; }  // kN - Shear in local 3 direction
        public double T { get; set; }   // kN·m - Torsion about pier axis
        public double M2 { get; set; }  // kN·m - Moment about local 2 axis
        public double M3 { get; set; }  // kN·m - Moment about local 3 axis
    }

    /// <summary>
    /// Spandrel force results - horizontal wall segments (coupling beams)
    /// </summary>
    public class SpandrelForceResult
    {
        public string Story { get; set; }
        public string SpandrelName { get; set; }
        public string LoadCase { get; set; }
        public string Location { get; set; }  // "Left" or "Right" end
        public double P { get; set; }   // kN - Axial force along spandrel
        public double V2 { get; set; }  // kN - Shear
        public double V3 { get; set; }  // kN - Shear
        public double T { get; set; }   // kN·m - Torsion
        public double M2 { get; set; }  // kN·m - Bending moment
        public double M3 { get; set; }  // kN·m - Bending moment
    }

    /// <summary>
    /// Frame force results - beams, columns, braces
    /// </summary>
    public class FrameForceResult
    {
        public string Frame { get; set; }
        public double Station { get; set; }  // 0 to 1 along member (0=I-end, 1=J-end)
        public string Element { get; set; }  // Element name if meshed
        public double ElementStation { get; set; }
        public string LoadCase { get; set; }
        public string StepType { get; set; }
        public double StepNum { get; set; }
        public double P { get; set; }   // kN - Axial force
        public double V2 { get; set; }  // kN - Shear in local 2
        public double V3 { get; set; }  // kN - Shear in local 3
        public double T { get; set; }   // kN·m - Torsion
        public double M2 { get; set; }  // kN·m - Moment about local 2
        public double M3 { get; set; }  // kN·m - Moment about local 3
    }

    /// <summary>
    /// Area/Shell force results - detailed finite element forces in walls/slabs
    /// </summary>
    public class AreaForceResult
    {
        public string Area { get; set; }
        public string Element { get; set; }
        public string PointElm { get; set; }
        public string LoadCase { get; set; }
        public string StepType { get; set; }
        public double StepNum { get; set; }

        // Membrane forces per unit length (kN/m) - in-plane forces
        public double F11 { get; set; }  // Local 1 direction (usually horizontal)
        public double F22 { get; set; }  // Local 2 direction (usually vertical)
        public double F12 { get; set; }  // In-plane shear
        public double FMax { get; set; }  // Principal membrane force (max)
        public double FMin { get; set; }  // Principal membrane force (min)
        public double FAngle { get; set; }  // Angle of FMax from local 1 (degrees)
        public double FVM { get; set; }  // Von Mises equivalent membrane force

        // Bending moments per unit length (kN·m/m) - out-of-plane bending
        public double M11 { get; set; }  // About local 1 axis
        public double M22 { get; set; }  // About local 2 axis
        public double M12 { get; set; }  // Twisting moment
        public double MMax { get; set; }  // Principal moment (max)
        public double MMin { get; set; }  // Principal moment (min)
        public double MAngle { get; set; }  // Angle of MMax from local 1 (degrees)

        // Transverse shear forces per unit length (kN/m) - through-thickness
        public double V13 { get; set; }  // Local 1-3 plane
        public double V23 { get; set; }  // Local 2-3 plane
        public double VMax { get; set; }  // Resultant shear
        public double VAngle { get; set; }  // Angle from local 1 (degrees)
    }

    /// <summary>
    /// Modal analysis results - periods, frequencies, and mass participation
    /// </summary>
    public class ModalResult
    {
        public int Mode { get; set; }
        public string LoadCase { get; set; }
        public double Period { get; set; }      // seconds
        public double Frequency { get; set; }   // Hz
        public double CircFreq { get; set; }    // rad/s (ω = 2πf)
        public double Eigenvalue { get; set; }

        // Mass participation ratios (0 to 1, multiply by 100 for percentage)
        public double UX { get; set; }  // Translational participation in X
        public double UY { get; set; }  // Translational participation in Y
        public double UZ { get; set; }  // Translational participation in Z
        public double RX { get; set; }  // Rotational participation about X
        public double RY { get; set; }  // Rotational participation about Y
        public double RZ { get; set; }  // Rotational participation about Z

        // Cumulative mass participation (sum up to this mode)
        public double SumUX { get; set; }
        public double SumUY { get; set; }
        public double SumUZ { get; set; }
        public double SumRX { get; set; }
        public double SumRY { get; set; }
        public double SumRZ { get; set; }
    }

    /// <summary>
    /// Joint reaction results - support reactions at restrained joints
    /// </summary>
    public class JointReactionResult
    {
        public string Joint { get; set; }
        public string LoadCase { get; set; }
        public string StepType { get; set; }
        public double StepNum { get; set; }
        public double F1 { get; set; }  // kN - Reaction force in global X
        public double F2 { get; set; }  // kN - Reaction force in global Y
        public double F3 { get; set; }  // kN - Reaction force in global Z
        public double M1 { get; set; }  // kN·m - Reaction moment about global X
        public double M2 { get; set; }  // kN·m - Reaction moment about global Y
        public double M3 { get; set; }  // kN·m - Reaction moment about global Z
    }

    /// <summary>
    /// Container for all analysis results
    /// </summary>
    public class AnalysisResults
    {
        public string ModelName { get; set; }
        public DateTime ExtractionTime { get; set; } = DateTime.Now;
        public string Units { get; set; } = "kN, m, C";  // Default metric units

        public List<BaseReactionResult> BaseReactions { get; set; } = new List<BaseReactionResult>();
        public List<StoryDriftResult> StoryDrifts { get; set; } = new List<StoryDriftResult>();
        public List<JointDisplacementResult> JointDisplacements { get; set; } = new List<JointDisplacementResult>();
        public List<JointReactionResult> JointReactions { get; set; } = new List<JointReactionResult>();
        public List<PierForceResult> PierForces { get; set; } = new List<PierForceResult>();
        public List<SpandrelForceResult> SpandrelForces { get; set; } = new List<SpandrelForceResult>();
        public List<FrameForceResult> FrameForces { get; set; } = new List<FrameForceResult>();
        public List<AreaForceResult> AreaForces { get; set; } = new List<AreaForceResult>();
        public List<ModalResult> ModalResults { get; set; } = new List<ModalResult>();

        /// <summary>
        /// Get summary statistics
        /// </summary>
        public string GetSummary()
        {
            return $"Results Summary:\n" +
                   $"  Base Reactions: {BaseReactions.Count}\n" +
                   $"  Story Drifts: {StoryDrifts.Count}\n" +
                   $"  Joint Displacements: {JointDisplacements.Count}\n" +
                   $"  Joint Reactions: {JointReactions.Count}\n" +
                   $"  Pier Forces: {PierForces.Count}\n" +
                   $"  Spandrel Forces: {SpandrelForces.Count}\n" +
                   $"  Frame Forces: {FrameForces.Count}\n" +
                   $"  Area Forces: {AreaForces.Count}\n" +
                   $"  Modal Results: {ModalResults.Count}";
        }
    }
}
