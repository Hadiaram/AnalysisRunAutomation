\# ETABS Results Extraction - Complete Implementation Guide



\## Overview



This guide provides a complete solution for extracting analysis results from ETABS using the v1 API. The implementation follows your existing manager architecture pattern and integrates seamlessly with your 11-step automation workflow.



---



\## Project Structure



Add the following to your \*\*existing ETABS plugin project\*\*:

```

ETABSPlugin/

├── Managers/

│   ├── SectionManager.cs

│   ├── LoadManager.cs

│   ├── BoundaryManager.cs

│   ├── DiaphragmManager.cs

│   ├── MeshManager.cs

│   └── ResultsManager.cs          ← NEW

├── Models/                         ← NEW FOLDER

│   └── ResultsModels.cs            ← NEW

├── Utilities/                      ← NEW FOLDER (Optional)

│   └── ResultsExporter.cs          ← NEW

├── Plugins/

│   ├── CoreWallPlacer.cs

│   └── \[Other plugins]

└── Program.cs or MainWorkflow.cs

```



\*\*Recommendation\*\*: Add to your existing project, NOT a separate class library, because:

\- Maintains consistency with your existing manager pattern

\- Shares the same `ETABSv1` COM reference

\- Simplifies deployment (one DLL)

\- Direct integration with your workflow



---



\## 1. Result Data Models



Create `Models/ResultsModels.cs`:

```csharp

using System;

using System.Collections.Generic;



namespace ETABSPlugin.Models

{

&nbsp;   /// <summary>

&nbsp;   /// Base reaction results - total reactions at all supports

&nbsp;   /// </summary>

&nbsp;   public class BaseReactionResult

&nbsp;   {

&nbsp;       public string LoadCase { get; set; }

&nbsp;       public string StepType { get; set; }

&nbsp;       public double StepNum { get; set; }

&nbsp;       public double FX { get; set; }  // kN - Global X direction

&nbsp;       public double FY { get; set; }  // kN - Global Y direction

&nbsp;       public double FZ { get; set; }  // kN - Global Z direction (vertical)

&nbsp;       public double MX { get; set; }  // kN·m - Moment about global X

&nbsp;       public double MY { get; set; }  // kN·m - Moment about global Y

&nbsp;       public double MZ { get; set; }  // kN·m - Moment about global Z (torsion)

&nbsp;       public double OriginX { get; set; }  // m - Point about which moments are computed

&nbsp;       public double OriginY { get; set; }

&nbsp;       public double OriginZ { get; set; }

&nbsp;   }



&nbsp;   /// <summary>

&nbsp;   /// Story drift results - lateral deflections between stories

&nbsp;   /// </summary>

&nbsp;   public class StoryDriftResult

&nbsp;   {

&nbsp;       public string Story { get; set; }

&nbsp;       public string LoadCase { get; set; }

&nbsp;       public string StepType { get; set; }

&nbsp;       public double StepNum { get; set; }

&nbsp;       public string Direction { get; set; }  // "X" or "Y"

&nbsp;       public double Drift { get; set; }  // Drift ratio (unitless, e.g., 0.002 = 0.2%)

&nbsp;       public string Label { get; set; }  // Joint where max drift occurs

&nbsp;       public double X { get; set; }  // Absolute displacement at that joint (m)

&nbsp;       public double Y { get; set; }

&nbsp;       public double Z { get; set; }

&nbsp;   }



&nbsp;   /// <summary>

&nbsp;   /// Joint displacement results

&nbsp;   /// </summary>

&nbsp;   public class JointDisplacementResult

&nbsp;   {

&nbsp;       public string Joint { get; set; }

&nbsp;       public string LoadCase { get; set; }

&nbsp;       public string StepType { get; set; }

&nbsp;       public double StepNum { get; set; }

&nbsp;       public double U1 { get; set; }  // m - Translation in global X

&nbsp;       public double U2 { get; set; }  // m - Translation in global Y

&nbsp;       public double U3 { get; set; }  // m - Translation in global Z

&nbsp;       public double R1 { get; set; }  // rad - Rotation about global X

&nbsp;       public double R2 { get; set; }  // rad - Rotation about global Y

&nbsp;       public double R3 { get; set; }  // rad - Rotation about global Z

&nbsp;   }



&nbsp;   /// <summary>

&nbsp;   /// Pier force results - integrated wall forces per pier label

&nbsp;   /// Critical for shear wall design

&nbsp;   /// </summary>

&nbsp;   public class PierForceResult

&nbsp;   {

&nbsp;       public string Story { get; set; }

&nbsp;       public string PierName { get; set; }

&nbsp;       public string LoadCase { get; set; }

&nbsp;       public string Location { get; set; }  // "Top" or "Bottom" of pier segment

&nbsp;       public double P { get; set; }   // kN - Axial force (compression positive typically)

&nbsp;       public double V2 { get; set; }  // kN - Shear in local 2 direction

&nbsp;       public double V3 { get; set; }  // kN - Shear in local 3 direction

&nbsp;       public double T { get; set; }   // kN·m - Torsion about pier axis

&nbsp;       public double M2 { get; set; }  // kN·m - Moment about local 2 axis

&nbsp;       public double M3 { get; set; }  // kN·m - Moment about local 3 axis

&nbsp;   }



&nbsp;   /// <summary>

&nbsp;   /// Spandrel force results - horizontal wall segments (coupling beams)

&nbsp;   /// </summary>

&nbsp;   public class SpandrelForceResult

&nbsp;   {

&nbsp;       public string Story { get; set; }

&nbsp;       public string SpandrelName { get; set; }

&nbsp;       public string LoadCase { get; set; }

&nbsp;       public string Location { get; set; }  // "Left" or "Right" end

&nbsp;       public double P { get; set; }   // kN - Axial force along spandrel

&nbsp;       public double V2 { get; set; }  // kN - Shear

&nbsp;       public double V3 { get; set; }  // kN - Shear

&nbsp;       public double T { get; set; }   // kN·m - Torsion

&nbsp;       public double M2 { get; set; }  // kN·m - Bending moment

&nbsp;       public double M3 { get; set; }  // kN·m - Bending moment

&nbsp;   }



&nbsp;   /// <summary>

&nbsp;   /// Frame force results - beams, columns, braces

&nbsp;   /// </summary>

&nbsp;   public class FrameForceResult

&nbsp;   {

&nbsp;       public string Frame { get; set; }

&nbsp;       public double Station { get; set; }  // 0 to 1 along member (0=I-end, 1=J-end)

&nbsp;       public string Element { get; set; }  // Element name if meshed

&nbsp;       public double ElementStation { get; set; }

&nbsp;       public string LoadCase { get; set; }

&nbsp;       public string StepType { get; set; }

&nbsp;       public double StepNum { get; set; }

&nbsp;       public double P { get; set; }   // kN - Axial force

&nbsp;       public double V2 { get; set; }  // kN - Shear in local 2

&nbsp;       public double V3 { get; set; }  // kN - Shear in local 3

&nbsp;       public double T { get; set; }   // kN·m - Torsion

&nbsp;       public double M2 { get; set; }  // kN·m - Moment about local 2

&nbsp;       public double M3 { get; set; }  // kN·m - Moment about local 3

&nbsp;   }



&nbsp;   /// <summary>

&nbsp;   /// Area/Shell force results - detailed finite element forces in walls/slabs

&nbsp;   /// </summary>

&nbsp;   public class AreaForceResult

&nbsp;   {

&nbsp;       public string Area { get; set; }

&nbsp;       public string Element { get; set; }

&nbsp;       public string PointElm { get; set; }

&nbsp;       public string LoadCase { get; set; }

&nbsp;       public string StepType { get; set; }

&nbsp;       public double StepNum { get; set; }

&nbsp;       

&nbsp;       // Membrane forces per unit length (kN/m) - in-plane forces

&nbsp;       public double F11 { get; set; }  // Local 1 direction (usually horizontal)

&nbsp;       public double F22 { get; set; }  // Local 2 direction (usually vertical)

&nbsp;       public double F12 { get; set; }  // In-plane shear

&nbsp;       public double FMax { get; set; }  // Principal membrane force (max)

&nbsp;       public double FMin { get; set; }  // Principal membrane force (min)

&nbsp;       public double FAngle { get; set; }  // Angle of FMax from local 1 (degrees)

&nbsp;       public double FVM { get; set; }  // Von Mises equivalent membrane force

&nbsp;       

&nbsp;       // Bending moments per unit length (kN·m/m) - out-of-plane bending

&nbsp;       public double M11 { get; set; }  // About local 1 axis

&nbsp;       public double M22 { get; set; }  // About local 2 axis

&nbsp;       public double M12 { get; set; }  // Twisting moment

&nbsp;       public double MMax { get; set; }  // Principal moment (max)

&nbsp;       public double MMin { get; set; }  // Principal moment (min)

&nbsp;       public double MAngle { get; set; }  // Angle of MMax from local 1 (degrees)

&nbsp;       

&nbsp;       // Transverse shear forces per unit length (kN/m) - through-thickness

&nbsp;       public double V13 { get; set; }  // Local 1-3 plane

&nbsp;       public double V23 { get; set; }  // Local 2-3 plane

&nbsp;       public double VMax { get; set; }  // Resultant shear

&nbsp;       public double VAngle { get; set; }  // Angle from local 1 (degrees)

&nbsp;   }



&nbsp;   /// <summary>

&nbsp;   /// Modal analysis results - periods, frequencies, and mass participation

&nbsp;   /// </summary>

&nbsp;   public class ModalResult

&nbsp;   {

&nbsp;       public int Mode { get; set; }

&nbsp;       public string LoadCase { get; set; }

&nbsp;       public double Period { get; set; }      // seconds

&nbsp;       public double Frequency { get; set; }   // Hz

&nbsp;       public double CircFreq { get; set; }    // rad/s (ω = 2πf)

&nbsp;       public double Eigenvalue { get; set; }

&nbsp;       

&nbsp;       // Mass participation ratios (0 to 1, multiply by 100 for percentage)

&nbsp;       public double UX { get; set; }  // Translational participation in X

&nbsp;       public double UY { get; set; }  // Translational participation in Y

&nbsp;       public double UZ { get; set; }  // Translational participation in Z

&nbsp;       public double RX { get; set; }  // Rotational participation about X

&nbsp;       public double RY { get; set; }  // Rotational participation about Y

&nbsp;       public double RZ { get; set; }  // Rotational participation about Z

&nbsp;       

&nbsp;       // Cumulative mass participation (sum up to this mode)

&nbsp;       public double SumUX { get; set; }

&nbsp;       public double SumUY { get; set; }

&nbsp;       public double SumUZ { get; set; }

&nbsp;       public double SumRX { get; set; }

&nbsp;       public double SumRY { get; set; }

&nbsp;       public double SumRZ { get; set; }

&nbsp;   }



&nbsp;   /// <summary>

&nbsp;   /// Joint reaction results - support reactions at restrained joints

&nbsp;   /// </summary>

&nbsp;   public class JointReactionResult

&nbsp;   {

&nbsp;       public string Joint { get; set; }

&nbsp;       public string LoadCase { get; set; }

&nbsp;       public string StepType { get; set; }

&nbsp;       public double StepNum { get; set; }

&nbsp;       public double F1 { get; set; }  // kN - Reaction force in global X

&nbsp;       public double F2 { get; set; }  // kN - Reaction force in global Y

&nbsp;       public double F3 { get; set; }  // kN - Reaction force in global Z

&nbsp;       public double M1 { get; set; }  // kN·m - Reaction moment about global X

&nbsp;       public double M2 { get; set; }  // kN·m - Reaction moment about global Y

&nbsp;       public double M3 { get; set; }  // kN·m - Reaction moment about global Z

&nbsp;   }



&nbsp;   /// <summary>

&nbsp;   /// Container for all analysis results

&nbsp;   /// </summary>

&nbsp;   public class AnalysisResults

&nbsp;   {

&nbsp;       public string ModelName { get; set; }

&nbsp;       public DateTime ExtractionTime { get; set; } = DateTime.Now;

&nbsp;       public string Units { get; set; } = "kN, m, C";  // Default metric units

&nbsp;       

&nbsp;       public List<BaseReactionResult> BaseReactions { get; set; } = new List<BaseReactionResult>();

&nbsp;       public List<StoryDriftResult> StoryDrifts { get; set; } = new List<StoryDriftResult>();

&nbsp;       public List<JointDisplacementResult> JointDisplacements { get; set; } = new List<JointDisplacementResult>();

&nbsp;       public List<JointReactionResult> JointReactions { get; set; } = new List<JointReactionResult>();

&nbsp;       public List<PierForceResult> PierForces { get; set; } = new List<PierForceResult>();

&nbsp;       public List<SpandrelForceResult> SpandrelForces { get; set; } = new List<SpandrelForceResult>();

&nbsp;       public List<FrameForceResult> FrameForces { get; set; } = new List<FrameForceResult>();

&nbsp;       public List<AreaForceResult> AreaForces { get; set; } = new List<AreaForceResult>();

&nbsp;       public List<ModalResult> ModalResults { get; set; } = new List<ModalResult>();



&nbsp;       /// <summary>

&nbsp;       /// Get summary statistics

&nbsp;       /// </summary>

&nbsp;       public string GetSummary()

&nbsp;       {

&nbsp;           return $"Results Summary:\\n" +

&nbsp;                  $"  Base Reactions: {BaseReactions.Count}\\n" +

&nbsp;                  $"  Story Drifts: {StoryDrifts.Count}\\n" +

&nbsp;                  $"  Joint Displacements: {JointDisplacements.Count}\\n" +

&nbsp;                  $"  Joint Reactions: {JointReactions.Count}\\n" +

&nbsp;                  $"  Pier Forces: {PierForces.Count}\\n" +

&nbsp;                  $"  Spandrel Forces: {SpandrelForces.Count}\\n" +

&nbsp;                  $"  Frame Forces: {FrameForces.Count}\\n" +

&nbsp;                  $"  Area Forces: {AreaForces.Count}\\n" +

&nbsp;                  $"  Modal Results: {ModalResults.Count}";

&nbsp;       }

&nbsp;   }

}

```



