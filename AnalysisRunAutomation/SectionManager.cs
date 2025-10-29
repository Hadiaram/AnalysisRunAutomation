using System;
using System.Collections.Generic;
using ETABSv1;
using System.Diagnostics;

namespace ETABS_Plugin
{
    public class SectionManager
    {
        private cSapModel _SapModel;

        public SectionManager(cSapModel sapModel)
        {
            _SapModel = sapModel;
        }

        #region Material Creation

        /// <summary>
        /// Creates standard concrete material from library (Metric)
        /// </summary>
        public bool CreateConcreteMaterial(string materialName, string region = "Euro", string standard = "EN 1992-1-1", string grade = "C25/30")
        {
            try
            {
                string tempName = materialName;
                int result = _SapModel.PropMaterial.AddMaterial(ref tempName, eMatType.Concrete, region, standard, grade);

                MessageBox.Show($"Result code: {result}, Final name: {tempName}");

                return result == 0;
            }
            catch (Exception ex)
            {
                // Log error appropriately
                return false;
            }
        }

        /// <summary>
        /// Creates standard steel material from library (Metric)
        /// </summary>
        public bool CreateSteelMaterial(string materialName, string region = "Euro", string standard = "EN 10025-2", string grade = "S355")
        {
            try
            {
                string tempName = materialName;
                int result = _SapModel.PropMaterial.AddMaterial(ref tempName, eMatType.Steel, region, standard, grade);
                return result == 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Replace the CreateCustomConcreteWithName method in SectionManager.cs

        /// <summary>
        /// Creates custom concrete material with specific properties (Metric units)
        /// </summary>
        /// <summary>
        /// Creates custom concrete material with specific properties (Metric units)
        /// </summary>
        public bool CreateCustomConcreteWithName(ref string materialName, double fc, double density = 2400.0)
        {
            try
            {
                // Check if material already exists
                if (MaterialExists(materialName))
                {
                    System.Windows.Forms.MessageBox.Show(
                        $"Material '{materialName}' already exists. Using existing material.",
                        "Material Exists",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information);
                    return true; // Return success - material exists and can be used
                }

                // Make a copy of the material name for the API call
                string tempName = materialName;

                // First, try to add the material using AddMaterial
                int result = _SapModel.PropMaterial.AddMaterial(
                    ref tempName,
                    eMatType.Concrete,
                    "US",
                    "ACI 318-19",
                    "4000");

                if (result != 0)
                {
                    System.Windows.Forms.MessageBox.Show(
                        $"Failed to create material. Error code: {result}\nMaterial name attempted: {tempName}",
                        "Material Creation Error");
                    return false;
                }

                materialName = tempName;

                // Set properties
                double E = 4700.0 * Math.Sqrt(fc);
                double poisson = 0.2;
                double thermalCoeff = 9.9e-6;

                result = _SapModel.PropMaterial.SetMPIsotropic(tempName, E, poisson, thermalCoeff);
                if (result != 0)
                {
                    System.Windows.Forms.MessageBox.Show(
                        $"Failed to set isotropic properties. Error code: {result}",
                        "Material Property Error");
                    return false;
                }

                result = _SapModel.PropMaterial.SetOConcrete_1(
                    tempName, fc, false, 0, 0, 0, 0, 0, 0);

                if (result != 0)
                {
                    System.Windows.Forms.MessageBox.Show(
                        $"Failed to set concrete properties. Error code: {result}",
                        "Concrete Property Error");
                    return false;
                }

                double weightPerVolume = density * 9.81 / 1e9;
                result = _SapModel.PropMaterial.SetWeightAndMass(tempName, 1, weightPerVolume);

                if (result != 0)
                {
                    System.Windows.Forms.MessageBox.Show(
                        $"Failed to set weight and mass. Error code: {result}",
                        "Material Weight Error");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Exception in CreateCustomConcreteWithName:\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                    "Exception");
                return false;
            }
        }


        #endregion

        #region Frame Sections

        /// <summary>
        /// Creates rectangular column sections in standard sizes (metric: mm)
        /// </summary>
        /// <summary>
        /// Creates rectangular column sections in standard sizes (metric: mm)
        /// </summary>
        public bool CreateStandardColumns(string concreteMaterial)
        {
            var columnSizes = new Dictionary<string, Tuple<double, double>>
    {
        {"COL-300x300", new Tuple<double, double>(300, 300)},
        {"COL-350x350", new Tuple<double, double>(350, 350)},
        {"COL-400x400", new Tuple<double, double>(400, 400)},
        {"COL-450x450", new Tuple<double, double>(450, 450)},
        {"COL-500x500", new Tuple<double, double>(500, 500)},
        {"COL-600x600", new Tuple<double, double>(600, 600)}
    };

            int created = 0;
            int skipped = 0;

            foreach (var column in columnSizes)
            {
                try
                {
                    // Check if section already exists
                    if (FrameSectionExists(column.Key))
                    {
                        skipped++;
                        continue; // Skip existing section
                    }

                    int result = _SapModel.PropFrame.SetRectangle(
                        column.Key,
                        concreteMaterial,
                        column.Value.Item1,
                        column.Value.Item2,
                        -1,
                        $"Standard rectangular column {column.Value.Item1}x{column.Value.Item2}mm");

                    if (result != 0)
                    {
                        System.Windows.Forms.MessageBox.Show($"Failed to create {column.Key}, error: {result}");
                        return false;
                    }
                    created++;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Exception creating {column.Key}: {ex.Message}");
                    return false;
                }
            }

            if (skipped > 0)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Column sections: {created} created, {skipped} already existed",
                    "Section Creation",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }

            return true;
        }

        /// <summary>
        /// Creates rectangular beam sections in standard sizes (metric: mm)
        /// </summary>
        public bool CreateStandardBeams(string concreteMaterial)
        {
            var beamSizes = new Dictionary<string, Tuple<double, double>>
    {
        {"BEAM-300x600", new Tuple<double, double>(600, 300)},
        {"BEAM-350x700", new Tuple<double, double>(700, 350)},
        {"BEAM-400x800", new Tuple<double, double>(800, 400)},
        {"BEAM-450x900", new Tuple<double, double>(900, 450)},
        {"BEAM-500x1000", new Tuple<double, double>(1000, 500)},
        {"BEAM-600x1200", new Tuple<double, double>(1200, 600)}
    };

            int created = 0;
            int skipped = 0;

            foreach (var beam in beamSizes)
            {
                try
                {
                    if (FrameSectionExists(beam.Key))
                    {
                        skipped++;
                        continue;
                    }

                    int result = _SapModel.PropFrame.SetRectangle(
                        beam.Key,
                        concreteMaterial,
                        beam.Value.Item1,
                        beam.Value.Item2,
                        -1,
                        $"Standard rectangular beam {beam.Value.Item2}x{beam.Value.Item1}mm");

                    if (result != 0) return false;
                    created++;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            if (skipped > 0)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Beam sections: {created} created, {skipped} already existed",
                    "Section Creation",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }

            return true;
        }

        #endregion

        #region Shell Sections

        /// <summary>
        /// Creates slab sections with various thicknesses (metric: mm)
        /// </summary>
        public bool CreateStandardSlabs(string concreteMaterial)
        {
            var slabThicknesses = new double[] { 100, 125, 150, 200, 250, 300 };

            int created = 0;
            int skipped = 0;

            foreach (double thickness in slabThicknesses)
            {
                try
                {
                    string slabName = $"SLAB-{thickness}MM";

                    if (AreaSectionExists(slabName))
                    {
                        skipped++;
                        continue;
                    }

                    int result = _SapModel.PropArea.SetSlab(
                        slabName,
                        eSlabType.Slab,
                        eShellType.ShellThin,
                        concreteMaterial,
                        thickness,
                        -1,
                        $"Standard slab {thickness}mm thick");

                    if (result != 0) return false;
                    created++;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            if (skipped > 0)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Slab sections: {created} created, {skipped} already existed",
                    "Section Creation",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }

            return true;
        }

        /// <summary>
        /// Creates wall sections with various thicknesses (metric: mm)
        /// </summary>
        public bool CreateStandardWalls(string concreteMaterial)
        {
            var wallThicknesses = new double[] { 150, 200, 250, 300, 350, 400, 450, 600 };

            int created = 0;
            int skipped = 0;

            foreach (double thickness in wallThicknesses)
            {
                try
                {
                    string wallName = $"WALL-{thickness}MM";

                    if (AreaSectionExists(wallName))
                    {
                        skipped++;
                        continue;
                    }

                    int result = _SapModel.PropArea.SetWall(
                        wallName,
                        eWallPropType.Specified,
                        eShellType.ShellThin,
                        concreteMaterial,
                        thickness,
                        -1,
                        $"Standard wall {thickness}mm thick");

                    if (result != 0) return false;
                    created++;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            if (skipped > 0)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Wall sections: {created} created, {skipped} already existed",
                    "Section Creation",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }

            return true;
        }
        #endregion

        #region Main Creation Method

        /// <summary>
        /// Creates all standard sections with specified concrete strength (Metric: MPa)
        /// </summary>
        public bool CreateAllStandardSections(double concreteStrength = 25)
        {
            try
            {
                // Create materials
                string concMaterial = $"CONC-C{concreteStrength}";
                if (!CreateCustomConcreteWithName(ref concMaterial, concreteStrength))
                    return false;

                string steelMaterial = "STEEL-S355";
                if (!CreateSteelMaterial(steelMaterial))
                    return false;

                // Create all section types
                if (!CreateStandardColumns(concMaterial)) return false;
                if (!CreateStandardBeams(concMaterial)) return false;
                if (!CreateStandardSlabs(concMaterial)) return false;
                if (!CreateStandardWalls(concMaterial)) return false;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Existence Checks

        /// <summary>
        /// Checks if a material already exists
        /// </summary>
        private bool MaterialExists(string materialName)
        {
            try
            {
                if (_SapModel.GetModelIsLocked()) _SapModel.SetModelIsLocked(false);

                string target = (materialName ?? "").Trim();
                int n = 0;
                string[] names = Array.Empty<string>();
                int ret = _SapModel.PropMaterial.GetNameList(ref n, ref names);

                // Log issues—don't silently fail
                if (ret != 0 || names == null)
                {
                   System.Diagnostics.Debug.WriteLine($"[MaterialExists] GetNameList failed. ret={ret}\r\n");
                    return false;
                }

                foreach (var raw in names)
                {
                    var s = (raw ?? "").Trim();
                    if (string.Equals(s, target, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MaterialExists] Exception: {ex.Message}\r\n");
                return false;
            }
        }

        /// <summary>
        /// Checks if a frame section already exists
        /// </summary>
        private bool FrameSectionExists(string sectionName)
        {
            try
            {
                int numSections = 0;
                string[] sectionNames = Array.Empty<string>();
                _SapModel.PropFrame.GetNameList(ref numSections, ref sectionNames);

                foreach (var name in sectionNames)
                {
                    if (name.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
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
        /// Checks if an area section already exists
        /// </summary>
        private bool AreaSectionExists(string sectionName)
        {
            try
            {
                int numSections = 0;
                string[] sectionNames = Array.Empty<string>();
                _SapModel.PropArea.GetNameList(ref numSections, ref sectionNames);

                foreach (var name in sectionNames)
                {
                    if (name.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}