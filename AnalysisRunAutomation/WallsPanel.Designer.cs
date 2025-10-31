namespace ETABS_Plugin
{
    partial class WallsPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            grpWallDefinition = new GroupBox();
            lblWallName = new Label();
            txtWallName = new TextBox();
            lblStory = new Label();
            cmbStory = new ComboBox();
            btnRefreshStories = new Button();
            grpSection = new GroupBox();
            lblSectionName = new Label();
            txtSectionName = new TextBox();
            lblThickness = new Label();
            numThickness = new NumericUpDown();
            lblThicknessMm = new Label();
            lblMaterial = new Label();
            txtMaterialName = new TextBox();
            grpGeometry = new GroupBox();
            lblX0 = new Label();
            numX0 = new NumericUpDown();
            lblY0 = new Label();
            numY0 = new NumericUpDown();
            lblZ0 = new Label();
            numZ0 = new NumericUpDown();
            lblLength = new Label();
            numLength = new NumericUpDown();
            lblHeight = new Label();
            numHeight = new NumericUpDown();
            rbAlongX = new RadioButton();
            rbAlongY = new RadioButton();
            grpOrientation = new GroupBox();
            lblAngle = new Label();
            numAngle = new NumericUpDown();
            lblDegrees = new Label();
            grpDesignLabels = new GroupBox();
            lblPier = new Label();
            txtPierLabel = new TextBox();
            lblSpandrel = new Label();
            txtSpandrelLabel = new TextBox();
            btnCreateWall = new Button();

            grpWallDefinition.SuspendLayout();
            grpSection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numThickness).BeginInit();
            grpGeometry.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numX0).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numY0).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numZ0).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLength).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
            grpOrientation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numAngle).BeginInit();
            grpDesignLabels.SuspendLayout();
            SuspendLayout();

            // grpWallDefinition
            grpWallDefinition.Controls.Add(lblWallName);
            grpWallDefinition.Controls.Add(txtWallName);
            grpWallDefinition.Controls.Add(lblStory);
            grpWallDefinition.Controls.Add(cmbStory);
            grpWallDefinition.Controls.Add(btnRefreshStories);
            grpWallDefinition.Location = new Point(10, 10);
            grpWallDefinition.Name = "grpWallDefinition";
            grpWallDefinition.Size = new Size(350, 120);
            grpWallDefinition.TabIndex = 0;
            grpWallDefinition.TabStop = false;
            grpWallDefinition.Text = "Wall Definition";

            // lblWallName
            lblWallName.AutoSize = true;
            lblWallName.Location = new Point(10, 25);
            lblWallName.Name = "lblWallName";
            lblWallName.Size = new Size(80, 20);
            lblWallName.Text = "Wall Name:";

            // txtWallName
            txtWallName.Location = new Point(100, 22);
            txtWallName.Name = "txtWallName";
            txtWallName.Size = new Size(200, 27);
            txtWallName.Text = "W_Story1_1";

            // lblStory
            lblStory.AutoSize = true;
            lblStory.Location = new Point(10, 60);
            lblStory.Name = "lblStory";
            lblStory.Size = new Size(45, 20);
            lblStory.Text = "Story:";

            // cmbStory
            cmbStory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStory.Location = new Point(100, 57);
            cmbStory.Name = "cmbStory";
            cmbStory.Size = new Size(150, 28);

            // btnRefreshStories
            btnRefreshStories.Location = new Point(260, 56);
            btnRefreshStories.Name = "btnRefreshStories";
            btnRefreshStories.Size = new Size(80, 30);
            btnRefreshStories.Text = "Refresh";
            btnRefreshStories.Click += btnRefreshStories_Click;

            // grpSection
            grpSection.Controls.Add(lblSectionName);
            grpSection.Controls.Add(txtSectionName);
            grpSection.Controls.Add(lblThickness);
            grpSection.Controls.Add(numThickness);
            grpSection.Controls.Add(lblThicknessMm);
            grpSection.Controls.Add(lblMaterial);
            grpSection.Controls.Add(txtMaterialName);
            grpSection.Location = new Point(10, 140);
            grpSection.Name = "grpSection";
            grpSection.Size = new Size(350, 140);
            grpSection.TabIndex = 1;
            grpSection.TabStop = false;
            grpSection.Text = "Wall Section Properties";

            // lblSectionName
            lblSectionName.AutoSize = true;
            lblSectionName.Location = new Point(10, 25);
            lblSectionName.Name = "lblSectionName";
            lblSectionName.Size = new Size(100, 20);
            lblSectionName.Text = "Section Name:";

            // txtSectionName
            txtSectionName.Location = new Point(120, 22);
            txtSectionName.Name = "txtSectionName";
            txtSectionName.Size = new Size(200, 27);
            txtSectionName.Text = "WALL_200mm_C30";

            // lblThickness
            lblThickness.AutoSize = true;
            lblThickness.Location = new Point(10, 60);
            lblThickness.Name = "lblThickness";
            lblThickness.Size = new Size(75, 20);
            lblThickness.Text = "Thickness:";

            // numThickness
            numThickness.Location = new Point(120, 58);
            numThickness.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numThickness.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            numThickness.Name = "numThickness";
            numThickness.Size = new Size(100, 27);
            numThickness.Value = new decimal(new int[] { 200, 0, 0, 0 });

            // lblThicknessMm
            lblThicknessMm.AutoSize = true;
            lblThicknessMm.Location = new Point(225, 60);
            lblThicknessMm.Name = "lblThicknessMm";
            lblThicknessMm.Size = new Size(35, 20);
            lblThicknessMm.Text = "mm";

            // lblMaterial
            lblMaterial.AutoSize = true;
            lblMaterial.Location = new Point(10, 95);
            lblMaterial.Name = "lblMaterial";
            lblMaterial.Size = new Size(65, 20);
            lblMaterial.Text = "Material:";

            // txtMaterialName
            txtMaterialName.Location = new Point(120, 92);
            txtMaterialName.Name = "txtMaterialName";
            txtMaterialName.Size = new Size(200, 27);
            txtMaterialName.Text = "CONC-C30";

            // grpGeometry
            grpGeometry.Controls.Add(lblX0);
            grpGeometry.Controls.Add(numX0);
            grpGeometry.Controls.Add(lblY0);
            grpGeometry.Controls.Add(numY0);
            grpGeometry.Controls.Add(lblZ0);
            grpGeometry.Controls.Add(numZ0);
            grpGeometry.Controls.Add(lblLength);
            grpGeometry.Controls.Add(numLength);
            grpGeometry.Controls.Add(lblHeight);
            grpGeometry.Controls.Add(numHeight);
            grpGeometry.Controls.Add(rbAlongX);
            grpGeometry.Controls.Add(rbAlongY);
            grpGeometry.Location = new Point(10, 290);
            grpGeometry.Name = "grpGeometry";
            grpGeometry.Size = new Size(350, 200);
            grpGeometry.TabIndex = 2;
            grpGeometry.TabStop = false;
            grpGeometry.Text = "Geometry (meters)";

            // lblX0
            lblX0.AutoSize = true;
            lblX0.Location = new Point(10, 25);
            lblX0.Text = "X₀:";

            // numX0
            numX0.DecimalPlaces = 3;
            numX0.Location = new Point(50, 23);
            numX0.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numX0.Minimum = new decimal(new int[] { -1000, 0, 0, int.MinValue });
            numX0.Name = "numX0";
            numX0.Size = new Size(80, 27);
            numX0.Value = new decimal(new int[] { 0, 0, 0, 0 });

            // lblY0
            lblY0.AutoSize = true;
            lblY0.Location = new Point(140, 25);
            lblY0.Text = "Y₀:";

            // numY0
            numY0.DecimalPlaces = 3;
            numY0.Location = new Point(180, 23);
            numY0.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numY0.Minimum = new decimal(new int[] { -1000, 0, 0, int.MinValue });
            numY0.Name = "numY0";
            numY0.Size = new Size(80, 27);
            numY0.Value = new decimal(new int[] { 0, 0, 0, 0 });

            // lblZ0
            lblZ0.AutoSize = true;
            lblZ0.Location = new Point(10, 60);
            lblZ0.Text = "Z₀:";

            // numZ0
            numZ0.DecimalPlaces = 3;
            numZ0.Location = new Point(50, 58);
            numZ0.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numZ0.Minimum = new decimal(new int[] { -1000, 0, 0, int.MinValue });
            numZ0.Name = "numZ0";
            numZ0.Size = new Size(80, 27);
            numZ0.Value = new decimal(new int[] { 0, 0, 0, 0 });

            // lblLength
            lblLength.AutoSize = true;
            lblLength.Location = new Point(10, 95);
            lblLength.Text = "Length:";

            // numLength
            numLength.DecimalPlaces = 3;
            numLength.Location = new Point(80, 93);
            numLength.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numLength.Minimum = new decimal(new int[] { 1, 0, 0, 196608 });
            numLength.Name = "numLength";
            numLength.Size = new Size(80, 27);
            numLength.Value = new decimal(new int[] { 4, 0, 0, 0 });

            // lblHeight
            lblHeight.AutoSize = true;
            lblHeight.Location = new Point(10, 130);
            lblHeight.Text = "Height:";

            // numHeight
            numHeight.DecimalPlaces = 3;
            numHeight.Location = new Point(80, 128);
            numHeight.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numHeight.Minimum = new decimal(new int[] { 1, 0, 0, 196608 });
            numHeight.Name = "numHeight";
            numHeight.Size = new Size(80, 27);
            numHeight.Value = new decimal(new int[] { 3, 0, 0, 0 });

            // rbAlongX
            rbAlongX.AutoSize = true;
            rbAlongX.Checked = true;
            rbAlongX.Location = new Point(10, 165);
            rbAlongX.Name = "rbAlongX";
            rbAlongX.Size = new Size(100, 24);
            rbAlongX.TabStop = true;
            rbAlongX.Text = "Along X axis";

            // rbAlongY
            rbAlongY.AutoSize = true;
            rbAlongY.Location = new Point(120, 165);
            rbAlongY.Name = "rbAlongY";
            rbAlongY.Size = new Size(100, 24);
            rbAlongY.Text = "Along Y axis";

            // grpOrientation
            grpOrientation.Controls.Add(lblAngle);
            grpOrientation.Controls.Add(numAngle);
            grpOrientation.Controls.Add(lblDegrees);
            grpOrientation.Location = new Point(10, 500);
            grpOrientation.Name = "grpOrientation";
            grpOrientation.Size = new Size(350, 70);
            grpOrientation.TabIndex = 3;
            grpOrientation.TabStop = false;
            grpOrientation.Text = "Orientation";

            // lblAngle
            lblAngle.AutoSize = true;
            lblAngle.Location = new Point(10, 30);
            lblAngle.Text = "Local Axis β:";

            // numAngle
            numAngle.DecimalPlaces = 2;
            numAngle.Location = new Point(100, 28);
            numAngle.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            numAngle.Minimum = new decimal(new int[] { -360, 0, 0, int.MinValue });
            numAngle.Name = "numAngle";
            numAngle.Size = new Size(100, 27);
            numAngle.Value = new decimal(new int[] { 0, 0, 0, 0 });

            // lblDegrees
            lblDegrees.AutoSize = true;
            lblDegrees.Location = new Point(210, 30);
            lblDegrees.Text = "degrees";

            // grpDesignLabels
            grpDesignLabels.Controls.Add(lblPier);
            grpDesignLabels.Controls.Add(txtPierLabel);
            grpDesignLabels.Controls.Add(lblSpandrel);
            grpDesignLabels.Controls.Add(txtSpandrelLabel);
            grpDesignLabels.Location = new Point(10, 580);
            grpDesignLabels.Name = "grpDesignLabels";
            grpDesignLabels.Size = new Size(350, 100);
            grpDesignLabels.TabIndex = 4;
            grpDesignLabels.TabStop = false;
            grpDesignLabels.Text = "Design Labels (Optional)";

            // lblPier
            lblPier.AutoSize = true;
            lblPier.Location = new Point(10, 30);
            lblPier.Text = "Pier:";

            // txtPierLabel
            txtPierLabel.Location = new Point(100, 27);
            txtPierLabel.Name = "txtPierLabel";
            txtPierLabel.Size = new Size(200, 27);

            // lblSpandrel
            lblSpandrel.AutoSize = true;
            lblSpandrel.Location = new Point(10, 65);
            lblSpandrel.Text = "Spandrel:";

            // txtSpandrel Label
            txtSpandrelLabel.Location = new Point(100, 62);
            txtSpandrelLabel.Name = "txtSpandrelLabel";
            txtSpandrelLabel.Size = new Size(200, 27);

            // btnCreateWall
            btnCreateWall.BackColor = Color.LightGreen;
            btnCreateWall.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnCreateWall.Location = new Point(10, 690);
            btnCreateWall.Name = "btnCreateWall";
            btnCreateWall.Size = new Size(350, 45);
            btnCreateWall.Text = "Create/Update Wall";
            btnCreateWall.UseVisualStyleBackColor = false;
            btnCreateWall.Click += btnCreateWall_Click;

            // WallsPanel
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpWallDefinition);
            Controls.Add(grpSection);
            Controls.Add(grpGeometry);
            Controls.Add(grpOrientation);
            Controls.Add(grpDesignLabels);
            Controls.Add(btnCreateWall);
            Name = "WallsPanel";
            Size = new Size(370, 750);
            grpWallDefinition.ResumeLayout(false);
            grpWallDefinition.PerformLayout();
            grpSection.ResumeLayout(false);
            grpSection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numThickness).EndInit();
            grpGeometry.ResumeLayout(false);
            grpGeometry.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numX0).EndInit();
            ((System.ComponentModel.ISupportInitialize)numY0).EndInit();
            ((System.ComponentModel.ISupportInitialize)numZ0).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLength).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
            grpOrientation.ResumeLayout(false);
            grpOrientation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numAngle).EndInit();
            grpDesignLabels.ResumeLayout(false);
            grpDesignLabels.PerformLayout();
            ResumeLayout(false);
        }

        private GroupBox grpWallDefinition;
        private Label lblWallName;
        private TextBox txtWallName;
        private Label lblStory;
        private ComboBox cmbStory;
        private Button btnRefreshStories;
        private GroupBox grpSection;
        private Label lblSectionName;
        private TextBox txtSectionName;
        private Label lblThickness;
        private NumericUpDown numThickness;
        private Label lblThicknessMm;
        private Label lblMaterial;
        private TextBox txtMaterialName;
        private GroupBox grpGeometry;
        private Label lblX0;
        private NumericUpDown numX0;
        private Label lblY0;
        private NumericUpDown numY0;
        private Label lblZ0;
        private NumericUpDown numZ0;
        private Label lblLength;
        private NumericUpDown numLength;
        private Label lblHeight;
        private NumericUpDown numHeight;
        private RadioButton rbAlongX;
        private RadioButton rbAlongY;
        private GroupBox grpOrientation;
        private Label lblAngle;
        private NumericUpDown numAngle;
        private Label lblDegrees;
        private GroupBox grpDesignLabels;
        private Label lblPier;
        private TextBox txtPierLabel;
        private Label lblSpandrel;
        private TextBox txtSpandrelLabel;
        private Button btnCreateWall;
    }
}