---



\## 2. Results Manager



Create `Managers/ResultsManager.cs`:

```csharp

using System;

using System.Collections.Generic;

using System.Linq;

using ETABSv1;

using ETABSPlugin.Models;



namespace ETABSPlugin.Managers

{

&nbsp;   /// <summary>

&nbsp;   /// Manager for extracting analysis results from ETABS

&nbsp;   /// Handles result setup, extraction, and formatting

&nbsp;   /// </summary>

&nbsp;   public class ResultsManager

&nbsp;   {

&nbsp;       private readonly cSapModel \_model;

&nbsp;       private readonly cAnalysisResults \_results;



&nbsp;       public ResultsManager(cSapModel model)

&nbsp;       {

&nbsp;           \_model = model ?? throw new ArgumentNullException(nameof(model));

&nbsp;           \_results = model.Results;

&nbsp;       }



&nbsp;       #region Setup Methods



&nbsp;       /// <summary>

&nbsp;       /// Select specific load cases for result extraction

&nbsp;       /// </summary>

&nbsp;       public int SelectCasesForOutput(params string\[] caseNames)

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               // Deselect all first

&nbsp;               int ret = \_results.Setup.DeselectAllCasesAndCombosForOutput();

&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"Warning: DeselectAll returned {ret}");

&nbsp;                   return ret;

&nbsp;               }



&nbsp;               // Select each specified case

&nbsp;               foreach (string caseName in caseNames)

&nbsp;               {

&nbsp;                   ret = \_results.Setup.SetCaseSelectedForOutput(caseName);

&nbsp;                   if (ret != 0)

&nbsp;                   {

&nbsp;                       Console.WriteLine($"⚠ Warning: Failed to select case '{caseName}' (code: {ret})");

&nbsp;                   }

&nbsp;                   else

&nbsp;                   {

&nbsp;                       Console.WriteLine($"✓ Selected case: {caseName}");

&nbsp;                   }

&nbsp;               }



&nbsp;               return 0;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error selecting cases: {ex.Message}");

&nbsp;               return -1;

&nbsp;           }

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Select specific load combinations for result extraction

&nbsp;       /// </summary>

&nbsp;       public int SelectCombosForOutput(params string\[] comboNames)

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               // Deselect all first

&nbsp;               int ret = \_results.Setup.DeselectAllCasesAndCombosForOutput();

&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"Warning: DeselectAll returned {ret}");

&nbsp;                   return ret;

&nbsp;               }



&nbsp;               // Select each specified combo

&nbsp;               foreach (string comboName in comboNames)

&nbsp;               {

&nbsp;                   ret = \_results.Setup.SetComboSelectedForOutput(comboName);

&nbsp;                   if (ret != 0)

&nbsp;                   {

&nbsp;                       Console.WriteLine($"⚠ Warning: Failed to select combo '{comboName}' (code: {ret})");

&nbsp;                   }

&nbsp;                   else

&nbsp;                   {

&nbsp;                       Console.WriteLine($"✓ Selected combo: {comboName}");

&nbsp;                   }

&nbsp;               }



&nbsp;               return 0;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error selecting combos: {ex.Message}");

&nbsp;               return -1;

&nbsp;           }

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Select both cases and combos for result extraction

&nbsp;       /// </summary>

&nbsp;       public int SelectCasesAndCombosForOutput(string\[] caseNames, string\[] comboNames)

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               // Deselect all first

&nbsp;               int ret = \_results.Setup.DeselectAllCasesAndCombosForOutput();

&nbsp;               if (ret != 0) return ret;



&nbsp;               // Select cases

&nbsp;               if (caseNames != null)

&nbsp;               {

&nbsp;                   foreach (string caseName in caseNames)

&nbsp;                   {

&nbsp;                       ret = \_results.Setup.SetCaseSelectedForOutput(caseName);

&nbsp;                       if (ret == 0)

&nbsp;                           Console.WriteLine($"✓ Selected case: {caseName}");

&nbsp;                   }

&nbsp;               }



&nbsp;               // Select combos

&nbsp;               if (comboNames != null)

&nbsp;               {

&nbsp;                   foreach (string comboName in comboNames)

&nbsp;                   {

&nbsp;                       ret = \_results.Setup.SetComboSelectedForOutput(comboName);

&nbsp;                       if (ret == 0)

&nbsp;                           Console.WriteLine($"✓ Selected combo: {comboName}");

&nbsp;                   }

&nbsp;               }



&nbsp;               return 0;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error selecting cases/combos: {ex.Message}");

&nbsp;               return -1;

&nbsp;           }

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Set whether multi-valued combos (envelopes, SRSS) return component results

&nbsp;       /// true = return results for each component case

&nbsp;       /// false = return single enveloped result (default)

&nbsp;       /// </summary>

&nbsp;       public int SetMultiValuedComboOption(bool returnComponents)

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               int ret = \_results.Setup.SetOptionMultiValuedCombo(returnComponents);

&nbsp;               if (ret == 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"✓ Multi-valued combo option set to: {(returnComponents ? "Component Results" : "Enveloped Results")}");

&nbsp;               }

&nbsp;               return ret;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error setting multi-valued combo option: {ex.Message}");

&nbsp;               return -1;

&nbsp;           }

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Check if analysis has been run and results are available

&nbsp;       /// </summary>

&nbsp;       public bool AreResultsAvailable()

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               bool runDone = false;

&nbsp;               \_model.Analyze.GetRunDone(ref runDone);

&nbsp;               

&nbsp;               if (!runDone)

&nbsp;               {

&nbsp;                   Console.WriteLine("⚠ Analysis has not been completed or results have been cleared.");

&nbsp;               }

&nbsp;               

&nbsp;               return runDone;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error checking analysis status: {ex.Message}");

&nbsp;               return false;

&nbsp;           }

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Base Reactions



&nbsp;       /// <summary>

&nbsp;       /// Extract base reaction results (total reactions at all supports)

&nbsp;       /// </summary>

&nbsp;       public List<BaseReactionResult> GetBaseReactions()

&nbsp;       {

&nbsp;           var results = new List<BaseReactionResult>();



&nbsp;           try

&nbsp;           {

&nbsp;               int numberResults = 0;

&nbsp;               string\[] loadCase = null;

&nbsp;               string\[] stepType = null;

&nbsp;               double\[] stepNum = null;

&nbsp;               double\[] fx = null, fy = null, fz = null;

&nbsp;               double\[] mx = null, my = null, mz = null;

&nbsp;               double\[] gx = null, gy = null, gz = null;



&nbsp;               int ret = \_results.BaseReact(

&nbsp;                   ref numberResults,

&nbsp;                   ref loadCase,

&nbsp;                   ref stepType,

&nbsp;                   ref stepNum,

&nbsp;                   ref fx, ref fy, ref fz,

&nbsp;                   ref mx, ref my, ref mz,

&nbsp;                   ref gx, ref gy, ref gz);



&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"❌ Error retrieving base reactions. Return code: {ret}");

&nbsp;                   return results;

&nbsp;               }



&nbsp;               for (int i = 0; i < numberResults; i++)

&nbsp;               {

&nbsp;                   results.Add(new BaseReactionResult

&nbsp;                   {

&nbsp;                       LoadCase = loadCase\[i],

&nbsp;                       StepType = stepType\[i],

&nbsp;                       StepNum = stepNum\[i],

&nbsp;                       FX = fx\[i],

&nbsp;                       FY = fy\[i],

&nbsp;                       FZ = fz\[i],

&nbsp;                       MX = mx\[i],

&nbsp;                       MY = my\[i],

&nbsp;                       MZ = mz\[i],

&nbsp;                       OriginX = gx\[i],

&nbsp;                       OriginY = gy\[i],

&nbsp;                       OriginZ = gz\[i]

&nbsp;                   });

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Extracted {numberResults} base reaction results");

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Exception in GetBaseReactions: {ex.Message}");

&nbsp;           }



&nbsp;           return results;

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Story Drifts



&nbsp;       /// <summary>

&nbsp;       /// Extract story drift results

&nbsp;       /// </summary>

&nbsp;       public List<StoryDriftResult> GetStoryDrifts()

&nbsp;       {

&nbsp;           var results = new List<StoryDriftResult>();



&nbsp;           try

&nbsp;           {

&nbsp;               int numberResults = 0;

&nbsp;               string\[] story = null;

&nbsp;               string\[] loadCase = null;

&nbsp;               string\[] stepType = null;

&nbsp;               double\[] stepNum = null;

&nbsp;               string\[] direction = null;

&nbsp;               double\[] drift = null;

&nbsp;               string\[] label = null;

&nbsp;               double\[] x = null, y = null, z = null;



&nbsp;               int ret = \_results.StoryDrifts(

&nbsp;                   ref numberResults,

&nbsp;                   ref story,

&nbsp;                   ref loadCase,

&nbsp;                   ref stepType,

&nbsp;                   ref stepNum,

&nbsp;                   ref direction,

&nbsp;                   ref drift,

&nbsp;                   ref label,

&nbsp;                   ref x, ref y, ref z);



&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"❌ Error retrieving story drifts. Return code: {ret}");

&nbsp;                   return results;

&nbsp;               }



&nbsp;               for (int i = 0; i < numberResults; i++)

&nbsp;               {

&nbsp;                   results.Add(new StoryDriftResult

&nbsp;                   {

&nbsp;                       Story = story\[i],

&nbsp;                       LoadCase = loadCase\[i],

&nbsp;                       StepType = stepType\[i],

&nbsp;                       StepNum = stepNum\[i],

&nbsp;                       Direction = direction\[i],

&nbsp;                       Drift = drift\[i],

&nbsp;                       Label = label\[i],

&nbsp;                       X = x\[i],

&nbsp;                       Y = y\[i],

&nbsp;                       Z = z\[i]

&nbsp;                   });

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Extracted {numberResults} story drift results");

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Exception in GetStoryDrifts: {ex.Message}");

&nbsp;           }



&nbsp;           return results;

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Get maximum story drift across all stories and directions

&nbsp;       /// </summary>

&nbsp;       public StoryDriftResult GetMaximumDrift(List<StoryDriftResult> drifts)

&nbsp;       {

&nbsp;           if (drifts == null || !drifts.Any())

&nbsp;               return null;



&nbsp;           return drifts.OrderByDescending(d => Math.Abs(d.Drift)).FirstOrDefault();

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Joint Displacements



&nbsp;       /// <summary>

&nbsp;       /// Extract joint displacement results

&nbsp;       /// </summary>

&nbsp;       /// <param name="jointName">Specific joint name or "All" for all joints</param>

&nbsp;       /// <param name="itemType">Object, Group, or Selection</param>

&nbsp;       public List<JointDisplacementResult> GetJointDisplacements(

&nbsp;           string jointName = "All", 

&nbsp;           eItemTypeElm itemType = eItemTypeElm.ObjectElm)

&nbsp;       {

&nbsp;           var results = new List<JointDisplacementResult>();



&nbsp;           try

&nbsp;           {

&nbsp;               int numberResults = 0;

&nbsp;               string\[] obj = null;

&nbsp;               string\[] loadCase = null;

&nbsp;               string\[] stepType = null;

&nbsp;               double\[] stepNum = null;

&nbsp;               double\[] u1 = null, u2 = null, u3 = null;

&nbsp;               double\[] r1 = null, r2 = null, r3 = null;



&nbsp;               int ret = \_results.JointDispl(

&nbsp;                   jointName,

&nbsp;                   itemType,

&nbsp;                   ref numberResults,

&nbsp;                   ref obj,

&nbsp;                   ref loadCase,

&nbsp;                   ref stepType,

&nbsp;                   ref stepNum,

&nbsp;                   ref u1, ref u2, ref u3,

&nbsp;                   ref r1, ref r2, ref r3);



&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"❌ Error retrieving joint displacements. Return code: {ret}");

&nbsp;                   return results;

&nbsp;               }



&nbsp;               for (int i = 0; i < numberResults; i++)

&nbsp;               {

&nbsp;                   results.Add(new JointDisplacementResult

&nbsp;                   {

&nbsp;                       Joint = obj\[i],

&nbsp;                       LoadCase = loadCase\[i],

&nbsp;                       StepType = stepType\[i],

&nbsp;                       StepNum = stepNum\[i],

&nbsp;                       U1 = u1\[i],

&nbsp;                       U2 = u2\[i],

&nbsp;                       U3 = u3\[i],

&nbsp;                       R1 = r1\[i],

&nbsp;                       R2 = r2\[i],

&nbsp;                       R3 = r3\[i]

&nbsp;                   });

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Extracted {numberResults} joint displacement results");

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Exception in GetJointDisplacements: {ex.Message}");

&nbsp;           }



&nbsp;           return results;

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Joint Reactions



&nbsp;       /// <summary>

&nbsp;       /// Extract joint reaction results (support reactions at restrained joints)

&nbsp;       /// </summary>

&nbsp;       /// <param name="jointName">Specific joint name or "All" for all joints</param>

&nbsp;       public List<JointReactionResult> GetJointReactions(

&nbsp;           string jointName = "All",

&nbsp;           eItemTypeElm itemType = eItemTypeElm.ObjectElm)

&nbsp;       {

&nbsp;           var results = new List<JointReactionResult>();



&nbsp;           try

&nbsp;           {

&nbsp;               int numberResults = 0;

&nbsp;               string\[] obj = null;

&nbsp;               string\[] loadCase = null;

&nbsp;               string\[] stepType = null;

&nbsp;               double\[] stepNum = null;

&nbsp;               double\[] f1 = null, f2 = null, f3 = null;

&nbsp;               double\[] m1 = null, m2 = null, m3 = null;



&nbsp;               int ret = \_results.JointReact(

&nbsp;                   jointName,

&nbsp;                   itemType,

&nbsp;                   ref numberResults,

&nbsp;                   ref obj,

&nbsp;                   ref loadCase,

&nbsp;                   ref stepType,

&nbsp;                   ref stepNum,

&nbsp;                   ref f1, ref f2, ref f3,

&nbsp;                   ref m1, ref m2, ref m3);



&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"❌ Error retrieving joint reactions. Return code: {ret}");

&nbsp;                   return results;

&nbsp;               }



&nbsp;               for (int i = 0; i < numberResults; i++)

&nbsp;               {

&nbsp;                   results.Add(new JointReactionResult

&nbsp;                   {

&nbsp;                       Joint = obj\[i],

&nbsp;                       LoadCase = loadCase\[i],

&nbsp;                       StepType = stepType\[i],

&nbsp;                       StepNum = stepNum\[i],

&nbsp;                       F1 = f1\[i],

&nbsp;                       F2 = f2\[i],

&nbsp;                       F3 = f3\[i],

&nbsp;                       M1 = m1\[i],

&nbsp;                       M2 = m2\[i],

&nbsp;                       M3 = m3\[i]

&nbsp;                   });

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Extracted {numberResults} joint reaction results");

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Exception in GetJointReactions: {ex.Message}");

&nbsp;           }



&nbsp;           return results;

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Pier Forces



&nbsp;       /// <summary>

&nbsp;       /// Extract pier force results (integrated wall forces per pier label)

&nbsp;       /// Critical for shear wall design - provides P-M diagram inputs

&nbsp;       /// </summary>

&nbsp;       public List<PierForceResult> GetPierForces()

&nbsp;       {

&nbsp;           var results = new List<PierForceResult>();



&nbsp;           try

&nbsp;           {

&nbsp;               int numberResults = 0;

&nbsp;               string\[] storyName = null;

&nbsp;               string\[] pierName = null;

&nbsp;               string\[] loadCase = null;

&nbsp;               string\[] location = null;

&nbsp;               double\[] p = null, v2 = null, v3 = null;

&nbsp;               double\[] t = null, m2 = null, m3 = null;



&nbsp;               int ret = \_results.PierForce(

&nbsp;                   ref numberResults,

&nbsp;                   ref storyName,

&nbsp;                   ref pierName,

&nbsp;                   ref loadCase,

&nbsp;                   ref location,

&nbsp;                   ref p, ref v2, ref v3,

&nbsp;                   ref t, ref m2, ref m3);



&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"❌ Error retrieving pier forces. Return code: {ret}");

&nbsp;                   return results;

&nbsp;               }



&nbsp;               for (int i = 0; i < numberResults; i++)

&nbsp;               {

&nbsp;                   results.Add(new PierForceResult

&nbsp;                   {

&nbsp;                       Story = storyName\[i],

&nbsp;                       PierName = pierName\[i],

&nbsp;                       LoadCase = loadCase\[i],

&nbsp;                       Location = location\[i],

&nbsp;                       P = p\[i],

&nbsp;                       V2 = v2\[i],

&nbsp;                       V3 = v3\[i],

&nbsp;                       T = t\[i],

&nbsp;                       M2 = m2\[i],

&nbsp;                       M3 = m3\[i]

&nbsp;                   });

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Extracted {numberResults} pier force results");

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Exception in GetPierForces: {ex.Message}");

&nbsp;           }



&nbsp;           return results;

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Get pier forces for a specific pier across all stories

&nbsp;       /// </summary>

&nbsp;       public List<PierForceResult> GetPierForcesByName(string pierName, List<PierForceResult> allResults = null)

&nbsp;       {

&nbsp;           var pierForces = allResults ?? GetPierForces();

&nbsp;           return pierForces.Where(p => p.PierName == pierName).OrderBy(p => p.Story).ToList();

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Spandrel Forces



&nbsp;       /// <summary>

&nbsp;       /// Extract spandrel force results (horizontal wall segments, coupling beams)

&nbsp;       /// </summary>

&nbsp;       public List<SpandrelForceResult> GetSpandrelForces()

&nbsp;       {

&nbsp;           var results = new List<SpandrelForceResult>();



&nbsp;           try

&nbsp;           {

&nbsp;               int numberResults = 0;

&nbsp;               string\[] storyName = null;

&nbsp;               string\[] spandrelName = null;

&nbsp;               string\[] loadCase = null;

&nbsp;               string\[] location = null;

&nbsp;               double\[] p = null, v2 = null, v3 = null;

&nbsp;               double\[] t = null, m2 = null, m3 = null;



&nbsp;               int ret = \_results.SpandrelForce(

&nbsp;                   ref numberResults,

&nbsp;                   ref storyName,

&nbsp;                   ref spandrelName,

&nbsp;                   ref loadCase,

&nbsp;                   ref location,

&nbsp;                   ref p, ref v2, ref v3,

&nbsp;                   ref t, ref m2, ref m3);



&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"❌ Error retrieving spandrel forces. Return code: {ret}");

&nbsp;                   return results;

&nbsp;               }



&nbsp;               for (int i = 0; i < numberResults; i++)

&nbsp;               {

&nbsp;                   results.Add(new SpandrelForceResult

&nbsp;                   {

&nbsp;                       Story = storyName\[i],

&nbsp;                       SpandrelName = spandrelName\[i],

&nbsp;                       LoadCase = loadCase\[i],

&nbsp;                       Location = location\[i],

&nbsp;                       P = p\[i],

&nbsp;                       V2 = v2\[i],

&nbsp;                       V3 = v3\[i],

&nbsp;                       T = t\[i],

&nbsp;                       M2 = m2\[i],

&nbsp;                       M3 = m3\[i]

&nbsp;                   });

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Extracted {numberResults} spandrel force results");

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Exception in GetSpandrelForces: {ex.Message}");

&nbsp;           }



&nbsp;           return results;

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Frame Forces



&nbsp;       /// <summary>

&nbsp;       /// Extract frame force results (beams, columns, braces)

&nbsp;       /// </summary>

&nbsp;       /// <param name="frameName">Specific frame name or "All" for all frames</param>

&nbsp;       /// <param name="itemType">Object, Element, Group, or Selection</param>

&nbsp;       public List<FrameForceResult> GetFrameForces(

&nbsp;           string frameName = "All",

&nbsp;           eItemTypeElm itemType = eItemTypeElm.ObjectElm)

&nbsp;       {

&nbsp;           var results = new List<FrameForceResult>();



&nbsp;           try

&nbsp;           {

&nbsp;               int numberResults = 0;

&nbsp;               string\[] obj = null;

&nbsp;               double\[] objSta = null;

&nbsp;               string\[] elm = null;

&nbsp;               double\[] elmSta = null;

&nbsp;               string\[] loadCase = null;

&nbsp;               string\[] stepType = null;

&nbsp;               double\[] stepNum = null;

&nbsp;               double\[] p = null, v2 = null, v3 = null;

&nbsp;               double\[] t = null, m2 = null, m3 = null;



&nbsp;               int ret = \_results.FrameForce(

&nbsp;                   frameName,

&nbsp;                   itemType,

&nbsp;                   ref numberResults,

&nbsp;                   ref obj,

&nbsp;                   ref objSta,

&nbsp;                   ref elm,

&nbsp;                   ref elmSta,

&nbsp;                   ref loadCase,

&nbsp;                   ref stepType,

&nbsp;                   ref stepNum,

&nbsp;                   ref p, ref v2, ref v3,

&nbsp;                   ref t, ref m2, ref m3);



&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"❌ Error retrieving frame forces. Return code: {ret}");

&nbsp;                   return results;

&nbsp;               }



&nbsp;               for (int i = 0; i < numberResults; i++)

&nbsp;               {

&nbsp;                   results.Add(new FrameForceResult

&nbsp;                   {

&nbsp;                       Frame = obj\[i],

&nbsp;                       Station = objSta\[i],

&nbsp;                       Element = elm\[i],

&nbsp;                       ElementStation = elmSta\[i],

&nbsp;                       LoadCase = loadCase\[i],

&nbsp;                       StepType = stepType\[i],

&nbsp;                       StepNum = stepNum\[i],

&nbsp;                       P = p\[i],

&nbsp;                       V2 = v2\[i],

&nbsp;                       V3 = v3\[i],

&nbsp;                       T = t\[i],

&nbsp;                       M2 = m2\[i],

&nbsp;                       M3 = m3\[i]

&nbsp;                   });

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Extracted {numberResults} frame force results");

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Exception in GetFrameForces: {ex.Message}");

&nbsp;           }



&nbsp;           return results;

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Area Forces



&nbsp;       /// <summary>

&nbsp;       /// Extract area/shell force results (detailed finite element forces)

&nbsp;       /// WARNING: This can return very large datasets for meshed walls/slabs

&nbsp;       /// </summary>

&nbsp;       /// <param name="areaName">Specific area name or "All" for all areas</param>

&nbsp;       /// <param name="itemType">Object, Element, Group, or Selection</param>

&nbsp;       public List<AreaForceResult> GetAreaForces(

&nbsp;           string areaName = "All",

&nbsp;           eItemTypeElm itemType = eItemTypeElm.ObjectElm)

&nbsp;       {

&nbsp;           var results = new List<AreaForceResult>();



&nbsp;           try

&nbsp;           {

&nbsp;               int numberResults = 0;

&nbsp;               string\[] obj = null;

&nbsp;               string\[] elm = null;

&nbsp;               string\[] pointElm = null;

&nbsp;               string\[] loadCase = null;

&nbsp;               string\[] stepType = null;

&nbsp;               double\[] stepNum = null;

&nbsp;               

&nbsp;               double\[] f11 = null, f22 = null, f12 = null;

&nbsp;               double\[] fMax = null, fMin = null, fAngle = null, fvm = null;

&nbsp;               double\[] m11 = null, m22 = null, m12 = null;

&nbsp;               double\[] mMax = null, mMin = null, mAngle = null;

&nbsp;               double\[] v13 = null, v23 = null, vMax = null, vAngle = null;



&nbsp;               int ret = \_results.AreaForceShell(

&nbsp;                   areaName,

&nbsp;                   itemType,

&nbsp;                   ref numberResults,

&nbsp;                   ref obj,

&nbsp;                   ref elm,

&nbsp;                   ref pointElm,

&nbsp;                   ref loadCase,

&nbsp;                   ref stepType,

&nbsp;                   ref stepNum,

&nbsp;                   ref f11, ref f22, ref f12,

&nbsp;                   ref fMax, ref fMin, ref fAngle, ref fvm,

&nbsp;                   ref m11, ref m22, ref m12,

&nbsp;                   ref mMax, ref mMin, ref mAngle,

&nbsp;                   ref v13, ref v23, ref vMax, ref vAngle);



&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"❌ Error retrieving area forces. Return code: {ret}");

&nbsp;                   return results;

&nbsp;               }



&nbsp;               for (int i = 0; i < numberResults; i++)

&nbsp;               {

&nbsp;                   results.Add(new AreaForceResult

&nbsp;                   {

&nbsp;                       Area = obj\[i],

&nbsp;                       Element = elm\[i],

&nbsp;                       PointElm = pointElm\[i],

&nbsp;                       LoadCase = loadCase\[i],

&nbsp;                       StepType = stepType\[i],

&nbsp;                       StepNum = stepNum\[i],

&nbsp;                       F11 = f11\[i],

&nbsp;                       F22 = f22\[i],

&nbsp;                       F12 = f12\[i],

&nbsp;                       FMax = fMax\[i],

&nbsp;                       FMin = fMin\[i],

&nbsp;                       FAngle = fAngle\[i],

&nbsp;                       FVM = fvm\[i],

&nbsp;                       M11 = m11\[i],

&nbsp;                       M22 = m22\[i],

&nbsp;                       M12 = m12\[i],

&nbsp;                       MMax = mMax\[i],

&nbsp;                       MMin = mMin\[i],

&nbsp;                       MAngle = mAngle\[i],

&nbsp;                       V13 = v13\[i],

&nbsp;                       V23 = v23\[i],

&nbsp;                       VMax = vMax\[i],

&nbsp;                       VAngle = vAngle\[i]

&nbsp;                   });

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Extracted {numberResults} area force results");

&nbsp;               

&nbsp;               if (numberResults > 1000)

&nbsp;               {

&nbsp;                   Console.WriteLine($"⚠ Large dataset extracted ({numberResults} results). Consider filtering by specific areas.");

&nbsp;               }

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Exception in GetAreaForces: {ex.Message}");

&nbsp;           }



&nbsp;           return results;

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Modal Results



&nbsp;       /// <summary>

&nbsp;       /// Extract modal period and frequency results with mass participation ratios

&nbsp;       /// </summary>

&nbsp;       public List<ModalResult> GetModalResults()

&nbsp;       {

&nbsp;           var results = new List<ModalResult>();



&nbsp;           try

&nbsp;           {

&nbsp;               // First get modal periods

&nbsp;               int numberResults = 0;

&nbsp;               string\[] loadCase = null;

&nbsp;               string\[] stepType = null;

&nbsp;               double\[] stepNum = null;

&nbsp;               double\[] period = null;

&nbsp;               double\[] frequency = null;

&nbsp;               double\[] circFreq = null;

&nbsp;               double\[] eigenvalue = null;



&nbsp;               int ret = \_results.ModalPeriod(

&nbsp;                   ref numberResults,

&nbsp;                   ref loadCase,

&nbsp;                   ref stepType,

&nbsp;                   ref stepNum,

&nbsp;                   ref period,

&nbsp;                   ref frequency,

&nbsp;                   ref circFreq,

&nbsp;                   ref eigenvalue);



&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"❌ Error retrieving modal periods. Return code: {ret}");

&nbsp;                   return results;

&nbsp;               }



&nbsp;               // Create initial results with period data

&nbsp;               for (int i = 0; i < numberResults; i++)

&nbsp;               {

&nbsp;                   results.Add(new ModalResult

&nbsp;                   {

&nbsp;                       Mode = (int)stepNum\[i],

&nbsp;                       LoadCase = loadCase\[i],

&nbsp;                       Period = period\[i],

&nbsp;                       Frequency = frequency\[i],

&nbsp;                       CircFreq = circFreq\[i],

&nbsp;                       Eigenvalue = eigenvalue\[i]

&nbsp;                   });

&nbsp;               }



&nbsp;               // Now get mass participation ratios

&nbsp;               numberResults = 0;

&nbsp;               loadCase = null;

&nbsp;               stepType = null;

&nbsp;               stepNum = null;

&nbsp;               double\[] ux = null, uy = null, uz = null;

&nbsp;               double\[] sumUx = null, sumUy = null, sumUz = null;

&nbsp;               double\[] rx = null, ry = null, rz = null;

&nbsp;               double\[] sumRx = null, sumRy = null, sumRz = null;



&nbsp;               ret = \_results.ModalParticipatingMassRatios(

&nbsp;                   ref numberResults,

&nbsp;                   ref loadCase,

&nbsp;                   ref stepType,

&nbsp;                   ref stepNum,

&nbsp;                   ref ux, ref uy, ref uz,

&nbsp;                   ref sumUx, ref sumUy, ref sumUz,

&nbsp;                   ref rx, ref ry, ref rz,

&nbsp;                   ref sumRx, ref sumRy, ref sumRz);



&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"⚠ Warning: Could not retrieve mass participation ratios. Return code: {ret}");

&nbsp;               }

&nbsp;               else

&nbsp;               {

&nbsp;                   // Add mass participation data to existing results

&nbsp;                   for (int i = 0; i < Math.Min(numberResults, results.Count); i++)

&nbsp;                   {

&nbsp;                       results\[i].UX = ux\[i];

&nbsp;                       results\[i].UY = uy\[i];

&nbsp;                       results\[i].UZ = uz\[i];

&nbsp;                       results\[i].RX = rx\[i];

&nbsp;                       results\[i].RY = ry\[i];

&nbsp;                       results\[i].RZ = rz\[i];

&nbsp;                       results\[i].SumUX = sumUx\[i];

&nbsp;                       results\[i].SumUY = sumUy\[i];

&nbsp;                       results\[i].SumUZ = sumUz\[i];

&nbsp;                       results\[i].SumRX = sumRx\[i];

&nbsp;                       results\[i].SumRY = sumRy\[i];

&nbsp;                       results\[i].SumRZ = sumRz\[i];

&nbsp;                   }

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Extracted {results.Count} modal results");

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Exception in GetModalResults: {ex.Message}");

&nbsp;           }



&nbsp;           return results;

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Check if sufficient modes capture the required mass participation

&nbsp;       /// </summary>

&nbsp;       /// <param name="results">Modal results</param>

&nbsp;       /// <param name="requiredParticipation">Required participation (e.g., 0.90 for 90%)</param>

&nbsp;       /// <param name="direction">"X", "Y", or "Z"</param>

&nbsp;       public bool CheckMassParticipation(List<ModalResult> results, double requiredParticipation = 0.90, string direction = "X")

&nbsp;       {

&nbsp;           if (results == null || !results.Any())

&nbsp;               return false;



&nbsp;           var lastMode = results.OrderBy(r => r.Mode).Last();

&nbsp;           

&nbsp;           double cumulative = direction.ToUpper() switch

&nbsp;           {

&nbsp;               "X" => lastMode.SumUX,

&nbsp;               "Y" => lastMode.SumUY,

&nbsp;               "Z" => lastMode.SumUZ,

&nbsp;               \_ => 0

&nbsp;           };



&nbsp;           bool sufficient = cumulative >= requiredParticipation;

&nbsp;           

&nbsp;           Console.WriteLine($"Mass participation in {direction}: {cumulative \* 100:F1}% " +

&nbsp;                           $"({(sufficient ? "✓ Sufficient" : "⚠ Insufficient")})");

&nbsp;           

&nbsp;           return sufficient;

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Comprehensive Extraction



&nbsp;       /// <summary>

&nbsp;       /// Extract all common results in one call

&nbsp;       /// </summary>

&nbsp;       /// <param name="includeModal">Whether to include modal results</param>

&nbsp;       /// <param name="includeAreaForces">Whether to include detailed area forces (can be large)</param>

&nbsp;       /// <param name="includeFrameForces">Whether to include frame forces</param>

&nbsp;       /// <param name="includeJointDisplacements">Whether to include joint displacements</param>

&nbsp;       /// <param name="includeJointReactions">Whether to include joint reactions</param>

&nbsp;       public AnalysisResults GetAllResults(

&nbsp;           bool includeModal = true,

&nbsp;           bool includeAreaForces = false,

&nbsp;           bool includeFrameForces = false,

&nbsp;           bool includeJointDisplacements = false,

&nbsp;           bool includeJointReactions = false)

&nbsp;       {

&nbsp;           Console.WriteLine("\\n========================================");

&nbsp;           Console.WriteLine("    EXTRACTING ANALYSIS RESULTS");

&nbsp;           Console.WriteLine("========================================\\n");



&nbsp;           var allResults = new AnalysisResults();



&nbsp;           // Get current units

&nbsp;           eUnits units = eUnits.kN\_m\_C;

&nbsp;           \_model.GetPresentUnits(ref units);

&nbsp;           allResults.Units = units.ToString().Replace("\_", ", ");



&nbsp;           // Get model filename if available

&nbsp;           try

&nbsp;           {

&nbsp;               string modelPath = "";

&nbsp;               \_model.GetModelFilename(ref modelPath);

&nbsp;               allResults.ModelName = System.IO.Path.GetFileNameWithoutExtension(modelPath);

&nbsp;           }

&nbsp;           catch { }



&nbsp;           // Extract core results (always included)

&nbsp;           Console.WriteLine("Extracting base reactions...");

&nbsp;           allResults.BaseReactions = GetBaseReactions();



&nbsp;           Console.WriteLine("Extracting story drifts...");

&nbsp;           allResults.StoryDrifts = GetStoryDrifts();



&nbsp;           Console.WriteLine("Extracting pier forces...");

&nbsp;           allResults.PierForces = GetPierForces();



&nbsp;           Console.WriteLine("Extracting spandrel forces...");

&nbsp;           allResults.SpandrelForces = GetSpandrelForces();



&nbsp;           // Optional results

&nbsp;           if (includeModal)

&nbsp;           {

&nbsp;               Console.WriteLine("Extracting modal results...");

&nbsp;               allResults.ModalResults = GetModalResults();

&nbsp;           }



&nbsp;           if (includeFrameForces)

&nbsp;           {

&nbsp;               Console.WriteLine("Extracting frame forces...");

&nbsp;               allResults.FrameForces = GetFrameForces();

&nbsp;           }



&nbsp;           if (includeAreaForces)

&nbsp;           {

&nbsp;               Console.WriteLine("Extracting area forces (this may take a while)...");

&nbsp;               allResults.AreaForces = GetAreaForces();

&nbsp;           }



&nbsp;           if (includeJointDisplacements)

&nbsp;           {

&nbsp;               Console.WriteLine("Extracting joint displacements...");

&nbsp;               allResults.JointDisplacements = GetJointDisplacements();

&nbsp;           }



&nbsp;           if (includeJointReactions)

&nbsp;           {

&nbsp;               Console.WriteLine("Extracting joint reactions...");

&nbsp;               allResults.JointReactions = GetJointReactions();

&nbsp;           }



&nbsp;           Console.WriteLine("\\n========================================");

&nbsp;           Console.WriteLine("    RESULTS EXTRACTION COMPLETE");

&nbsp;           Console.WriteLine("========================================\\n");



&nbsp;           return allResults;

&nbsp;       }



&nbsp;       #endregion



&nbsp;       #region Result Summary and Reporting



&nbsp;       /// <summary>

&nbsp;       /// Print a comprehensive summary of extracted results

&nbsp;       /// </summary>

&nbsp;       public void PrintResultsSummary(AnalysisResults results)

&nbsp;       {

&nbsp;           Console.WriteLine("\\n╔════════════════════════════════════════════════════════════╗");

&nbsp;           Console.WriteLine("║          ANALYSIS RESULTS SUMMARY                          ║");

&nbsp;           Console.WriteLine("╚════════════════════════════════════════════════════════════╝\\n");



&nbsp;           Console.WriteLine($"Model: {results.ModelName ?? "Unknown"}");

&nbsp;           Console.WriteLine($"Extraction Time: {results.ExtractionTime:yyyy-MM-dd HH:mm:ss}");

&nbsp;           Console.WriteLine($"Units: {results.Units}\\n");



&nbsp;           // Base Reactions

&nbsp;           if (results.BaseReactions.Any())

&nbsp;           {

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               Console.WriteLine("BASE REACTIONS:");

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               

&nbsp;               foreach (var reaction in results.BaseReactions)

&nbsp;               {

&nbsp;                   Console.WriteLine($"\\n  Load Case: {reaction.LoadCase}");

&nbsp;                   Console.WriteLine($"    FX = {reaction.FX,10:F2} kN    MX = {reaction.MX,10:F2} kN·m");

&nbsp;                   Console.WriteLine($"    FY = {reaction.FY,10:F2} kN    MY = {reaction.MY,10:F2} kN·m");

&nbsp;                   Console.WriteLine($"    FZ = {reaction.FZ,10:F2} kN    MZ = {reaction.MZ,10:F2} kN·m");

&nbsp;               }

&nbsp;               Console.WriteLine();

&nbsp;           }



&nbsp;           // Story Drifts

&nbsp;           if (results.StoryDrifts.Any())

&nbsp;           {

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               Console.WriteLine("STORY DRIFTS:");

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               

&nbsp;               var driftsByCase = results.StoryDrifts.GroupBy(d => d.LoadCase);

&nbsp;               

&nbsp;               foreach (var caseGroup in driftsByCase)

&nbsp;               {

&nbsp;                   Console.WriteLine($"\\n  Load Case: {caseGroup.Key}");

&nbsp;                   

&nbsp;                   var maxDriftX = caseGroup.Where(d => d.Direction == "X").OrderByDescending(d => Math.Abs(d.Drift)).FirstOrDefault();

&nbsp;                   var maxDriftY = caseGroup.Where(d => d.Direction == "Y").OrderByDescending(d => Math.Abs(d.Drift)).FirstOrDefault();

&nbsp;                   

&nbsp;                   if (maxDriftX != null)

&nbsp;                   {

&nbsp;                       Console.WriteLine($"    Max Drift X: {maxDriftX.Drift \* 100,8:F4}% at {maxDriftX.Story}");

&nbsp;                   }

&nbsp;                   

&nbsp;                   if (maxDriftY != null)

&nbsp;                   {

&nbsp;                       Console.WriteLine($"    Max Drift Y: {maxDriftY.Drift \* 100,8:F4}% at {maxDriftY.Story}");

&nbsp;                   }

&nbsp;               }

&nbsp;               Console.WriteLine();

&nbsp;           }



&nbsp;           // Pier Forces Summary

&nbsp;           if (results.PierForces.Any())

&nbsp;           {

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               Console.WriteLine("PIER FORCES (Sample - First 5 results per case):");

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               

&nbsp;               var piersByCase = results.PierForces.GroupBy(p => p.LoadCase);

&nbsp;               

&nbsp;               foreach (var caseGroup in piersByCase)

&nbsp;               {

&nbsp;                   Console.WriteLine($"\\n  Load Case: {caseGroup.Key}");

&nbsp;                   

&nbsp;                   foreach (var pier in caseGroup.Take(5))

&nbsp;                   {

&nbsp;                       Console.WriteLine($"    {pier.PierName} @ {pier.Story} ({pier.Location}):");

&nbsp;                       Console.WriteLine($"      P  = {pier.P,10:F1} kN    M2 = {pier.M2,10:F1} kN·m");

&nbsp;                       Console.WriteLine($"      V2 = {pier.V2,10:F1} kN    M3 = {pier.M3,10:F1} kN·m");

&nbsp;                       Console.WriteLine($"      V3 = {pier.V3,10:F1} kN    T  = {pier.T,10:F1} kN·m");

&nbsp;                   }

&nbsp;                   

&nbsp;                   if (caseGroup.Count() > 5)

&nbsp;                   {

&nbsp;                       Console.WriteLine($"    ... and {caseGroup.Count() - 5} more pier results for this case");

&nbsp;                   }

&nbsp;               }

&nbsp;               Console.WriteLine();

&nbsp;           }



&nbsp;           // Spandrel Forces Summary

&nbsp;           if (results.SpandrelForces.Any())

&nbsp;           {

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               Console.WriteLine($"SPANDREL FORCES: {results.SpandrelForces.Count} results extracted");

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────\\n");

&nbsp;           }



&nbsp;           // Modal Results

&nbsp;           if (results.ModalResults.Any())

&nbsp;           {

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               Console.WriteLine("MODAL ANALYSIS:");

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               Console.WriteLine($"\\n  {"Mode",-6} {"Period",-10} {"Freq",-10} {"UX%",-8} {"UY%",-8} {"ΣUX%",-8} {"ΣUY%",-8}");

&nbsp;               Console.WriteLine("  " + new string('─', 60));

&nbsp;               

&nbsp;               foreach (var mode in results.ModalResults.Take(12))

&nbsp;               {

&nbsp;                   Console.WriteLine($"  {mode.Mode,-6} {mode.Period,-10:F4} {mode.Frequency,-10:F4} " +

&nbsp;                                   $"{mode.UX \* 100,-8:F2} {mode.UY \* 100,-8:F2} " +

&nbsp;                                   $"{mode.SumUX \* 100,-8:F1} {mode.SumUY \* 100,-8:F1}");

&nbsp;               }

&nbsp;               

&nbsp;               if (results.ModalResults.Count > 12)

&nbsp;               {

&nbsp;                   Console.WriteLine($"\\n  ... and {results.ModalResults.Count - 12} more modes");

&nbsp;               }

&nbsp;               

&nbsp;               // Check mass participation adequacy

&nbsp;               Console.WriteLine();

&nbsp;               CheckMassParticipation(results.ModalResults, 0.90, "X");

&nbsp;               CheckMassParticipation(results.ModalResults, 0.90, "Y");

&nbsp;               Console.WriteLine();

&nbsp;           }



&nbsp;           // Summary counts for other results

&nbsp;           if (results.FrameForces.Any())

&nbsp;           {

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               Console.WriteLine($"FRAME FORCES: {results.FrameForces.Count} results extracted");

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────\\n");

&nbsp;           }



&nbsp;           if (results.AreaForces.Any())

&nbsp;           {

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               Console.WriteLine($"AREA FORCES: {results.AreaForces.Count} results extracted");

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────\\n");

&nbsp;           }



&nbsp;           if (results.JointDisplacements.Any())

&nbsp;           {

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               Console.WriteLine($"JOINT DISPLACEMENTS: {results.JointDisplacements.Count} results extracted");

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────\\n");

&nbsp;           }



&nbsp;           if (results.JointReactions.Any())

&nbsp;           {

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────");

&nbsp;               Console.WriteLine($"JOINT REACTIONS: {results.JointReactions.Count} results extracted");

&nbsp;               Console.WriteLine("─────────────────────────────────────────────────────────────\\n");

&nbsp;           }



&nbsp;           Console.WriteLine("╔════════════════════════════════════════════════════════════╗");

&nbsp;           Console.WriteLine("║                  END OF SUMMARY                            ║");

&nbsp;           Console.WriteLine("╚════════════════════════════════════════════════════════════╝\\n");

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Print simplified summary with just counts

&nbsp;       /// </summary>

&nbsp;       public void PrintQuickSummary(AnalysisResults results)

&nbsp;       {

&nbsp;           Console.WriteLine("\\n=== Quick Results Summary ===");

&nbsp;           Console.WriteLine(results.GetSummary());

&nbsp;           Console.WriteLine("============================\\n");

&nbsp;       }



&nbsp;       #endregion

&nbsp;   }

}

```



