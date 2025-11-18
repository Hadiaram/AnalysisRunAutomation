using System;
using System.Windows.Forms;
using ETABSv1;

namespace ETABS_Plugin
{
    public partial class DataExtractionForm : Form
    {
        private cSapModel _SapModel;
        private DataExtractionManager _ExtractionManager;

        public DataExtractionForm(cSapModel sapModel)
        {
            _SapModel = sapModel;
            _ExtractionManager = new DataExtractionManager(_SapModel);
            InitializeComponent();
        }

        private void DataExtractionForm_Load(object sender, EventArgs e)
        {
            txtStatus.AppendText("Data Extraction Window Ready\r\n");
            txtStatus.AppendText("Click extraction buttons to extract and save data\r\n\r\n");
        }

        private void btnExtractBaseReactions_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("=== EXTRACTING BASE REACTIONS ===\r\n");

                // Extract data
                if (_ExtractionManager.ExtractBaseReactions(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);

                    // Ask user where to save
                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"BaseReactions_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Base Reactions Data";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Base reactions extracted successfully!\n\nSaved to:\n{saveDialog.FileName}",
                                    "Extraction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show("Failed to extract base reactions. Check status for details.", "Extraction Error",
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

        private void btnExtractProjectInfo_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("=== EXTRACTING PROJECT/MODEL INFORMATION ===\r\n");

                // Extract data
                if (_ExtractionManager.ExtractProjectInfo(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);

                    // Ask user where to save
                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"ProjectInfo_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Project Information";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Project information extracted successfully!\n\nSaved to:\n{saveDialog.FileName}",
                                    "Extraction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show("Failed to extract project information. Check status for details.", "Extraction Error",
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

        private void btnExtractStoryInfo_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("=== EXTRACTING STORY/LEVEL INFORMATION ===\r\n");

                // Extract data
                if (_ExtractionManager.ExtractStoryInfo(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);

                    // Ask user where to save
                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"StoryInfo_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Story Information";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Story information extracted successfully!\n\nSaved to:\n{saveDialog.FileName}",
                                    "Extraction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show("Failed to extract story information. Check status for details.", "Extraction Error",
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

        // Placeholder for future extraction methods
        // Add more extraction button handlers here as needed

        private void btnClearStatus_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();
            txtStatus.AppendText("Status cleared\r\n\r\n");
        }
    }
}
