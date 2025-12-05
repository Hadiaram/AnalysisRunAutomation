using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ETABSv1;

namespace ETABS_Plugin
{
    public partial class Form1 : Form
    {
        private cPluginCallback _Plugin = null;
        private cSapModel _SapModel = null;
        private SectionManager _SectionManager = null;
        private LoadManager _LoadManager = null;
        private BoundaryManager _BoundaryManager = null;
        private DiaphragmManager _DiaphragmManager = null;
        private MeshManager _MeshManager = null;
        private SectionAssignmentManager _SectionAssignmentManager = null;
        private LoadAssignmentManager _LoadAssignmentManager = null;
        private MassSourceManager _MassSourceManager = null;
        private AnalysisManager _AnalysisManager = null;

        public Form1(cSapModel SapModel, cPluginCallback Plugin)
        {
            _Plugin = Plugin;
            _SapModel = SapModel;
            _SectionManager = new SectionManager(_SapModel);
            _LoadManager = new LoadManager(_SapModel);
            _BoundaryManager = new BoundaryManager(_SapModel);
            _DiaphragmManager = new DiaphragmManager(_SapModel);
            _MeshManager = new MeshManager(_SapModel);
            _SectionAssignmentManager = new SectionAssignmentManager(_SapModel);
            _LoadAssignmentManager = new LoadAssignmentManager(_SapModel);
            _MassSourceManager = new MassSourceManager(_SapModel);
            _AnalysisManager = new AnalysisManager(_SapModel);

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Example usage - create all standard sections
            bool success = _SectionManager.CreateAllStandardSections(4000);
            if (success)
            {
                MessageBox.Show("Standard sections created successfully!");
            }
            else
            {
                MessageBox.Show("Error creating sections");
            }

        }