---



\## 3. Results Exporter Utility (Optional)



Create `Utilities/ResultsExporter.cs`:

```csharp

using System;

using System.Collections.Generic;

using System.IO;

using System.Text.Json;

using ETABSPlugin.Models;



namespace ETABSPlugin.Utilities

{

&nbsp;   /// <summary>

&nbsp;   /// Utility class for exporting results to various formats

&nbsp;   /// </summary>

&nbsp;   public static class ResultsExporter

&nbsp;   {

&nbsp;       /// <summary>

&nbsp;       /// Export complete results to JSON file

&nbsp;       /// </summary>

&nbsp;       public static bool ExportToJson(AnalysisResults results, string filePath)

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               var options = new JsonSerializerOptions

&nbsp;               {

&nbsp;                   WriteIndented = true,

&nbsp;                   PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

&nbsp;                   DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull

&nbsp;               };



&nbsp;               string jsonString = JsonSerializer.Serialize(results, options);

&nbsp;               

&nbsp;               // Ensure directory exists

&nbsp;               string directory = Path.GetDirectoryName(filePath);

&nbsp;               if (!string.IsNullOrEmpty(directory) \&\& !Directory.Exists(directory))

&nbsp;               {

&nbsp;                   Directory.CreateDirectory(directory);

&nbsp;               }



&nbsp;               File.WriteAllText(filePath, jsonString);

&nbsp;               Console.WriteLine($"✓ Results exported to: {filePath}");

&nbsp;               Console.WriteLine($"  File size: {new FileInfo(filePath).Length / 1024.0:F2} KB");

&nbsp;               

&nbsp;               return true;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error exporting to JSON: {ex.Message}");

&nbsp;               return false;

&nbsp;           }

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Export pier forces only to JSON

&nbsp;       /// </summary>

&nbsp;       public static bool ExportPierForcesToJson(List<PierForceResult> pierForces, string filePath)

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               var options = new JsonSerializerOptions

&nbsp;               {

&nbsp;                   WriteIndented = true,

&nbsp;                   PropertyNamingPolicy = JsonNamingPolicy.CamelCase

&nbsp;               };



&nbsp;               string jsonString = JsonSerializer.Serialize(pierForces, options);

&nbsp;               

&nbsp;               string directory = Path.GetDirectoryName(filePath);

&nbsp;               if (!string.IsNullOrEmpty(directory) \&\& !Directory.Exists(directory))

&nbsp;               {

&nbsp;                   Directory.CreateDirectory(directory);

&nbsp;               }



&nbsp;               File.WriteAllText(filePath, jsonString);

&nbsp;               Console.WriteLine($"✓ Pier forces exported to: {filePath}");

&nbsp;               

&nbsp;               return true;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error exporting pier forces: {ex.Message}");

&nbsp;               return false;

&nbsp;           }

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Export results to CSV format

&nbsp;       /// </summary>

&nbsp;       public static bool ExportPierForcesToCsv(List<PierForceResult> pierForces, string filePath)

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               using (var writer = new StreamWriter(filePath))

&nbsp;               {

&nbsp;                   // Write header

&nbsp;                   writer.WriteLine("Story,PierName,LoadCase,Location,P,V2,V3,T,M2,M3");



&nbsp;                   // Write data

&nbsp;                   foreach (var pier in pierForces)

&nbsp;                   {

&nbsp;                       writer.WriteLine($"{pier.Story},{pier.PierName},{pier.LoadCase},{pier.Location}," +

&nbsp;                                      $"{pier.P:F2},{pier.V2:F2},{pier.V3:F2}," +

&nbsp;                                      $"{pier.T:F2},{pier.M2:F2},{pier.M3:F2}");

&nbsp;                   }

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Pier forces exported to CSV: {filePath}");

&nbsp;               return true;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error exporting to CSV: {ex.Message}");

&nbsp;               return false;

&nbsp;           }

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Export story drifts to CSV

&nbsp;       /// </summary>

&nbsp;       public static bool ExportStoryDriftsToCsv(List<StoryDriftResult> drifts, string filePath)

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               using (var writer = new StreamWriter(filePath))

&nbsp;               {

&nbsp;                   // Write header

&nbsp;                   writer.WriteLine("Story,LoadCase,Direction,Drift,DriftPercent,Label");



&nbsp;                   // Write data

&nbsp;                   foreach (var drift in drifts)

&nbsp;                   {

&nbsp;                       writer.WriteLine($"{drift.Story},{drift.LoadCase},{drift.Direction}," +

&nbsp;                                      $"{drift.Drift:F6},{drift.Drift \* 100:F4},{drift.Label}");

&nbsp;                   }

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Story drifts exported to CSV: {filePath}");

&nbsp;               return true;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error exporting drifts to CSV: {ex.Message}");

&nbsp;               return false;

&nbsp;           }

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Export modal results to CSV

&nbsp;       /// </summary>

&nbsp;       public static bool ExportModalResultsToCsv(List<ModalResult> modalResults, string filePath)

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               using (var writer = new StreamWriter(filePath))

&nbsp;               {

&nbsp;                   // Write header

&nbsp;                   writer.WriteLine("Mode,Period,Frequency,UX,UY,UZ,SumUX,SumUY,SumUZ");



&nbsp;                   // Write data

&nbsp;                   foreach (var mode in modalResults)

&nbsp;                   {

&nbsp;                       writer.WriteLine($"{mode.Mode},{mode.Period:F6},{mode.Frequency:F6}," +

&nbsp;                                      $"{mode.UX:F6},{mode.UY:F6},{mode.UZ:F6}," +

&nbsp;                                      $"{mode.SumUX:F6},{mode.SumUY:F6},{mode.SumUZ:F6}");

&nbsp;                   }

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Modal results exported to CSV: {filePath}");

&nbsp;               return true;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error exporting modal results to CSV: {ex.Message}");

&nbsp;               return false;

&nbsp;           }

&nbsp;       }



&nbsp;       /// <summary>

&nbsp;       /// Create a comprehensive results report as text file

&nbsp;       /// </summary>

&nbsp;       public static bool ExportTextReport(AnalysisResults results, string filePath)

&nbsp;       {

&nbsp;           try

&nbsp;           {

&nbsp;               using (var writer = new StreamWriter(filePath))

&nbsp;               {

&nbsp;                   writer.WriteLine("====================================================");

&nbsp;                   writer.WriteLine("         ETABS ANALYSIS RESULTS REPORT              ");

&nbsp;                   writer.WriteLine("====================================================");

&nbsp;                   writer.WriteLine();

&nbsp;                   writer.WriteLine($"Model: {results.ModelName ?? "Unknown"}");

&nbsp;                   writer.WriteLine($"Extraction Time: {results.ExtractionTime:yyyy-MM-dd HH:mm:ss}");

&nbsp;                   writer.WriteLine($"Units: {results.Units}");

&nbsp;                   writer.WriteLine();



&nbsp;                   // Base Reactions

&nbsp;                   if (results.BaseReactions.Any())

&nbsp;                   {

&nbsp;                       writer.WriteLine("----------------------------------------------------");

&nbsp;                       writer.WriteLine("BASE REACTIONS");

&nbsp;                       writer.WriteLine("----------------------------------------------------");

&nbsp;                       foreach (var reaction in results.BaseReactions)

&nbsp;                       {

&nbsp;                           writer.WriteLine($"\\nLoad Case: {reaction.LoadCase}");

&nbsp;                           writer.WriteLine($"  FX = {reaction.FX,12:F2} kN    MX = {reaction.MX,12:F2} kN·m");

&nbsp;                           writer.WriteLine($"  FY = {reaction.FY,12:F2} kN    MY = {reaction.MY,12:F2} kN·m");

&nbsp;                           writer.WriteLine($"  FZ = {reaction.FZ,12:F2} kN    MZ = {reaction.MZ,12:F2} kN·m");

&nbsp;                       }

&nbsp;                       writer.WriteLine();

&nbsp;                   }



&nbsp;                   // Story Drifts

&nbsp;                   if (results.StoryDrifts.Any())

&nbsp;                   {

&nbsp;                       writer.WriteLine("----------------------------------------------------");

&nbsp;                       writer.WriteLine("STORY DRIFTS");

&nbsp;                       writer.WriteLine("----------------------------------------------------");

&nbsp;                       

&nbsp;                       var driftsByCase = results.StoryDrifts.GroupBy(d => d.LoadCase);

&nbsp;                       foreach (var caseGroup in driftsByCase)

&nbsp;                       {

&nbsp;                           writer.WriteLine($"\\nLoad Case: {caseGroup.Key}");

&nbsp;                           foreach (var drift in caseGroup)

&nbsp;                           {

&nbsp;                               writer.WriteLine($"  {drift.Story,-15} {drift.Direction}: {drift.Drift \* 100,8:F4}%");

&nbsp;                           }

&nbsp;                       }

&nbsp;                       writer.WriteLine();

&nbsp;                   }



&nbsp;                   // Modal Results

&nbsp;                   if (results.ModalResults.Any())

&nbsp;                   {

&nbsp;                       writer.WriteLine("----------------------------------------------------");

&nbsp;                       writer.WriteLine("MODAL ANALYSIS");

&nbsp;                       writer.WriteLine("----------------------------------------------------");

&nbsp;                       writer.WriteLine($"\\n{"Mode",-6} {"Period",-10} {"Freq",-10} {"UX%",-8} {"UY%",-8} {"ΣUX%",-8} {"ΣUY%",-8}");

&nbsp;                       writer.WriteLine(new string('-', 60));

&nbsp;                       

&nbsp;                       foreach (var mode in results.ModalResults)

&nbsp;                       {

&nbsp;                           writer.WriteLine($"{mode.Mode,-6} {mode.Period,-10:F4} {mode.Frequency,-10:F4} " +

&nbsp;                                          $"{mode.UX \* 100,-8:F2} {mode.UY \* 100,-8:F2} " +

&nbsp;                                          $"{mode.SumUX \* 100,-8:F1} {mode.SumUY \* 100,-8:F1}");

&nbsp;                       }

&nbsp;                       writer.WriteLine();

&nbsp;                   }



&nbsp;                   writer.WriteLine("====================================================");

&nbsp;                   writer.WriteLine("                  END OF REPORT                     ");

&nbsp;                   writer.WriteLine("====================================================");

&nbsp;               }



&nbsp;               Console.WriteLine($"✓ Text report exported to: {filePath}");

&nbsp;               return true;

&nbsp;           }

&nbsp;           catch (Exception ex)

&nbsp;           {

&nbsp;               Console.WriteLine($"❌ Error exporting text report: {ex.Message}");

&nbsp;               return false;

&nbsp;           }

&nbsp;       }

&nbsp;   }

}

```



