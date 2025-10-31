using System;
using System.Windows.Forms;
using ETABSv1;

namespace ETABS_Plugin
{
    public partial class WallPlacementForm : Form
    {
        private cSapModel _SapModel;
        private WallPlacementManager _WallManager;

        public WallPlacementForm(cSapModel sapModel)
        {
            _SapModel = sapModel;
            _WallManager = new WallPlacementManager(_SapModel);
            InitializeComponent();
        }

        private void WallPlacementForm_Load(object sender, EventArgs e)
        {
            // Load available concrete materials
            if (_WallManager.GetConcreteMaterials(out string[] materials, out string error))
            {
                cmbMaterial.Items.Clear();
                foreach (string mat in materials)
                {
                    cmbMaterial.Items.Add(mat);
                }

                if (cmbMaterial.Items.Count > 0)
                {
                    // Try to find "CONC" material
                    int conIndex = cmbMaterial.FindString("CONC");
                    if (conIndex >= 0)
                        cmbMaterial.SelectedIndex = conIndex;
                    else
                        cmbMaterial.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No concrete materials found in the model.\nPlease create a concrete material first.",
                        "No Materials", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show($"Error loading materials: {error}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPlaceWall_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate material selection
                if (cmbMaterial.SelectedItem == null)
                {
                    MessageBox.Show("Please select a material.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate pier label
                if (string.IsNullOrWhiteSpace(txtPierLabel.Text))
                {
                    MessageBox.Show("Please enter a pier label.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor = Cursors.WaitCursor;
                txtStatus.Clear();
                txtStatus.AppendText("Placing wall...\r\n\r\n");

                // Get values from controls
                double x1 = (double)numX1.Value;
                double y1 = (double)numY1.Value;
                double x2 = (double)numX2.Value;
                double y2 = (double)numY2.Value;
                double x3 = (double)numX3.Value;
                double y3 = (double)numY3.Value;
                double x4 = (double)numX4.Value;
                double y4 = (double)numY4.Value;
                double elevation = (double)numElevation.Value;
                string material = cmbMaterial.SelectedItem.ToString();
                double thickness = (double)numThickness.Value / 1000.0; // Convert mm to m
                double betaAngle = (double)numBetaAngle.Value;
                string pierLabel = txtPierLabel.Text.Trim();

                // Place the wall
                if (_WallManager.PlaceWall(
                    x1, y1, x2, y2, x3, y3, x4, y4,
                    elevation, material, thickness,
                    betaAngle, pierLabel,
                    out string report))
                {
                    txtStatus.AppendText(report);
                    MessageBox.Show("Wall placed successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText(report);
                    MessageBox.Show("Failed to place wall. Check status for details.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nEXCEPTION: {ex.Message}\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnLoadExample_Click(object sender, EventArgs e)
        {
            // Load example values for a simple rectangular wall
            numX1.Value = 0;
            numY1.Value = 0;
            numX2.Value = 4;
            numY2.Value = 0;
            numX3.Value = 4;
            numY3.Value = 0.2m;
            numX4.Value = 0;
            numY4.Value = 0.2m;
            numElevation.Value = 3;
            numThickness.Value = 200;
            numBetaAngle.Value = 90;
            txtPierLabel.Text = "P1";

            MessageBox.Show("Example values loaded!\n\nThis will create a 4m x 0.2m wall at elevation 3m.",
                "Example Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
