namespace ETABS_Plugin
{
    partial class WallPlacementForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitle = new System.Windows.Forms.Label();
            grpCoordinates = new System.Windows.Forms.GroupBox();
            lblPoint4 = new System.Windows.Forms.Label();
            lblPoint3 = new System.Windows.Forms.Label();
            lblPoint2 = new System.Windows.Forms.Label();
            lblPoint1 = new System.Windows.Forms.Label();
            numY4 = new System.Windows.Forms.NumericUpDown();
            numX4 = new System.Windows.Forms.NumericUpDown();
            numY3 = new System.Windows.Forms.NumericUpDown();
            numX3 = new System.Windows.Forms.NumericUpDown();
            numY2 = new System.Windows.Forms.NumericUpDown();
            numX2 = new System.Windows.Forms.NumericUpDown();
            numY1 = new System.Windows.Forms.NumericUpDown();
            numX1 = new System.Windows.Forms.NumericUpDown();
            lblY = new System.Windows.Forms.Label();
            lblX = new System.Windows.Forms.Label();
            grpProperties = new System.Windows.Forms.GroupBox();
            lblPierLabel = new System.Windows.Forms.Label();
            txtPierLabel = new System.Windows.Forms.TextBox();
            lblBetaDeg = new System.Windows.Forms.Label();
            lblBetaAngle = new System.Windows.Forms.Label();
            numBetaAngle = new System.Windows.Forms.NumericUpDown();
            lblThicknessMm = new System.Windows.Forms.Label();
            lblThickness = new System.Windows.Forms.Label();
            numThickness = new System.Windows.Forms.NumericUpDown();
            lblMaterial = new System.Windows.Forms.Label();
            cmbMaterial = new System.Windows.Forms.ComboBox();
            lblElevationM = new System.Windows.Forms.Label();
            lblElevation = new System.Windows.Forms.Label();
            numElevation = new System.Windows.Forms.NumericUpDown();
            btnPlaceWall = new System.Windows.Forms.Button();
            btnLoadExample = new System.Windows.Forms.Button();
            btnGenerateTemplate = new System.Windows.Forms.Button();
            btnImportCsv = new System.Windows.Forms.Button();
            txtStatus = new System.Windows.Forms.TextBox();
            lblStatus = new System.Windows.Forms.Label();
            grpCoordinates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numY4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numX4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numY3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numX3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numY2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numX2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numY1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numX1).BeginInit();
            grpProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBetaAngle).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numThickness).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numElevation).BeginInit();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblTitle.Location = new System.Drawing.Point(12, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(145, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Wall Placement";
            //
            // grpCoordinates
            //
            grpCoordinates.Controls.Add(lblPoint4);
            grpCoordinates.Controls.Add(lblPoint3);
            grpCoordinates.Controls.Add(lblPoint2);
            grpCoordinates.Controls.Add(lblPoint1);
            grpCoordinates.Controls.Add(numY4);
            grpCoordinates.Controls.Add(numX4);
            grpCoordinates.Controls.Add(numY3);
            grpCoordinates.Controls.Add(numX3);
            grpCoordinates.Controls.Add(numY2);
            grpCoordinates.Controls.Add(numX2);
            grpCoordinates.Controls.Add(numY1);
            grpCoordinates.Controls.Add(numX1);
            grpCoordinates.Controls.Add(lblY);
            grpCoordinates.Controls.Add(lblX);
            grpCoordinates.Location = new System.Drawing.Point(12, 42);
            grpCoordinates.Name = "grpCoordinates";
            grpCoordinates.Size = new System.Drawing.Size(360, 180);
            grpCoordinates.TabIndex = 1;
            grpCoordinates.TabStop = false;
            grpCoordinates.Text = "Corner Coordinates (meters)";
            //
            // lblPoint4
            //
            lblPoint4.AutoSize = true;
            lblPoint4.Location = new System.Drawing.Point(15, 140);
            lblPoint4.Name = "lblPoint4";
            lblPoint4.Size = new System.Drawing.Size(52, 15);
            lblPoint4.TabIndex = 13;
            lblPoint4.Text = "Point 4:";
            //
            // lblPoint3
            //
            lblPoint3.AutoSize = true;
            lblPoint3.Location = new System.Drawing.Point(15, 110);
            lblPoint3.Name = "lblPoint3";
            lblPoint3.Size = new System.Drawing.Size(52, 15);
            lblPoint3.TabIndex = 12;
            lblPoint3.Text = "Point 3:";
            //
            // lblPoint2
            //
            lblPoint2.AutoSize = true;
            lblPoint2.Location = new System.Drawing.Point(15, 80);
            lblPoint2.Name = "lblPoint2";
            lblPoint2.Size = new System.Drawing.Size(52, 15);
            lblPoint2.TabIndex = 11;
            lblPoint2.Text = "Point 2:";
            //
            // lblPoint1
            //
            lblPoint1.AutoSize = true;
            lblPoint1.Location = new System.Drawing.Point(15, 50);
            lblPoint1.Name = "lblPoint1";
            lblPoint1.Size = new System.Drawing.Size(52, 15);
            lblPoint1.TabIndex = 10;
            lblPoint1.Text = "Point 1:";
            //
            // numY4
            //
            numY4.DecimalPlaces = 2;
            numY4.Location = new System.Drawing.Point(245, 138);
            numY4.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numY4.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numY4.Name = "numY4";
            numY4.Size = new System.Drawing.Size(100, 23);
            numY4.TabIndex = 9;
            numY4.Value = new decimal(new int[] { 2, 0, 0, 65536 });
            //
            // numX4
            //
            numX4.DecimalPlaces = 2;
            numX4.Location = new System.Drawing.Point(115, 138);
            numX4.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numX4.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numX4.Name = "numX4";
            numX4.Size = new System.Drawing.Size(100, 23);
            numX4.TabIndex = 8;
            //
            // numY3
            //
            numY3.DecimalPlaces = 2;
            numY3.Location = new System.Drawing.Point(245, 108);
            numY3.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numY3.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numY3.Name = "numY3";
            numY3.Size = new System.Drawing.Size(100, 23);
            numY3.TabIndex = 7;
            numY3.Value = new decimal(new int[] { 2, 0, 0, 65536 });
            //
            // numX3
            //
            numX3.DecimalPlaces = 2;
            numX3.Location = new System.Drawing.Point(115, 108);
            numX3.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numX3.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numX3.Name = "numX3";
            numX3.Size = new System.Drawing.Size(100, 23);
            numX3.TabIndex = 6;
            numX3.Value = new decimal(new int[] { 4, 0, 0, 0 });
            //
            // numY2
            //
            numY2.DecimalPlaces = 2;
            numY2.Location = new System.Drawing.Point(245, 78);
            numY2.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numY2.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numY2.Name = "numY2";
            numY2.Size = new System.Drawing.Size(100, 23);
            numY2.TabIndex = 5;
            //
            // numX2
            //
            numX2.DecimalPlaces = 2;
            numX2.Location = new System.Drawing.Point(115, 78);
            numX2.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numX2.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numX2.Name = "numX2";
            numX2.Size = new System.Drawing.Size(100, 23);
            numX2.TabIndex = 4;
            numX2.Value = new decimal(new int[] { 4, 0, 0, 0 });
            //
            // numY1
            //
            numY1.DecimalPlaces = 2;
            numY1.Location = new System.Drawing.Point(245, 48);
            numY1.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numY1.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numY1.Name = "numY1";
            numY1.Size = new System.Drawing.Size(100, 23);
            numY1.TabIndex = 3;
            //
            // numX1
            //
            numX1.DecimalPlaces = 2;
            numX1.Location = new System.Drawing.Point(115, 48);
            numX1.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numX1.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numX1.Name = "numX1";
            numX1.Size = new System.Drawing.Size(100, 23);
            numX1.TabIndex = 2;
            //
            // lblY
            //
            lblY.AutoSize = true;
            lblY.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblY.Location = new System.Drawing.Point(280, 25);
            lblY.Name = "lblY";
            lblY.Size = new System.Drawing.Size(17, 15);
            lblY.TabIndex = 1;
            lblY.Text = "Y";
            //
            // lblX
            //
            lblX.AutoSize = true;
            lblX.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblX.Location = new System.Drawing.Point(150, 25);
            lblX.Name = "lblX";
            lblX.Size = new System.Drawing.Size(17, 15);
            lblX.TabIndex = 0;
            lblX.Text = "X";
            //
            // grpProperties
            //
            grpProperties.Controls.Add(lblPierLabel);
            grpProperties.Controls.Add(txtPierLabel);
            grpProperties.Controls.Add(lblBetaDeg);
            grpProperties.Controls.Add(lblBetaAngle);
            grpProperties.Controls.Add(numBetaAngle);
            grpProperties.Controls.Add(lblThicknessMm);
            grpProperties.Controls.Add(lblThickness);
            grpProperties.Controls.Add(numThickness);
            grpProperties.Controls.Add(lblMaterial);
            grpProperties.Controls.Add(cmbMaterial);
            grpProperties.Controls.Add(lblElevationM);
            grpProperties.Controls.Add(lblElevation);
            grpProperties.Controls.Add(numElevation);
            grpProperties.Location = new System.Drawing.Point(12, 238);
            grpProperties.Name = "grpProperties";
            grpProperties.Size = new System.Drawing.Size(360, 210);
            grpProperties.TabIndex = 2;
            grpProperties.TabStop = false;
            grpProperties.Text = "Wall Properties";
            //
            // lblPierLabel
            //
            lblPierLabel.AutoSize = true;
            lblPierLabel.Location = new System.Drawing.Point(15, 175);
            lblPierLabel.Name = "lblPierLabel";
            lblPierLabel.Size = new System.Drawing.Size(66, 15);
            lblPierLabel.TabIndex = 12;
            lblPierLabel.Text = "Pier Label:";
            //
            // txtPierLabel
            //
            txtPierLabel.Location = new System.Drawing.Point(115, 172);
            txtPierLabel.Name = "txtPierLabel";
            txtPierLabel.Size = new System.Drawing.Size(100, 23);
            txtPierLabel.TabIndex = 11;
            txtPierLabel.Text = "P1";
            //
            // lblBetaDeg
            //
            lblBetaDeg.AutoSize = true;
            lblBetaDeg.Location = new System.Drawing.Point(300, 145);
            lblBetaDeg.Name = "lblBetaDeg";
            lblBetaDeg.Size = new System.Drawing.Size(45, 15);
            lblBetaDeg.TabIndex = 10;
            lblBetaDeg.Text = "degrees";
            //
            // lblBetaAngle
            //
            lblBetaAngle.AutoSize = true;
            lblBetaAngle.Location = new System.Drawing.Point(15, 145);
            lblBetaAngle.Name = "lblBetaAngle";
            lblBetaAngle.Size = new System.Drawing.Size(93, 15);
            lblBetaAngle.TabIndex = 9;
            lblBetaAngle.Text = "Beta Angle (β):";
            //
            // numBetaAngle
            //
            numBetaAngle.DecimalPlaces = 1;
            numBetaAngle.Location = new System.Drawing.Point(115, 143);
            numBetaAngle.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            numBetaAngle.Minimum = new decimal(new int[] { 360, 0, 0, int.MinValue });
            numBetaAngle.Name = "numBetaAngle";
            numBetaAngle.Size = new System.Drawing.Size(100, 23);
            numBetaAngle.TabIndex = 8;
            numBetaAngle.Value = new decimal(new int[] { 90, 0, 0, 0 });
            //
            // lblThicknessMm
            //
            lblThicknessMm.AutoSize = true;
            lblThicknessMm.Location = new System.Drawing.Point(300, 115);
            lblThicknessMm.Name = "lblThicknessMm";
            lblThicknessMm.Size = new System.Drawing.Size(29, 15);
            lblThicknessMm.TabIndex = 7;
            lblThicknessMm.Text = "mm";
            //
            // lblThickness
            //
            lblThickness.AutoSize = true;
            lblThickness.Location = new System.Drawing.Point(15, 115);
            lblThickness.Name = "lblThickness";
            lblThickness.Size = new System.Drawing.Size(62, 15);
            lblThickness.TabIndex = 6;
            lblThickness.Text = "Thickness:";
            //
            // numThickness
            //
            numThickness.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numThickness.Location = new System.Drawing.Point(115, 113);
            numThickness.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numThickness.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            numThickness.Name = "numThickness";
            numThickness.Size = new System.Drawing.Size(100, 23);
            numThickness.TabIndex = 5;
            numThickness.Value = new decimal(new int[] { 200, 0, 0, 0 });
            //
            // lblMaterial
            //
            lblMaterial.AutoSize = true;
            lblMaterial.Location = new System.Drawing.Point(15, 85);
            lblMaterial.Name = "lblMaterial";
            lblMaterial.Size = new System.Drawing.Size(56, 15);
            lblMaterial.TabIndex = 4;
            lblMaterial.Text = "Material:";
            //
            // cmbMaterial
            //
            cmbMaterial.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbMaterial.FormattingEnabled = true;
            cmbMaterial.Location = new System.Drawing.Point(115, 82);
            cmbMaterial.Name = "cmbMaterial";
            cmbMaterial.Size = new System.Drawing.Size(230, 23);
            cmbMaterial.TabIndex = 3;
            //
            // lblElevationM
            //
            lblElevationM.AutoSize = true;
            lblElevationM.Location = new System.Drawing.Point(300, 55);
            lblElevationM.Name = "lblElevationM";
            lblElevationM.Size = new System.Drawing.Size(18, 15);
            lblElevationM.TabIndex = 2;
            lblElevationM.Text = "m";
            //
            // lblElevation
            //
            lblElevation.AutoSize = true;
            lblElevation.Location = new System.Drawing.Point(15, 55);
            lblElevation.Name = "lblElevation";
            lblElevation.Size = new System.Drawing.Size(94, 15);
            lblElevation.TabIndex = 1;
            lblElevation.Text = "Elevation (Z):";
            //
            // numElevation
            //
            numElevation.DecimalPlaces = 2;
            numElevation.Location = new System.Drawing.Point(115, 53);
            numElevation.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numElevation.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            numElevation.Name = "numElevation";
            numElevation.Size = new System.Drawing.Size(100, 23);
            numElevation.TabIndex = 0;
            numElevation.Value = new decimal(new int[] { 3, 0, 0, 0 });
            //
            // btnPlaceWall
            //
            btnPlaceWall.BackColor = System.Drawing.Color.LightGreen;
            btnPlaceWall.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            btnPlaceWall.Location = new System.Drawing.Point(12, 464);
            btnPlaceWall.Name = "btnPlaceWall";
            btnPlaceWall.Size = new System.Drawing.Size(250, 40);
            btnPlaceWall.TabIndex = 3;
            btnPlaceWall.Text = "Place Wall";
            btnPlaceWall.UseVisualStyleBackColor = false;
            btnPlaceWall.Click += btnPlaceWall_Click;
            //
            // btnLoadExample
            //
            btnLoadExample.Location = new System.Drawing.Point(268, 464);
            btnLoadExample.Name = "btnLoadExample";
            btnLoadExample.Size = new System.Drawing.Size(104, 40);
            btnLoadExample.TabIndex = 4;
            btnLoadExample.Text = "Load Example";
            btnLoadExample.UseVisualStyleBackColor = true;
            btnLoadExample.Click += btnLoadExample_Click;
            //
            // btnGenerateTemplate
            //
            btnGenerateTemplate.Location = new System.Drawing.Point(12, 510);
            btnGenerateTemplate.Name = "btnGenerateTemplate";
            btnGenerateTemplate.Size = new System.Drawing.Size(175, 35);
            btnGenerateTemplate.TabIndex = 7;
            btnGenerateTemplate.Text = "1. Generate CSV Template";
            btnGenerateTemplate.UseVisualStyleBackColor = true;
            btnGenerateTemplate.Click += btnGenerateTemplate_Click;
            //
            // btnImportCsv
            //
            btnImportCsv.BackColor = System.Drawing.Color.LightBlue;
            btnImportCsv.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            btnImportCsv.Location = new System.Drawing.Point(193, 510);
            btnImportCsv.Name = "btnImportCsv";
            btnImportCsv.Size = new System.Drawing.Size(179, 35);
            btnImportCsv.TabIndex = 8;
            btnImportCsv.Text = "2. Import from CSV";
            btnImportCsv.UseVisualStyleBackColor = false;
            btnImportCsv.Click += btnImportCsv_Click;
            //
            // txtStatus
            //
            txtStatus.Location = new System.Drawing.Point(12, 571);
            txtStatus.Multiline = true;
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtStatus.Size = new System.Drawing.Size(360, 120);
            txtStatus.TabIndex = 5;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Location = new System.Drawing.Point(12, 553);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(42, 15);
            lblStatus.TabIndex = 6;
            lblStatus.Text = "Status:";
            //
            // WallPlacementForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(384, 703);
            Controls.Add(lblStatus);
            Controls.Add(txtStatus);
            Controls.Add(btnImportCsv);
            Controls.Add(btnGenerateTemplate);
            Controls.Add(btnLoadExample);
            Controls.Add(btnPlaceWall);
            Controls.Add(grpProperties);
            Controls.Add(grpCoordinates);
            Controls.Add(lblTitle);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "WallPlacementForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ETABS Wall Placement";
            Load += WallPlacementForm_Load;
            grpCoordinates.ResumeLayout(false);
            grpCoordinates.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numY4).EndInit();
            ((System.ComponentModel.ISupportInitialize)numX4).EndInit();
            ((System.ComponentModel.ISupportInitialize)numY3).EndInit();
            ((System.ComponentModel.ISupportInitialize)numX3).EndInit();
            ((System.ComponentModel.ISupportInitialize)numY2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numX2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numY1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numX1).EndInit();
            grpProperties.ResumeLayout(false);
            grpProperties.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numBetaAngle).EndInit();
            ((System.ComponentModel.ISupportInitialize)numThickness).EndInit();
            ((System.ComponentModel.ISupportInitialize)numElevation).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpCoordinates;
        private System.Windows.Forms.NumericUpDown numX1;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.NumericUpDown numY4;
        private System.Windows.Forms.NumericUpDown numX4;
        private System.Windows.Forms.NumericUpDown numY3;
        private System.Windows.Forms.NumericUpDown numX3;
        private System.Windows.Forms.NumericUpDown numY2;
        private System.Windows.Forms.NumericUpDown numX2;
        private System.Windows.Forms.NumericUpDown numY1;
        private System.Windows.Forms.Label lblPoint4;
        private System.Windows.Forms.Label lblPoint3;
        private System.Windows.Forms.Label lblPoint2;
        private System.Windows.Forms.Label lblPoint1;
        private System.Windows.Forms.GroupBox grpProperties;
        private System.Windows.Forms.Label lblElevation;
        private System.Windows.Forms.NumericUpDown numElevation;
        private System.Windows.Forms.Label lblMaterial;
        private System.Windows.Forms.ComboBox cmbMaterial;
        private System.Windows.Forms.Label lblElevationM;
        private System.Windows.Forms.Label lblThicknessMm;
        private System.Windows.Forms.Label lblThickness;
        private System.Windows.Forms.NumericUpDown numThickness;
        private System.Windows.Forms.Label lblBetaDeg;
        private System.Windows.Forms.Label lblBetaAngle;
        private System.Windows.Forms.NumericUpDown numBetaAngle;
        private System.Windows.Forms.Label lblPierLabel;
        private System.Windows.Forms.TextBox txtPierLabel;
        private System.Windows.Forms.Button btnPlaceWall;
        private System.Windows.Forms.Button btnLoadExample;
        private System.Windows.Forms.Button btnGenerateTemplate;
        private System.Windows.Forms.Button btnImportCsv;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label lblStatus;
    }
}