---



\## 4. Usage Examples



\### Example 1: Basic Usage in Workflow



Add to your existing workflow (e.g., after Step 11 - Analysis):

```csharp

using System;

using ETABSv1;

using ETABSPlugin.Managers;

using ETABSPlugin.Models;

using ETABSPlugin.Utilities;

using System.IO;



namespace ETABSPlugin

{

&nbsp;   class WorkflowExample

&nbsp;   {

&nbsp;       static void Main(string\[] args)

&nbsp;       {

&nbsp;           // Connect to ETABS

&nbsp;           cHelper helper = new Helper();

&nbsp;           cOAPI etabs = (cOAPI)System.Runtime.InteropServices.Marshal.GetActiveObject("CSI.ETABS.API.ETABSObject");

&nbsp;           cSapModel model = etabs.SapModel;



&nbsp;           // Your existing steps 1-11 here...

&nbsp;           // (Material creation, sections, loads, analysis, etc.)



&nbsp;           // ===================================================================

&nbsp;           // STEP 12: EXTRACT RESULTS

&nbsp;           // ===================================================================

&nbsp;           

&nbsp;           Console.WriteLine("\\n=== Step 12: Extracting Analysis Results ===\\n");



&nbsp;           // Create ResultsManager

&nbsp;           var resultsManager = new ResultsManager(model);



&nbsp;           // Check if results are available

&nbsp;           if (!resultsManager.AreResultsAvailable())

&nbsp;           {

&nbsp;               Console.WriteLine("⚠ No results available. Running analysis...");

&nbsp;               int ret = model.Analyze.RunAnalysis();

&nbsp;               if (ret != 0)

&nbsp;               {

&nbsp;                   Console.WriteLine($"❌ Analysis failed with code: {ret}");

&nbsp;                   return;

&nbsp;               }

&nbsp;           }



&nbsp;           // Select specific load combinations for extraction

&nbsp;           resultsManager.SelectCombosForOutput("1.4DL", "1.2DL+1.6LL", "EQ-X", "EQ-Y");



&nbsp;           // Extract all relevant results

&nbsp;           var results = resultsManager.GetAllResults(

&nbsp;               includeModal: true,              // Include modal analysis

&nbsp;               includeAreaForces: false,        // Skip detailed area forces (very large)

&nbsp;               includeFrameForces: false,       // Skip frame forces if not needed

&nbsp;               includeJointDisplacements: false,

&nbsp;               includeJointReactions: false

&nbsp;           );



&nbsp;           // Print comprehensive summary

&nbsp;           resultsManager.PrintResultsSummary(results);



&nbsp;           // Export to files

&nbsp;           string outputDir = @"C:\\ETABS\_API\\Results";

&nbsp;           Directory.CreateDirectory(outputDir);



&nbsp;           string timestamp = DateTime.Now.ToString("yyyyMMdd\_HHmmss");

&nbsp;           

&nbsp;           ResultsExporter.ExportToJson(results, Path.Combine(outputDir, $"results\_{timestamp}.json"));

&nbsp;           ResultsExporter.ExportTextReport(results, Path.Combine(outputDir, $"report\_{timestamp}.txt"));

&nbsp;           ResultsExporter.ExportPierForcesToCsv(results.PierForces, Path.Combine(outputDir, $"pier\_forces\_{timestamp}.csv"));

&nbsp;           ResultsExporter.ExportStoryDriftsToCsv(results.StoryDrifts, Path.Combine(outputDir, $"story\_drifts\_{timestamp}.csv"));



&nbsp;           if (results.ModalResults.Any())

&nbsp;           {

&nbsp;               ResultsExporter.ExportModalResultsToCsv(results.ModalResults, Path.Combine(outputDir, $"modal\_{timestamp}.csv"));

&nbsp;           }



&nbsp;           Console.WriteLine("\\n✓ Results extraction and export complete!");

&nbsp;       }

&nbsp;   }

}

```



