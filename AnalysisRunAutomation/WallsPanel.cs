using System.Windows.Forms;
using Etabs.Automation.Walls.Models;
using Etabs.Automation.Walls.Services;

namespace ETABS_Plugin;

public partial class WallsPanel : UserControl
{
    private readonly IWallsService _wallsService;
    private readonly TextBox _statusTextBox;

    public WallsPanel(IWallsService wallsService, TextBox statusTextBox)
    {
        _wallsService = wallsService ?? throw new ArgumentNullException(nameof(wallsService));
        _statusTextBox = statusTextBox ?? throw new ArgumentNullException(nameof(statusTextBox));

        InitializeComponent();
        LoadStories();
    }

    private void LoadStories()
    {
        try
        {
            var stories = _wallsService.GetStoryNames();
            cmbStory.Items.Clear();

            if (stories != null && stories.Length > 0)
            {
                foreach (var story in stories)
                {
                    cmbStory.Items.Add(story);
                }

                if (cmbStory.Items.Count > 0)
                    cmbStory.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            _statusTextBox.AppendText($"Error loading stories: {ex.Message}\r\n");
        }
    }

    private void btnCreateWall_Click(object sender, EventArgs e)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtWallName.Text))
            {
                MessageBox.Show("Please enter a wall name", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbStory.SelectedItem == null)
            {
                MessageBox.Show("Please select a story", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _statusTextBox.AppendText("\n=== CREATING WALL ===\r\n");
            Cursor = Cursors.WaitCursor;

            // Get values
            string wallName = txtWallName.Text.Trim();
            string storyName = cmbStory.SelectedItem.ToString() ?? string.Empty;
            string sectionName = txtSectionName.Text.Trim();
            double thicknessMm = (double)numThickness.Value;
            string materialName = txtMaterialName.Text.Trim();

            double x0 = (double)numX0.Value;
            double y0 = (double)numY0.Value;
            double z0 = (double)numZ0.Value;
            double length = (double)numLength.Value;
            double height = (double)numHeight.Value;
            bool isAlongX = rbAlongX.Checked;

            // Create wall spec
            var spec = WallSpec.CreateRectangular(
                wallName,
                storyName,
                sectionName,
                x0, y0, z0,
                length, height,
                isAlongX
            );

            spec.ThicknessMm = thicknessMm;
            spec.MaterialName = materialName;
            spec.LocalAxisAngleDeg = (double)numAngle.Value;

            if (!string.IsNullOrWhiteSpace(txtPierLabel.Text))
                spec.PierLabel = txtPierLabel.Text.Trim();

            if (!string.IsNullOrWhiteSpace(txtSpandrelLabel.Text))
                spec.SpandrelLabel = txtSpandrelLabel.Text.Trim();

            // Create wall
            if (_wallsService.UpsertWall(spec, out string report))
            {
                _statusTextBox.AppendText(report);
                _statusTextBox.AppendText("\r\n✓ Wall operation completed successfully!\r\n");
                MessageBox.Show($"Wall '{wallName}' created/updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _statusTextBox.AppendText(report);
                _statusTextBox.AppendText("\r\n✗ Wall operation failed!\r\n");
                MessageBox.Show($"Failed to create/update wall:\n\n{report}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _statusTextBox.AppendText($"\r\nERROR: {ex.Message}\r\n");
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void btnRefreshStories_Click(object sender, EventArgs e)
    {
        LoadStories();
        _statusTextBox.AppendText("Stories refreshed.\r\n");
    }
}
