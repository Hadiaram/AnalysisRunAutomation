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
            txtStatus.AppendText("TIP: Click 'Run Model Diagnostics' to see what data is available\r\n\r\n");
        }

        private void btnRunDiagnostics_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Running model diagnostics...\r\n\r\n");

                if (_ExtractionManager.RunModelDiagnostics(out string report))
                {
                    txtStatus.AppendText(report);
                    txtStatus.AppendText("\r\n");
                }
                else
                {
                    txtStatus.AppendText($"FAILED: {report}\r\n\r\n");
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

        private void btnExtractAll_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                // Ask user to select output folder
                using (var folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Select folder to save all extracted data";
                    folderDialog.ShowNewFolderButton = true;

                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        txtStatus.AppendText($"Extracting all data to: {folderDialog.SelectedPath}\r\n\r\n");

                        // Show progress bar
                        ShowProgress(14);

                        if (_ExtractionManager.ExtractAllData(folderDialog.SelectedPath, out string report,
                            (current, total, message) => UpdateProgress(current, total, message)))
                        {
                            txtStatus.AppendText(report);
                            txtStatus.AppendText("\r\n");

                            MessageBox.Show($"Extraction complete!\n\nFiles saved to:\n{folderDialog.SelectedPath}",
                                "Extract All Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            txtStatus.AppendText($"FAILED: {report}\r\n\r\n");
                            MessageBox.Show("Extraction completed with errors. Check status window for details.",
                                "Extract All - Some Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        txtStatus.AppendText("Extract All cancelled by user\r\n\r\n");
                    }
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
                HideProgress();
                Cursor = Cursors.Default;
            }
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

        private void btnExtractGridInfo_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("=== EXTRACTING GRID SYSTEM INFORMATION ===\r\n");

                // Extract data
                if (_ExtractionManager.ExtractGridInfo(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);

                    // Ask user where to save
                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"GridInfo_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Grid System Information";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Grid system information extracted successfully!\n\nSaved to:\n{saveDialog.FileName}",
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
                    MessageBox.Show("Failed to extract grid system information. Check status for details.", "Extraction Error",
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

        private void btnExtractFrameModifiers_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("=== EXTRACTING FRAME PROPERTY MODIFIERS ===\r\n");

                // Extract data
                if (_ExtractionManager.ExtractFrameModifiers(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);

                    // Ask user where to save
                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"FrameModifiers_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Frame Modifiers";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Frame modifiers extracted successfully!\n\nSaved to:\n{saveDialog.FileName}",
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
                    MessageBox.Show("Failed to extract frame modifiers. Check status for details.", "Extraction Error",
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

        private void btnExtractAreaModifiers_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("=== EXTRACTING AREA PROPERTY MODIFIERS ===\r\n");

                // Extract data
                if (_ExtractionManager.ExtractAreaModifiers(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);

                    // Ask user where to save
                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"AreaModifiers_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Area Modifiers";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Area modifiers extracted successfully!\n\nSaved to:\n{saveDialog.FileName}",
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
                    MessageBox.Show("Failed to extract area modifiers. Check status for details.", "Extraction Error",
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

        private void btnExtractWallElements_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("=== EXTRACTING WALL ELEMENTS ===\r\n");

                // Show progress bar
                ShowProgress();

                // Extract data with progress callback
                if (_ExtractionManager.ExtractWallElements(out string csvData, out string report,
                    (current, total, message) => UpdateProgress(current, total, message)))
                {
                    txtStatus.AppendText(report);

                    // Ask user where to save
                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"WallElements_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Wall Elements";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Wall elements extracted successfully!\n\nSaved to:\n{saveDialog.FileName}",
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
                    MessageBox.Show("Failed to extract wall elements. Check status for details.", "Extraction Error",
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
                HideProgress();
                Cursor = Cursors.Default;
            }
        }

        private void btnExtractColumnElements_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                txtStatus.AppendText("=== EXTRACTING COLUMN ELEMENTS ===\r\n");

                // Extract data
                if (_ExtractionManager.ExtractColumnElements(out string csvData, out string report))
                {
                    txtStatus.AppendText(report);

                    // Ask user where to save
                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveDialog.DefaultExt = "csv";
                        saveDialog.FileName = $"ColumnElements_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        saveDialog.Title = "Save Column Elements";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                            {
                                txtStatus.AppendText($"✓ Data saved to: {saveDialog.FileName}\r\n\r\n");
                                MessageBox.Show($"Column elements extracted successfully!\n\nSaved to:\n{saveDialog.FileName}",
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
                    MessageBox.Show("Failed to extract column elements. Check status for details.", "Extraction Error",
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

        private void btnExtractModalPeriods_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting modal periods...\r\n");

                // Extract data
                if (!_ExtractionManager.ExtractModalPeriods(out string csvData, out string report))
                {
                    txtStatus.AppendText($"FAILED: {report}\r\n\r\n");
                    MessageBox.Show(report, "Extraction Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtStatus.AppendText($"{report}\r\n");

                // Save dialog
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    Title = "Save Modal Periods",
                    FileName = "ModalPeriods.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                    {
                        txtStatus.AppendText($"Saved to: {saveDialog.FileName}\r\n\r\n");
                        MessageBox.Show($"Data saved successfully to:\n{saveDialog.FileName}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        txtStatus.AppendText($"Save failed: {error}\r\n\r\n");
                        MessageBox.Show($"Failed to save file:\n{error}",
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    txtStatus.AppendText("Save cancelled by user\r\n\r\n");
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

        private void btnExtractModalMassRatios_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting modal participating mass ratios...\r\n");

                // Extract data
                if (!_ExtractionManager.ExtractModalMassRatios(out string csvData, out string report))
                {
                    txtStatus.AppendText($"FAILED: {report}\r\n\r\n");
                    MessageBox.Show(report, "Extraction Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtStatus.AppendText($"{report}\r\n");

                // Save dialog
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    Title = "Save Modal Mass Ratios",
                    FileName = "ModalMassRatios.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                    {
                        txtStatus.AppendText($"Saved to: {saveDialog.FileName}\r\n\r\n");
                        MessageBox.Show($"Data saved successfully to:\n{saveDialog.FileName}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        txtStatus.AppendText($"Save failed: {error}\r\n\r\n");
                        MessageBox.Show($"Failed to save file:\n{error}",
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    txtStatus.AppendText("Save cancelled by user\r\n\r\n");
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

        private void btnExtractStoryDrifts_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting story drifts...\r\n");

                // Extract data
                if (!_ExtractionManager.ExtractStoryDrifts(out string csvData, out string report))
                {
                    txtStatus.AppendText($"FAILED: {report}\r\n\r\n");
                    MessageBox.Show(report, "Extraction Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtStatus.AppendText($"{report}\r\n");

                // Save dialog
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    Title = "Save Story Drifts",
                    FileName = "StoryDrifts.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                    {
                        txtStatus.AppendText($"Saved to: {saveDialog.FileName}\r\n\r\n");
                        MessageBox.Show($"Data saved successfully to:\n{saveDialog.FileName}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        txtStatus.AppendText($"Save failed: {error}\r\n\r\n");
                        MessageBox.Show($"Failed to save file:\n{error}",
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    txtStatus.AppendText("Save cancelled by user\r\n\r\n");
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

        private void btnExtractBaseShear_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting base shear/reactions...\r\n");

                // Extract data
                if (!_ExtractionManager.ExtractBaseShear(out string csvData, out string report))
                {
                    txtStatus.AppendText($"FAILED: {report}\r\n\r\n");
                    MessageBox.Show(report, "Extraction Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtStatus.AppendText($"{report}\r\n");

                // Save dialog
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    Title = "Save Base Shear/Reactions",
                    FileName = "BaseShear.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                    {
                        txtStatus.AppendText($"Saved to: {saveDialog.FileName}\r\n\r\n");
                        MessageBox.Show($"Data saved successfully to:\n{saveDialog.FileName}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        txtStatus.AppendText($"Save failed: {error}\r\n\r\n");
                        MessageBox.Show($"Failed to save file:\n{error}",
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    txtStatus.AppendText("Save cancelled by user\r\n\r\n");
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

        private void btnExtractCompositeColumnDesign_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting composite column design results (DCR and PMM ratios)...\r\n");

                // Extract data
                if (!_ExtractionManager.ExtractCompositeColumnDesign(out string csvData, out string report))
                {
                    txtStatus.AppendText($"FAILED: {report}\r\n\r\n");
                    MessageBox.Show(report, "Extraction Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtStatus.AppendText($"{report}\r\n");

                // Save dialog
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    Title = "Save Composite Column Design Results",
                    FileName = "CompositeColumnDesign.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                    {
                        txtStatus.AppendText($"Saved to: {saveDialog.FileName}\r\n\r\n");
                        MessageBox.Show($"Data saved successfully to:\n{saveDialog.FileName}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        txtStatus.AppendText($"Save failed: {error}\r\n\r\n");
                        MessageBox.Show($"Failed to save file:\n{error}",
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    txtStatus.AppendText("Save cancelled by user\r\n\r\n");
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

        private void btnExtractQuantitiesSummary_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                txtStatus.AppendText("Extracting quantities summary...\r\n");

                // Extract data
                if (!_ExtractionManager.ExtractQuantitiesSummary(out string csvData, out string report))
                {
                    txtStatus.AppendText($"FAILED: {report}\r\n\r\n");
                    MessageBox.Show(report, "Extraction Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtStatus.AppendText($"{report}\r\n");

                // Save dialog
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    Title = "Save Quantities Summary",
                    FileName = "QuantitiesSummary.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_ExtractionManager.SaveToFile(csvData, saveDialog.FileName, out string error))
                    {
                        txtStatus.AppendText($"Saved to: {saveDialog.FileName}\r\n\r\n");
                        MessageBox.Show($"Data saved successfully to:\n{saveDialog.FileName}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        txtStatus.AppendText($"Save failed: {error}\r\n\r\n");
                        MessageBox.Show($"Failed to save file:\n{error}",
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    txtStatus.AppendText("Save cancelled by user\r\n\r\n");
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

        private void btnClearStatus_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();
            txtStatus.AppendText("Status cleared\r\n\r\n");
        }

        #region Progress Bar Helpers

        /// <summary>
        /// Shows and initializes the progress bar
        /// </summary>
        private void ShowProgress(int maximum = 100)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowProgress(maximum)));
                return;
            }

            progressBar.Minimum = 0;
            progressBar.Maximum = maximum;
            progressBar.Value = 0;
            progressBar.Visible = true;
            lblProgress.Visible = true;
            lblProgress.Text = "0%";
            progressBar.Refresh();
        }

        /// <summary>
        /// Updates progress bar value and text
        /// </summary>
        private void UpdateProgress(int current, int total, string message = "")
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(current, total, message)));
                return;
            }

            if (total > 0)
            {
                int percentage = (int)((double)current / total * 100);
                progressBar.Maximum = total;
                progressBar.Value = Math.Min(current, total);

                if (string.IsNullOrEmpty(message))
                    lblProgress.Text = $"{percentage}% ({current}/{total})";
                else
                    lblProgress.Text = $"{percentage}% - {message}";

                // Force immediate visual update without processing all messages
                progressBar.Refresh();
                lblProgress.Refresh();
            }
        }

        /// <summary>
        /// Hides the progress bar
        /// </summary>
        private void HideProgress()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(HideProgress));
                return;
            }

            progressBar.Visible = false;
            lblProgress.Visible = false;
            lblProgress.Text = "";
        }

        #endregion
    }
}