\### Example 2: Extracting Specific Results

```csharp

// Extract only pier forces for a specific load combination

var resultsManager = new ResultsManager(model);

resultsManager.SelectCombosForOutput("1.2DL+1.6LL");



var pierForces = resultsManager.GetPierForces();



// Filter for specific pier

var p1Forces = resultsManager.GetPierForcesByName("P1", pierForces);



Console.WriteLine("\\n=== Pier P1 Forces ===");

foreach (var force in p1Forces)

{

&nbsp;   Console.WriteLine($"{force.Story} ({force.Location}): P={force.P:F1} kN, V3={force.V3:F1} kN, M3={force.M3:F1} kN·m");

}

```



\### Example 3: Checking Story Drifts

```csharp

var resultsManager = new ResultsManager(model);

resultsManager.SelectCombosForOutput("EQ-X", "EQ-Y");



var drifts = resultsManager.GetStoryDrifts();



// Find maximum drift

var maxDrift = resultsManager.GetMaximumDrift(drifts);



if (maxDrift != null)

{

&nbsp;   Console.WriteLine($"\\nMaximum Drift: {maxDrift.Drift \* 100:F4}% at {maxDrift.Story} in {maxDrift.Direction} direction");

&nbsp;   Console.WriteLine($"Load Case: {maxDrift.LoadCase}");

&nbsp;   

&nbsp;   // Check against code limits (e.g., 0.5% for seismic)

&nbsp;   double limitPercent = 0.5;

&nbsp;   if (maxDrift.Drift \* 100 > limitPercent)

&nbsp;   {

&nbsp;       Console.WriteLine($"⚠ WARNING: Drift exceeds {limitPercent}% limit!");

&nbsp;   }

&nbsp;   else

&nbsp;   {

&nbsp;       Console.WriteLine($"✓ Drift is within {limitPercent}% limit");

&nbsp;   }

}

```



