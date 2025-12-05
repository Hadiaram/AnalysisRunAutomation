using System;
using System.Windows.Forms;
using ETABSv1;

namespace ETABS_Plugin
{
    public partial class DesignCheckForm : Form
    {
        private cSapModel _SapModel;
        private DesignCheckManager _DesignCheckManager;

        public DesignCheckForm(cSapModel sapModel)
        {
            _SapModel = sapModel;
            _DesignCheckManager = new DesignCheckManager(_SapModel);
            InitializeComponent();
        }

        private void DesignCheckForm_Load(object sender, EventArgs e)
        {
            txtStatus.AppendText("Design Check Window Ready\r\n");
            txtStatus.AppendText("=========================\r\n\r\n");
            txtStatus.AppendText("This window allows you to:\r\n");
            txtStatus.AppendText("  1. Start design checks for different element types\r\n");
            txtStatus.AppendText("  2. Extract design check results to CSV files\r\n\r\n");
            txtStatus.AppendText("NOTE: Design checks require completed analysis results.\r\n");
            txtStatus.AppendText("If no results exist, analysis will run automatically.\r\n\r\n");
        }

        #region Steel Frame Design

        private void btnRunSteelDesign_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("╔════════════════════════════════════════════╗\r\n");
                txtStatus.AppendText("║     STEEL FRAME DESIGN CHECK               ║\r\n");
                txtStatus.AppendText("╚════════════════════════════════════════════╝\r\n\r\n");

                if (_DesignCheckManager.StartSteelDesign(out string report))
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    MessageBox.Show("Steel frame design check completed successfully!\n\n" +
                                    "Click 'Extract Steel Results' to save results to CSV.",
                                    "Steel Design Complete",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    MessageBox.Show("Steel design check failed. Check status window for details.",
                                    "Steel Design Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nEXCEPTION: {ex.Message}\r\n\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnExtractSteelResults_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting steel design results...\r\n");

                if (_DesignCheckManager.ExtractSteelDesignResults(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    // Save dialog
                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"SteelDesign_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Steel Design Results";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_DesignCheckManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Steel design results saved successfully!\n\nSaved to:\n{saveDialog.FileName}",
                                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                txtStatus.AppendText($"✗ Error saving file: {error}\r\n\r\n");
                                MessageBox.Show($"Error saving file:\n{error}", "Save Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            txtStatus.AppendText("Save cancelled by user\r\n\r\n");
                        }
                    }
                }
                else
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");
                    MessageBox.Show("Failed to extract steel design results. Check status for details.",
                        "Extraction Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nEXCEPTION: {ex.Message}\r\n\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Concrete Frame Design

        private void btnRunConcreteDesign_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("╔════════════════════════════════════════════╗\r\n");
                txtStatus.AppendText("║     CONCRETE FRAME DESIGN CHECK            ║\r\n");
                txtStatus.AppendText("╚════════════════════════════════════════════╝\r\n\r\n");

                if (_DesignCheckManager.StartConcreteDesign(out string report))
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    MessageBox.Show("Concrete frame design check completed successfully!\n\n" +
                                    "Use the extract buttons to save column/beam results to CSV.",
                                    "Concrete Design Complete",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    MessageBox.Show("Concrete design check failed. Check status window for details.",
                                    "Concrete Design Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nEXCEPTION: {ex.Message}\r\n\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnExtractConcreteColumns_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting concrete column design results...\r\n");

                if (_DesignCheckManager.ExtractConcreteColumnResults(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"ConcreteColumns_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Concrete Column Design Results";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_DesignCheckManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Concrete column design results saved!\n\nSaved to:\n{saveDialog.FileName}",
                                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                txtStatus.AppendText($"✗ Error saving file: {error}\r\n\r\n");
                            }
                        }
                        else
                        {
                            txtStatus.AppendText("Save cancelled by user\r\n\r\n");
                        }
                    }
                }
                else
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nEXCEPTION: {ex.Message}\r\n\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnExtractConcreteBeams_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting concrete beam design results...\r\n");

                if (_DesignCheckManager.ExtractConcreteBeamResults(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"ConcreteBeams_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Concrete Beam Design Results";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_DesignCheckManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Concrete beam design results saved!\n\nSaved to:\n{saveDialog.FileName}",
                                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                txtStatus.AppendText($"✗ Error saving file: {error}\r\n\r\n");
                            }
                        }
                        else
                        {
                            txtStatus.AppendText("Save cancelled by user\r\n\r\n");
                        }
                    }
                }
                else
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nEXCEPTION: {ex.Message}\r\n\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Composite Beam Design

        private void btnRunCompositeDesign_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("╔════════════════════════════════════════════╗\r\n");
                txtStatus.AppendText("║     COMPOSITE BEAM DESIGN CHECK            ║\r\n");
                txtStatus.AppendText("╚════════════════════════════════════════════╝\r\n\r\n");

                if (_DesignCheckManager.StartCompositeBeamDesign(out string report))
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    MessageBox.Show("Composite beam design check completed successfully!\n\n" +
                                    "Click 'Extract Composite Beam Results' to save results to CSV.",
                                    "Composite Beam Design Complete",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    MessageBox.Show("Composite beam design check failed. Check status window for details.",
                                    "Composite Beam Design Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nEXCEPTION: {ex.Message}\r\n\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnExtractCompositeBeams_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting composite beam design results...\r\n");

                if (_DesignCheckManager.ExtractCompositeBeamResults(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"CompositeBeams_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Composite Beam Design Results";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_DesignCheckManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Composite beam design results saved!\n\nSaved to:\n{saveDialog.FileName}",
                                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                txtStatus.AppendText($"✗ Error saving file: {error}\r\n\r\n");
                            }
                        }
                        else
                        {
                            txtStatus.AppendText("Save cancelled by user\r\n\r\n");
                        }
                    }
                }
                else
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nEXCEPTION: {ex.Message}\r\n\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Slab Design

        private void btnRunSlabDesign_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("╔════════════════════════════════════════════╗\r\n");
                txtStatus.AppendText("║     SLAB DESIGN CHECK                      ║\r\n");
                txtStatus.AppendText("╚════════════════════════════════════════════╝\r\n\r\n");

                if (_DesignCheckManager.StartSlabDesign(out string report))
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    MessageBox.Show("Slab design check completed successfully!\n\n" +
                                    "Click 'Extract Slab Results' to save results to CSV.",
                                    "Slab Design Complete",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    MessageBox.Show("Slab design check failed. Check status window for details.",
                                    "Slab Design Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nEXCEPTION: {ex.Message}\r\n\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnExtractSlabResults_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting slab design results...\r\n");

                if (_DesignCheckManager.ExtractSlabDesignResults(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"SlabDesign_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Slab Design Results";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_DesignCheckManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Slab design results saved!\n\nSaved to:\n{saveDialog.FileName}",
                                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                txtStatus.AppendText($"✗ Error saving file: {error}\r\n\r\n");
                            }
                        }
                        else
                        {
                            txtStatus.AppendText("Save cancelled by user\r\n\r\n");
                        }
                    }
                }
                else
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"\r\nEXCEPTION: {ex.Message}\r\n\r\n");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        private void btnClearStatus_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();
            txtStatus.AppendText("Status cleared\r\n\r\n");
        }
    }
}
