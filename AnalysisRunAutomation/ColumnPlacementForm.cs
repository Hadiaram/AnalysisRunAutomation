using System;
using System.Windows.Forms;
using ETABSv1;

namespace ETABS_Plugin
{
    public partial class ColumnPlacementForm : Form
    {
        private cSapModel _SapModel;
        private ColumnPlacementManager _ColumnManager;

        public ColumnPlacementForm(cSapModel sapModel)
        {
            _SapModel = sapModel;
            _ColumnManager = new ColumnPlacementManager(_SapModel);
            InitializeComponent();
        }

        private void ColumnPlacementForm_Load(object sender, EventArgs e)
        {
            // Load available frame sections
            if (_ColumnManager.GetFrameSections(out string[] sections, out string error))
            {
                cmbSection.Items.Clear();
                foreach (string section in sections)
                {
                    cmbSection.Items.Add(section);
                }

                if (cmbSection.Items.Count > 0)
                {
                    // Try to find a column section (COL-*)
                    int colIndex = cmbSection.FindString("COL-");
                    if (colIndex >= 0)
                        cmbSection.SelectedIndex = colIndex;
                    else
                        cmbSection.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No frame sections found in the model.\nPlease create frame sections first.",
                        "No Sections", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show($"Error loading frame sections: {error}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPlaceColumn_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate section selection
                if (cmbSection.SelectedItem == null)
                {
                    MessageBox.Show("Please select a frame section.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate column label
                if (string.IsNullOrWhiteSpace(txtColumnLabel.Text))
                {
                    MessageBox.Show("Please enter a column label.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor = Cursors.WaitCursor;
                txtStatus.Clear();
                txtStatus.AppendText("Placing column...\r\n\r\n");

                // Get values from controls
                double x = (double)numX.Value;
                double y = (double)numY.Value;
                double baseElevation = (double)numBaseElevation.Value;
                double height = (double)numHeight.Value;
                string section = cmbSection.SelectedItem.ToString();
                double rotationAngle = (double)numRotationAngle.Value;
                string columnLabel = txtColumnLabel.Text.Trim();

                // Place the column
                if (_ColumnManager.PlaceColumn(
                    x, y, baseElevation, height,
                    section, rotationAngle, columnLabel,
                    out string report))
                {
                    txtStatus.AppendText(report);
                    MessageBox.Show("Column placed successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText(report);
                    MessageBox.Show("Failed to place column. Check status for details.", "Error",
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
            // Load example values for a simple column
            numX.Value = 0;
            numY.Value = 0;
            numBaseElevation.Value = 0;
            numHeight.Value = 3;
            numRotationAngle.Value = 0;
            txtColumnLabel.Text = "C1";

            MessageBox.Show("Example values loaded!\n\nThis will create a column at origin (0,0) with 3m height.",
                "Example Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnGenerateTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                using var saveFileDialog = new SaveFileDialog
                {
                    Title = "Save CSV template",
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = "columns_template.csv"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_ColumnManager.GenerateCsvTemplate(saveFileDialog.FileName, out string report))
                    {
                        txtStatus.Clear();
                        txtStatus.AppendText(report);

                        var result = MessageBox.Show(
                            $"CSV template created successfully!\n\n{saveFileDialog.FileName}\n\n" +
                            "Would you like to open the template file now?",
                            "Template Created",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveFileDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                    else
                    {
                        txtStatus.Clear();
                        txtStatus.AppendText(report);
                        MessageBox.Show($"Failed to create template:\n\n{report}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nERROR: {ex.Message}\r\n");
                MessageBox.Show($"Error creating template: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                using var openFileDialog = new OpenFileDialog
                {
                    Title = "Select CSV file with column data",
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    CheckFileExists = true
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtStatus.Clear();
                    txtStatus.AppendText($"Importing columns from: {openFileDialog.FileName}\r\n\r\n");

                    Cursor = Cursors.WaitCursor;

                    if (_ColumnManager.ImportColumnsFromCsv(openFileDialog.FileName, out string report))
                    {
                        txtStatus.AppendText(report);
                        MessageBox.Show($"CSV import completed successfully!\n\nCheck the status window for details.",
                            "CSV Import Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        txtStatus.AppendText(report);
                        MessageBox.Show($"CSV import had errors. Check status window for details.\n\nCommon issues:\n" +
                            "- Missing required columns\n" +
                            "- Invalid coordinate values\n" +
                            "- Section doesn't exist in model\n\n" +
                            "See status window for specific errors.",
                            "CSV Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nERROR: {ex.Message}\r\n");
                MessageBox.Show($"Error importing CSV: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