\### Example 4: Modal Analysis Check

```csharp

var resultsManager = new ResultsManager(model);

resultsManager.SelectCasesForOutput("Modal");



var modalResults = resultsManager.GetModalResults();



Console.WriteLine("\\n=== Modal Analysis Summary ===");

Console.WriteLine($"Total modes analyzed: {modalResults.Count}");

Console.WriteLine($"\\nFirst 3 periods:");



foreach (var mode in modalResults.Take(3))

{

&nbsp;   Console.WriteLine($"  Mode {mode.Mode}: T = {mode.Period:F4}s (f = {mode.Frequency:F3} Hz)");

}



// Check mass participation

bool xOk = resultsManager.CheckMassParticipation(modalResults, 0.90, "X");

bool yOk = resultsManager.CheckMassParticipation(modalResults, 0.90, "Y");



if (!xOk || !yOk)

{

&nbsp;   Console.WriteLine("\\n⚠ WARNING: Insufficient mass participation. Consider adding more modes.");

}

```



\### Example 5: Integration with Existing Managers

```csharp

// After your existing workflow setup

var sectionManager = new SectionManager(model);

var loadManager = new LoadManager(model);

// ... other managers ...



// Run your automation workflow

// ...



// After analysis is complete

var resultsManager = new ResultsManager(model);



// Select the combinations you created with LoadManager

resultsManager.SelectCombosForOutput("1.4DL", "1.2DL+1.6LL");



// Extract and process results

var results = resultsManager.GetAllResults(includeModal: true);



// Check specific results

var maxDrift = resultsManager.GetMaximumDrift(results.StoryDrifts);

Console.WriteLine($"Max drift: {maxDrift?.Drift \* 100:F4}%");



// Export for documentation

ResultsExporter.ExportToJson(results, outputPath);

```