        // Button click to create sections based on user selection
        private void btnCreateSections_Click(object sender, EventArgs e)
        {
            try
            {
                // ADD DEBUG CODE HERE - at the very beginning
                txtStatus.AppendText("=== DEBUG INFO ===\r\n");

                // Test basic model access first
                if (_SapModel == null)
                {
                    txtStatus.AppendText("ERROR: _SapModel is null\r\n");
                    return;
                }
                txtStatus.AppendText("✓ _SapModel is valid\r\n");

                if (_SapModel.PropMaterial == null)
                {
                    txtStatus.AppendText("ERROR: PropMaterial is null\r\n");
                    return;
                }
                txtStatus.AppendText("✓ PropMaterial is accessible\r\n");

                // Test simple material creation with basic US concrete
                txtStatus.AppendText("Testing basic material creation...\r\n");
                try
                {
                    string testName = "DEBUG_TEST_CONCRETE";
                    int result = _SapModel.PropMaterial.AddMaterial(ref testName, eMatType.Concrete, "US", "ACI 318-19", "4000");
                    txtStatus.AppendText($"AddMaterial result code: {result}, Final name: {testName}\r\n");

                    if (result == 0)
                    {
                        txtStatus.AppendText("✓ Basic material creation works!\r\n");
                    }
                    else
                    {
                        txtStatus.AppendText($"⚠ Material creation returned error code: {result}\r\n");
                    }
                }
                catch (Exception debugEx)
                {
                    txtStatus.AppendText($"✗ AddMaterial exception: {debugEx.Message}\r\n");
                }

                txtStatus.AppendText("=== END DEBUG ===\r\n\r\n");

                double concreteStrength = (double)numConcreteStrength.Value;

                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText($"Creating sections with C{concreteStrength} concrete...\r\n");

                // Create material first
                string requestedMaterialName = $"CONC-C{concreteStrength}";
                string actualMaterialName = requestedMaterialName;

                // Pass by ref to get the actual name back
                if (!_SectionManager.CreateCustomConcreteWithName(ref actualMaterialName, concreteStrength))
                {
                    txtStatus.AppendText("ERROR: Failed to create concrete material.\r\n");
                    Cursor = Cursors.Default;
                    return;
                }

                txtStatus.AppendText($"Concrete material created successfully as '{actualMaterialName}'.\r\n");

                txtStatus.AppendText("Concrete material created successfully.\r\n");

                // Create sections based on user selection
                bool allSuccess = true;

                if (chkColumns.Checked)
                {
                    txtStatus.AppendText("About to call CreateStandardColumns... \r\n");
                    bool success = _SectionManager.CreateStandardColumns(actualMaterialName);
                    txtStatus.AppendText($"CreateStandardColumns returned: {success}\r\n");
                    txtStatus.AppendText(success ? "Columns created.\r\n" : "ERROR: Column creation failed.\r\n");
                    allSuccess &= success;
                }

                if (chkBeams.Checked)
                {
                    bool success = _SectionManager.CreateStandardBeams(actualMaterialName);
                    txtStatus.AppendText(success ? "Beams created.\r\n" : "ERROR: Beam creation failed.\r\n");
                    allSuccess &= success;
                }

                if (chkSlabs.Checked)
                {
                    bool success = _SectionManager.CreateStandardSlabs(actualMaterialName);
                    txtStatus.AppendText(success ? "Slabs created.\r\n" : "ERROR: Slab creation failed.\r\n");
                    allSuccess &= success;
                }

                if (chkWalls.Checked)
                {
                    bool success = _SectionManager.CreateStandardWalls(actualMaterialName);
                    txtStatus.AppendText(success ? "Walls created.\r\n" : "ERROR: Wall creation failed.\r\n");
                    allSuccess &= success;
                }

                Cursor = Cursors.Default;

                if (allSuccess)
                {
                    txtStatus.AppendText("All selected sections created successfully!\r\n\r\n");
                    MessageBox.Show("Sections created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText("Some sections failed to create. Check ETABS for details.\r\n\r\n");
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                txtStatus.AppendText($"ERROR: {ex.Message}\r\n\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCreateLoads_Click(object sender, EventArgs e)
        {
            try
            {
                txtStatus.AppendText("=== LOAD CREATION DEBUG ===\r\n");

                // Test each object
                if (_SapModel == null)
                {
                    txtStatus.AppendText("ERROR: _SapModel is null\r\n");
                    return;
                }
                txtStatus.AppendText("✓ _SapModel is valid\r\n");

                if (_LoadManager == null)
                {
                    txtStatus.AppendText("ERROR: _LoadManager is null\r\n");
                    return;
                }
                txtStatus.AppendText("✓ _LoadManager is valid\r\n");

                txtStatus.AppendText("About to call CreateAllStandardLoads...\r\n");

                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("Creating load patterns and combinations...\r\n");

                bool success = _LoadManager.CreateAllStandardLoads();

                Cursor = Cursors.Default;

                if (success)
                {
                    txtStatus.AppendText("All load cases and combinations created successfully!\r\n\r\n");
                }
                else
                {
                    txtStatus.AppendText("ERROR: Load creation failed.\r\n\r\n");
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                txtStatus.AppendText($"EXCEPTION: {ex.Message}\r\n");
                txtStatus.AppendText($"STACK TRACE: {ex.StackTrace}\r\n");
                MessageBox.Show($"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnApplyBCs_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (_BoundaryManager.ApplyFixedSupportsAtBase(tolMm: 5.0, out var report))
            {
                txtStatus.AppendText(report + "\r\n");
                MessageBox.Show("Fixed supports applied at base.", "Boundary Conditions");
            }
            else
            {
                txtStatus.AppendText(report + "\r\n");
                MessageBox.Show("Boundary condition application had issues. See Status.", "Boundary Conditions");
            }
            Cursor = Cursors.Default;
        }

        private void btnCreateDiaphragms_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (_DiaphragmManager.CreateAndAssignDiaphragms(out var report, 50.0))
                {
                    txtStatus.AppendText(report + Environment.NewLine);
                    MessageBox.Show("Diaphragms created and assigned successfully.", "Diaphragms");
                }
                else
                {
                    txtStatus.AppendText(report + Environment.NewLine);
                    MessageBox.Show("Diaphragm creation had issues. See Status.", "Diaphragms");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Diaphragms");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnApplyMesh_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("Applying 1m (1000mm) mesh to shell elements...\r\n");

                if (_MeshManager.ApplyStandardMesh(1000.0, out var report))
                {
                    txtStatus.AppendText(report + "\r\n");
                    MessageBox.Show("Mesh applied successfully!", "Meshing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText(report + "\r\n");
                    MessageBox.Show("Meshing had issues. See Status.", "Meshing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"ERROR: {ex.Message}\r\n");
                MessageBox.Show(ex.ToString(), "Meshing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnAssignSections_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("Assigning sections intelligently to all elements...\r\n");

                // Assign frame sections
                if (_SectionAssignmentManager.AssignFrameSectionsIntelligently(out var frameReport))
                {
                    txtStatus.AppendText(frameReport + "\r\n");
                }
                else
                {
                    txtStatus.AppendText(frameReport + "\r\n");
                }

                // Assign area properties
                if (_SectionAssignmentManager.AssignAreaPropertiesIntelligently(out var areaReport))
                {
                    txtStatus.AppendText(areaReport + "\r\n");
                }
                else
                {
                    txtStatus.AppendText(areaReport + "\r\n");
                }

                // Show summary
                if (_SectionAssignmentManager.GetAssignmentSummary(out var summary))
                {
                    txtStatus.AppendText("\n" + summary + "\r\n");
                }

                MessageBox.Show("Section assignment complete! Check Status for details.",
                    "Section Assignment", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"ERROR: {ex.Message}\r\n");
                MessageBox.Show(ex.ToString(), "Section Assignment Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnAssignLoads_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("Assigning loads to slabs...\r\n");

                // Apply 5 kPa (0.005 N/mm²) live load to all slabs
                double liveLoadValue = 0.005; // 5 kPa = 0.005 N/mm²

                if (_LoadAssignmentManager.ApplyLoadToAllSlabs("PLUGIN_LIVE", liveLoadValue, out var report))
                {
                    txtStatus.AppendText(report + "\r\n");
                    MessageBox.Show("Live loads applied to all slabs successfully!",
                        "Load Assignment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText(report + "\r\n");
                    MessageBox.Show("Load assignment had issues. See Status.",
                        "Load Assignment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"ERROR: {ex.Message}\r\n");
                MessageBox.Show(ex.ToString(), "Load Assignment Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnSetupMass_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("Setting up seismic mass source...\r\n");

                if (_MassSourceManager.SetupSeismicMassSource(out var report))
                {
                    txtStatus.AppendText(report + "\r\n");
                    MessageBox.Show("Mass source configured successfully!",
                        "Mass Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText(report + "\r\n");
                    MessageBox.Show("Mass source configuration had issues. See Status.",
                        "Mass Source", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"ERROR: {ex.Message}\r\n");
                MessageBox.Show(ex.ToString(), "Mass Source Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnRunAnalysis_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("=== RUNNING ANALYSIS ===\r\n");

                // Run the analysis
                if (_AnalysisManager.RunAnalysis(out var report))
                {
                    txtStatus.AppendText(report + "\r\n");

                    // Try to get base reactions for first load case
                    txtStatus.AppendText("\nGetting base reactions...\r\n");
                    if (_AnalysisManager.GetBaseReactions("PLUGIN_DEAD", out var reactionReport))
                    {
                        txtStatus.AppendText(reactionReport + "\r\n");
                    }

                    MessageBox.Show("Analysis completed successfully!\n\nCheck Status window for results summary.",
                        "Analysis Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText(report + "\r\n");
                    MessageBox.Show("Analysis failed. See Status window for details.\n\nCheck ETABS messages for specific errors.",
                        "Analysis Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"ERROR: {ex.Message}\r\n");
                MessageBox.Show(ex.ToString(), "Analysis Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // Placeholder for the complete workflow
        private void btnRunWorkflow_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.Clear();
                txtStatus.AppendText("╔════════════════════════════════════════════╗\r\n");
                txtStatus.AppendText("║   ETABS COMPLETE WORKFLOW AUTOMATION       ║\r\n");
                txtStatus.AppendText("╚════════════════════════════════════════════╝\r\n\r\n");

                // Get user inputs
                double concreteStrength = (double)numConcreteStrength.Value;
                bool createColumns = chkColumns.Checked;
                bool createBeams = chkBeams.Checked;
                bool createSlabs = chkSlabs.Checked;
                bool createWalls = chkWalls.Checked;

                int stepNumber = 1;
                bool overallSuccess = true;

                // ============================================
                // STEP 1: CREATE MATERIALS
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] CREATING MATERIALS\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");

                string materialName = $"CONC-C{concreteStrength}";
                string actualMaterialName = materialName;

                if (!_SectionManager.CreateCustomConcreteWithName(ref actualMaterialName, concreteStrength))
                {
                    txtStatus.AppendText("✗ ERROR: Failed to create concrete material.\r\n\r\n");
                    MessageBox.Show("Workflow stopped: Material creation failed.", "Workflow Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if material already existed or was created
                txtStatus.AppendText($"✓ Concrete material ready: {actualMaterialName}\r\n");
                txtStatus.AppendText($"  - Strength: {concreteStrength} MPa\r\n");
                txtStatus.AppendText($"  - Density: 2400 kg/m³\r\n\r\n");

                // ============================================
                // STEP 2: CREATE SECTION DEFINITIONS
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] CREATING SECTION DEFINITIONS\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");

                int sectionsCreated = 0;

                if (createColumns)
                {
                    if (_SectionManager.CreateStandardColumns(actualMaterialName))
                    {
                        txtStatus.AppendText("✓ Column sections created (6 sizes)\r\n");
                        sectionsCreated++;
                    }
                    else
                    {
                        txtStatus.AppendText("✗ Column section creation failed\r\n");
                        overallSuccess = false;
                    }
                }

                if (createBeams)
                {
                    if (_SectionManager.CreateStandardBeams(actualMaterialName))
                    {
                        txtStatus.AppendText("✓ Beam sections created (6 sizes)\r\n");
                        sectionsCreated++;
                    }
                    else
                    {
                        txtStatus.AppendText("✗ Beam section creation failed\r\n");
                        overallSuccess = false;
                    }
                }

                if (createSlabs)
                {
                    if (_SectionManager.CreateStandardSlabs(actualMaterialName))
                    {
                        txtStatus.AppendText("✓ Slab sections created (6 thicknesses)\r\n");
                        sectionsCreated++;
                    }
                    else
                    {
                        txtStatus.AppendText("✗ Slab section creation failed\r\n");
                        overallSuccess = false;
                    }
                }

                if (createWalls)
                {
                    if (_SectionManager.CreateStandardWalls(actualMaterialName))
                    {
                        txtStatus.AppendText("✓ Wall sections created (8 thicknesses)\r\n");
                        sectionsCreated++;
                    }
                    else
                    {
                        txtStatus.AppendText("✗ Wall section creation failed\r\n");
                        overallSuccess = false;
                    }
                }

                txtStatus.AppendText($"\nTotal section types created: {sectionsCreated}\r\n\r\n");

                if (sectionsCreated == 0)
                {
                    txtStatus.AppendText("⚠ WARNING: No sections were created. Check your selections.\r\n\r\n");
                }

                // ============================================
                // STEP 3: ASSIGN SECTIONS TO ELEMENTS
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] ASSIGNING SECTIONS TO ELEMENTS\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");

                // Assign frame sections (columns and beams)
                if (_SectionAssignmentManager.AssignFrameSectionsIntelligently(out var frameReport))
                {
                    txtStatus.AppendText(frameReport);
                }
                else
                {
                    txtStatus.AppendText(frameReport);
                    txtStatus.AppendText("⚠ Frame section assignment had issues\r\n");
                    overallSuccess = false;
                }

                // Assign area properties (slabs and walls)
                if (_SectionAssignmentManager.AssignAreaPropertiesIntelligently(out var areaReport))
                {
                    txtStatus.AppendText(areaReport);
                }
                else
                {
                    txtStatus.AppendText(areaReport);
                    txtStatus.AppendText("⚠ Area property assignment had issues\r\n");
                    overallSuccess = false;
                }

                // Show assignment summary
                if (_SectionAssignmentManager.GetAssignmentSummary(out var assignmentSummary))
                {
                    txtStatus.AppendText("\n" + assignmentSummary + "\r\n\r\n");
                }

                // ============================================
                // STEP 4: CREATE LOAD PATTERNS
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] CREATING LOAD PATTERNS\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");

                if (_LoadManager.CreateStandardLoadPatterns())
                {
                    txtStatus.AppendText("✓ Load patterns created:\r\n");
                    txtStatus.AppendText("  • PLUGIN_DEAD (Self-weight = 1.0)\r\n");
                    txtStatus.AppendText("  • PLUGIN_LIVE\r\n");
                    txtStatus.AppendText("  • PLUGIN_WIND\r\n");
                    txtStatus.AppendText("  • PLUGIN_SEISMIC\r\n\r\n");
                }
                else
                {
                    txtStatus.AppendText("✗ Load pattern creation failed\r\n\r\n");
                    overallSuccess = false;
                }

                // ============================================
                // STEP 5: CREATE LOAD COMBINATIONS
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] CREATING LOAD COMBINATIONS\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");

                bool combosSuccess = true;

                // Ultimate limit state combinations
                if (_LoadManager.CreateUltimateLoadCombinations())
                {
                    txtStatus.AppendText("✓ Ultimate limit state combinations created (5)\r\n");
                }
                else
                {
                    txtStatus.AppendText("✗ ULS combination creation failed\r\n");
                    combosSuccess = false;
                    overallSuccess = false;
                }

                // Serviceability combinations
                if (_LoadManager.CreateServiceabilityLoadCombinations())
                {
                    txtStatus.AppendText("✓ Serviceability combinations created (5)\r\n");
                }
                else
                {
                    txtStatus.AppendText("✗ SLS combination creation failed\r\n");
                    combosSuccess = false;
                    overallSuccess = false;
                }

                // Envelope combinations
                if (_LoadManager.CreateEnvelopeCombinations())
                {
                    txtStatus.AppendText("✓ Envelope combinations created (2)\r\n");
                }
                else
                {
                    txtStatus.AppendText("✗ Envelope combination creation failed\r\n");
                    combosSuccess = false;
                    overallSuccess = false;
                }

                if (combosSuccess)
                {
                    txtStatus.AppendText("\nTotal combinations created: 12\r\n\r\n");
                }
                else
                {
                    txtStatus.AppendText("\n⚠ Some combinations failed to create\r\n\r\n");
                }

                // ============================================
                // STEP 6: ASSIGN LOADS TO ELEMENTS
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] ASSIGNING LOADS TO ELEMENTS\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");

                // Apply live load to slabs (5 kPa = 0.005 N/mm²)
                double liveLoadValue = 0.005;
                if (_LoadAssignmentManager.ApplyLoadToAllSlabs("PLUGIN_LIVE", liveLoadValue, out var loadReport))
                {
                    txtStatus.AppendText(loadReport);
                }
                else
                {
                    txtStatus.AppendText(loadReport);
                    txtStatus.AppendText("⚠ Load assignment had issues\r\n");
                    overallSuccess = false;
                }
                txtStatus.AppendText("\r\n");

                // ============================================
                // STEP 7: APPLY BOUNDARY CONDITIONS
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] APPLYING BOUNDARY CONDITIONS\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");

                if (_BoundaryManager.ApplyFixedSupportsAtBase(5.0, out var bcReport))
                {
                    txtStatus.AppendText(bcReport);
                }
                else
                {
                    txtStatus.AppendText(bcReport);
                    txtStatus.AppendText("⚠ Boundary condition application had issues\r\n");
                    overallSuccess = false;
                }
                txtStatus.AppendText("\r\n");

                // ============================================
                // STEP 8: CREATE AND ASSIGN DIAPHRAGMS
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] CREATING DIAPHRAGMS\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");

                if (_DiaphragmManager.CreateAndAssignDiaphragms(out var diaphReport, 50.0))
                {
                    txtStatus.AppendText(diaphReport);
                }
                else
                {
                    txtStatus.AppendText(diaphReport);
                    txtStatus.AppendText("⚠ Diaphragm creation had issues\r\n");
                    overallSuccess = false;
                }
                txtStatus.AppendText("\r\n");

                // ============================================
                // STEP 9: APPLY MESH TO SHELL ELEMENTS
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] APPLYING MESH STRATEGY\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");

                if (_MeshManager.ApplyStandardMesh(1000.0, out var meshReport))
                {
                    txtStatus.AppendText(meshReport);
                }
                else
                {
                    txtStatus.AppendText(meshReport);
                    txtStatus.AppendText("⚠ Meshing had issues\r\n");
                    overallSuccess = false;
                }
                txtStatus.AppendText("\r\n");

                // ============================================
                // STEP 10: SETUP MASS SOURCE
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] CONFIGURING MASS SOURCE\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");

                if (_MassSourceManager.SetupSeismicMassSource(out var massReport))
                {
                    txtStatus.AppendText(massReport);
                }
                else
                {
                    txtStatus.AppendText(massReport);
                    txtStatus.AppendText("⚠ Mass source configuration had issues\r\n");
                    overallSuccess = false;
                }
                txtStatus.AppendText("\r\n");

                // ============================================
                // STEP 11: RUN STRUCTURAL ANALYSIS
                // ============================================
                txtStatus.AppendText($"[{stepNumber++}] RUNNING STRUCTURAL ANALYSIS\r\n");
                txtStatus.AppendText("─────────────────────────────────────────\r\n");
                txtStatus.AppendText("This may take several minutes...\r\n\r\n");

                if (_AnalysisManager.RunAnalysis(out var analysisReport))
                {
                    txtStatus.AppendText(analysisReport);

                    // Get base reactions for verification
                    txtStatus.AppendText("\r\n");
                    if (_AnalysisManager.GetBaseReactions("PLUGIN_DEAD", out var reactionReport))
                    {
                        txtStatus.AppendText(reactionReport);
                    }
                }
                else
                {
                    txtStatus.AppendText(analysisReport);
                    txtStatus.AppendText("\r\n⚠ Analysis failed - model may have stability issues\r\n");
                    overallSuccess = false;
                }

                // ============================================
                // WORKFLOW SUMMARY
                // ============================================
                txtStatus.AppendText("\r\n");
                txtStatus.AppendText("╔════════════════════════════════════════════╗\r\n");
                if (overallSuccess)
                {
                    txtStatus.AppendText("║      ✓ WORKFLOW COMPLETED SUCCESSFULLY     ║\r\n");
                }
                else
                {
                    txtStatus.AppendText("║    ⚠ WORKFLOW COMPLETED WITH WARNINGS     ║\r\n");
                }
                txtStatus.AppendText("╚════════════════════════════════════════════╝\r\n\r\n");

                txtStatus.AppendText($"Total steps executed: {stepNumber - 1}\r\n");
                txtStatus.AppendText($"Material: {actualMaterialName}\r\n");
                txtStatus.AppendText($"Section types: {sectionsCreated}\r\n");
                txtStatus.AppendText($"Load patterns: 4\r\n");
                txtStatus.AppendText($"Load combinations: 12\r\n");
                txtStatus.AppendText($"Analysis: {(overallSuccess ? "Complete" : "Failed or Incomplete")}\r\n\r\n");

                // Show completion message
                if (overallSuccess)
                {
                    MessageBox.Show(
                        "Complete workflow executed successfully!\n\n" +
                        "✓ Materials created\n" +
                        "✓ Sections defined and assigned\n" +
                        "✓ Loads created and assigned\n" +
                        "✓ Boundary conditions applied\n" +
                        "✓ Diaphragms created\n" +
                        "✓ Mesh applied\n" +
                        "✓ Mass source configured\n" +
                        "✓ Analysis completed\n\n" +
                        "Check the Status window for detailed results.",
                        "Workflow Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Workflow completed with warnings.\n\n" +
                        "Some steps encountered issues.\n" +
                        "Check the Status window for details.\n\n" +
                        "The model may still be usable, but verify all assignments.",
                        "Workflow Complete with Warnings",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText("\r\n");
                txtStatus.AppendText("╔════════════════════════════════════════════╗\r\n");
                txtStatus.AppendText("║           ✗ WORKFLOW FAILED                ║\r\n");
                txtStatus.AppendText("╚════════════════════════════════════════════╝\r\n\r\n");
                txtStatus.AppendText($"EXCEPTION: {ex.Message}\r\n");
                txtStatus.AppendText($"\nStack Trace:\r\n{ex.StackTrace}\r\n");

                MessageBox.Show(
                    $"Workflow failed with an error:\n\n{ex.Message}\n\n" +
                    "Check the Status window for details.",
                    "Workflow Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnPlaceWalls_Click(object sender, EventArgs e)
        {
            try
            {
                WallPlacementForm wallForm = new WallPlacementForm(_SapModel);
                wallForm.ShowDialog();
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"ERROR opening wall placement window: {ex.Message}\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPlaceColumns_Click(object sender, EventArgs e)
        {
            try
            {
                ColumnPlacementForm columnForm = new ColumnPlacementForm(_SapModel);
                columnForm.ShowDialog();
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"ERROR opening column placement window: {ex.Message}\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExtractData_Click(object sender, EventArgs e)
        {
            try
            {
                DataExtractionForm extractionForm = new DataExtractionForm(_SapModel);
                extractionForm.ShowDialog();
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"ERROR opening data extraction window: {ex.Message}\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDesignCheck_Click(object sender, EventArgs e)
        {
            try
            {
                DesignCheckForm designCheckForm = new DesignCheckForm(_SapModel);
                designCheckForm.ShowDialog();
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"ERROR opening design check window: {ex.Message}\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // must include a call to finish()
            _Plugin.Finish(0);
        }
    }
}