---



\## 5. Key Features Summary



\### ✅ Comprehensive Result Types

\- \*\*Base Reactions\*\* - Total support reactions

\- \*\*Story Drifts\*\* - Inter-story drift ratios

\- \*\*Pier Forces\*\* - Integrated wall forces (P-M diagrams)

\- \*\*Spandrel Forces\*\* - Coupling beam forces

\- \*\*Frame Forces\*\* - Beam/column internal forces

\- \*\*Area Forces\*\* - Detailed shell element forces

\- \*\*Modal Results\*\* - Periods, frequencies, mass participation

\- \*\*Joint Displacements\*\* - Nodal displacements

\- \*\*Joint Reactions\*\* - Support reactions



\### ✅ Flexible Extraction

\- Select specific load cases or combinations

\- Extract all results or specific types

\- Optional inclusion of large datasets



\### ✅ Idempotent \& Robust

\- Existence checking before extraction

\- Comprehensive error handling

\- Clear status messages



\### ✅ Multiple Export Formats

\- JSON (structured data)

\- CSV (spreadsheet-friendly)

\- Text reports (human-readable)



\### ✅ Integration Ready

\- Follows your existing manager pattern

\- Works with your 11-step workflow

\- Compatible with all your existing managers



---



\## 6. Important Notes



\### Units

All results are returned in the model's current units (typically kN, m, C). The `AnalysisResults` object includes a `Units` property that records the units at extraction time.



\### Performance Considerations

\- \*\*Area Forces\*\* can be very large for meshed models (thousands of elements). Set `includeAreaForces = false` unless specifically needed.

\- \*\*Frame Forces\*\* are moderate in size but can grow with many members.

\- \*\*Pier/Spandrel Forces\*\* are compact and recommended for wall design.



\### Result Selection

Always call `SelectCasesForOutput()` or `SelectCombosForOutput()` before extraction to specify which load cases/combos you want. Otherwise, ETABS may return results for all cases, which can be overwhelming.



\### Error Codes

\- \*\*Return code 0\*\* = Success

\- \*\*Non-zero return\*\* = Error or warning

\- Check console output for specific error messages



\### Typical Workflow Order

1\. Verify analysis is complete (`AreResultsAvailable()`)

2\. Select cases/combos for output

3\. Extract specific result types or use `GetAllResults()`

4\. Print summary if desired

5\. Export to files for documentation



---



\## 7. Next Steps



1\. \*\*Add the three files\*\* to your existing project:

&nbsp;  - `Models/ResultsModels.cs`

&nbsp;  - `Managers/ResultsManager.cs`

&nbsp;  - `Utilities/ResultsExporter.cs`



2\. \*\*Add NuGet package\*\* for JSON serialization (if not already present):

```

&nbsp;  Install-Package System.Text.Json

```



3\. \*\*Integrate into your workflow\*\* by adding Step 12 after analysis



4\. \*\*Test with a simple model\*\* to verify extraction works



5\. \*\*Customize as needed\*\* - Add specific result filtering, custom reports, or additional export formats



This complete solution provides professional-grade results extraction that integrates seamlessly with your existing ETABS automation architecture. Let me know if you need any clarifications or additional features!

