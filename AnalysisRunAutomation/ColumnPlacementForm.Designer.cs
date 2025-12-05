namespace ETABS_Plugin
{
    partial class ColumnPlacementForm
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
            lblBaseElevationM = new System.Windows.Forms.Label();
            lblBaseElevation = new System.Windows.Forms.Label();
            numBaseElevation = new System.Windows.Forms.NumericUpDown();
            lblY = new System.Windows.Forms.Label();
            lblX = new System.Windows.Forms.Label();
            lblYCoord = new System.Windows.Forms.Label();
            lblXCoord = new System.Windows.Forms.Label();
            numY = new System.Windows.Forms.NumericUpDown();
            numX = new System.Windows.Forms.NumericUpDown();
            grpProperties = new System.Windows.Forms.GroupBox();
            lblColumnLabel = new System.Windows.Forms.Label();
            txtColumnLabel = new System.Windows.Forms.TextBox();
            lblRotationDeg = new System.Windows.Forms.Label();
            lblRotationAngle = new System.Windows.Forms.Label();
            numRotationAngle = new System.Windows.Forms.NumericUpDown();
            lblSection = new System.Windows.Forms.Label();
            cmbSection = new System.Windows.Forms.ComboBox();
            lblHeightM = new System.Windows.Forms.Label();
            lblHeight = new System.Windows.Forms.Label();
            numHeight = new System.Windows.Forms.NumericUpDown();
            btnPlaceColumn = new System.Windows.Forms.Button();
            btnLoadExample = new System.Windows.Forms.Button();
            btnGenerateTemplate = new System.Windows.Forms.Button();
            btnImportCsv = new System.Windows.Forms.Button();
            txtStatus = new System.Windows.Forms.TextBox();
            lblStatus = new System.Windows.Forms.Label();
            grpCoordinates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBaseElevation).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numX).BeginInit();
            grpProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numRotationAngle).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblTitle.Location = new System.Drawing.Point(12, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(168, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Column Placement";
            //
            // grpCoordinates
            //
            grpCoordinates.Controls.Add(lblBaseElevationM);
            grpCoordinates.Controls.Add(lblBaseElevation);
            grpCoordinates.Controls.Add(numBaseElevation);
            grpCoordinates.Controls.Add(lblY);
            grpCoordinates.Controls.Add(lblX);
            grpCoordinates.Controls.Add(lblYCoord);
            grpCoordinates.Controls.Add(lblXCoord);
            grpCoordinates.Controls.Add(numY);
            grpCoordinates.Controls.Add(numX);
            grpCoordinates.Location = new System.Drawing.Point(12, 42);
            grpCoordinates.Name = "grpCoordinates";
            grpCoordinates.Size = new System.Drawing.Size(360, 130);
            grpCoordinates.TabIndex = 1;
            grpCoordinates.TabStop = false;
            grpCoordinates.Text = "Column Location (meters)";
            //
            // lblBaseElevationM
            //
            lblBaseElevationM.AutoSize = true;
            lblBaseElevationM.Location = new System.Drawing.Point(300, 95);
            lblBaseElevationM.Name = "lblBaseElevationM";
            lblBaseElevationM.Size = new System.Drawing.Size(18, 15);
            lblBaseElevationM.TabIndex = 8;
            lblBaseElevationM.Text = "m";
            //
            // lblBaseElevation
            //
            lblBaseElevation.AutoSize = true;
            lblBaseElevation.Location = new System.Drawing.Point(15, 95);
            lblBaseElevation.Name = "lblBaseElevation";
            lblBaseElevation.Size = new System.Drawing.Size(94, 15);
            lblBaseElevation.TabIndex = 7;
            lblBaseElevation.Text = "Base Elevation:";
            //
            // numBaseElevation
            //
            numBaseElevation.DecimalPlaces = 2;
            numBaseElevation.Location = new System.Drawing.Point(115, 93);
            numBaseElevation.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numBaseElevation.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            numBaseElevation.Name = "numBaseElevation";
            numBaseElevation.Size = new System.Drawing.Size(100, 23);
            numBaseElevation.TabIndex = 6;
            //
            // lblY
            //
            lblY.AutoSize = true;
            lblY.Location = new System.Drawing.Point(300, 65);
            lblY.Name = "lblY";
            lblY.Size = new System.Drawing.Size(18, 15);
            lblY.TabIndex = 5;
            lblY.Text = "m";
            //
            // lblX
            //
            lblX.AutoSize = true;
            lblX.Location = new System.Drawing.Point(300, 35);
            lblX.Name = "lblX";
            lblX.Size = new System.Drawing.Size(18, 15);
            lblX.TabIndex = 4;
            lblX.Text = "m";
            //
            // lblYCoord
            //
            lblYCoord.AutoSize = true;
            lblYCoord.Location = new System.Drawing.Point(15, 65);
            lblYCoord.Name = "lblYCoord";
            lblYCoord.Size = new System.Drawing.Size(79, 15);
            lblYCoord.TabIndex = 3;
            lblYCoord.Text = "Y Coordinate:";
            //
            // lblXCoord
            //
            lblXCoord.AutoSize = true;
            lblXCoord.Location = new System.Drawing.Point(15, 35);
            lblXCoord.Name = "lblXCoord";
            lblXCoord.Size = new System.Drawing.Size(79, 15);
            lblXCoord.TabIndex = 2;
            lblXCoord.Text = "X Coordinate:";
            //
            // numY
            //
            numY.DecimalPlaces = 2;
            numY.Location = new System.Drawing.Point(115, 63);
            numY.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numY.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numY.Name = "numY";
            numY.Size = new System.Drawing.Size(100, 23);
            numY.TabIndex = 1;
            //
            // numX
            //
            numX.DecimalPlaces = 2;
            numX.Location = new System.Drawing.Point(115, 33);
            numX.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numX.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numX.Name = "numX";
            numX.Size = new System.Drawing.Size(100, 23);
            numX.TabIndex = 0;
            //
            // grpProperties
            //
            grpProperties.Controls.Add(lblColumnLabel);
            grpProperties.Controls.Add(txtColumnLabel);
            grpProperties.Controls.Add(lblRotationDeg);
            grpProperties.Controls.Add(lblRotationAngle);
            grpProperties.Controls.Add(numRotationAngle);
            grpProperties.Controls.Add(lblSection);
            grpProperties.Controls.Add(cmbSection);
            grpProperties.Controls.Add(lblHeightM);
            grpProperties.Controls.Add(lblHeight);
            grpProperties.Controls.Add(numHeight);
            grpProperties.Location = new System.Drawing.Point(12, 188);
            grpProperties.Name = "grpProperties";
            grpProperties.Size = new System.Drawing.Size(360, 180);
            grpProperties.TabIndex = 2;
            grpProperties.TabStop = false;
            grpProperties.Text = "Column Properties";
            //
            // lblColumnLabel
            //
            lblColumnLabel.AutoSize = true;
            lblColumnLabel.Location = new System.Drawing.Point(15, 145);
            lblColumnLabel.Name = "lblColumnLabel";
            lblColumnLabel.Size = new System.Drawing.Size(87, 15);
            lblColumnLabel.TabIndex = 9;
            lblColumnLabel.Text = "Column Label:";
            //
            // txtColumnLabel
            //
            txtColumnLabel.Location = new System.Drawing.Point(115, 142);
            txtColumnLabel.Name = "txtColumnLabel";
            txtColumnLabel.Size = new System.Drawing.Size(100, 23);
            txtColumnLabel.TabIndex = 8;
            txtColumnLabel.Text = "C1";
            //
            // lblRotationDeg
            //
            lblRotationDeg.AutoSize = true;
            lblRotationDeg.Location = new System.Drawing.Point(300, 115);
            lblRotationDeg.Name = "lblRotationDeg";
            lblRotationDeg.Size = new System.Drawing.Size(45, 15);
            lblRotationDeg.TabIndex = 7;
            lblRotationDeg.Text = "degrees";
            //
            // lblRotationAngle
            //
            lblRotationAngle.AutoSize = true;
            lblRotationAngle.Location = new System.Drawing.Point(15, 115);
            lblRotationAngle.Name = "lblRotationAngle";
            lblRotationAngle.Size = new System.Drawing.Size(94, 15);
            lblRotationAngle.TabIndex = 6;
            lblRotationAngle.Text = "Rotation Angle:";
            //
            // numRotationAngle
            //
            numRotationAngle.DecimalPlaces = 1;
            numRotationAngle.Location = new System.Drawing.Point(115, 113);
            numRotationAngle.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            numRotationAngle.Minimum = new decimal(new int[] { 360, 0, 0, int.MinValue });
            numRotationAngle.Name = "numRotationAngle";
            numRotationAngle.Size = new System.Drawing.Size(100, 23);
            numRotationAngle.TabIndex = 5;
            //
            // lblSection
            //
            lblSection.AutoSize = true;
            lblSection.Location = new System.Drawing.Point(15, 85);
            lblSection.Name = "lblSection";
            lblSection.Size = new System.Drawing.Size(50, 15);
            lblSection.TabIndex = 4;
            lblSection.Text = "Section:";
            //
            // cmbSection
            //
            cmbSection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbSection.FormattingEnabled = true;
            cmbSection.Location = new System.Drawing.Point(115, 82);
            cmbSection.Name = "cmbSection";
            cmbSection.Size = new System.Drawing.Size(230, 23);
            cmbSection.TabIndex = 3;
            //
            // lblHeightM
            //
            lblHeightM.AutoSize = true;
            lblHeightM.Location = new System.Drawing.Point(300, 55);
            lblHeightM.Name = "lblHeightM";
            lblHeightM.Size = new System.Drawing.Size(18, 15);
            lblHeightM.TabIndex = 2;
            lblHeightM.Text = "m";
            //
            // lblHeight
            //
            lblHeight.AutoSize = true;
            lblHeight.Location = new System.Drawing.Point(15, 55);
            lblHeight.Name = "lblHeight";
            lblHeight.Size = new System.Drawing.Size(46, 15);
            lblHeight.TabIndex = 1;
            lblHeight.Text = "Height:";
            //
            // numHeight
            //
            numHeight.DecimalPlaces = 2;
            numHeight.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            numHeight.Location = new System.Drawing.Point(115, 53);
            numHeight.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numHeight.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numHeight.Name = "numHeight";
            numHeight.Size = new System.Drawing.Size(100, 23);
            numHeight.TabIndex = 0;
            numHeight.Value = new decimal(new int[] { 3, 0, 0, 0 });
            //
            // btnPlaceColumn
            //
            btnPlaceColumn.BackColor = System.Drawing.Color.LightGreen;
            btnPlaceColumn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            btnPlaceColumn.Location = new System.Drawing.Point(12, 384);
            btnPlaceColumn.Name = "btnPlaceColumn";
            btnPlaceColumn.Size = new System.Drawing.Size(250, 40);
            btnPlaceColumn.TabIndex = 3;
            btnPlaceColumn.Text = "Place Column";
            btnPlaceColumn.UseVisualStyleBackColor = false;
            btnPlaceColumn.Click += btnPlaceColumn_Click;
            //
            // btnLoadExample
            //
            btnLoadExample.Location = new System.Drawing.Point(268, 384);
            btnLoadExample.Name = "btnLoadExample";
            btnLoadExample.Size = new System.Drawing.Size(104, 40);
            btnLoadExample.TabIndex = 4;
            btnLoadExample.Text = "Load Example";
            btnLoadExample.UseVisualStyleBackColor = true;
            btnLoadExample.Click += btnLoadExample_Click;
            //
            // btnGenerateTemplate
            //
            btnGenerateTemplate.Location = new System.Drawing.Point(12, 430);
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
            btnImportCsv.Location = new System.Drawing.Point(193, 430);
            btnImportCsv.Name = "btnImportCsv";
            btnImportCsv.Size = new System.Drawing.Size(179, 35);
            btnImportCsv.TabIndex = 8;
            btnImportCsv.Text = "2. Import from CSV";
            btnImportCsv.UseVisualStyleBackColor = false;
            btnImportCsv.Click += btnImportCsv_Click;
            //
            // txtStatus
            //
            txtStatus.Location = new System.Drawing.Point(12, 491);
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
            lblStatus.Location = new System.Drawing.Point(12, 473);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(42, 15);
            lblStatus.TabIndex = 6;
            lblStatus.Text = "Status:";
            //
            // ColumnPlacementForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(384, 623);
            Controls.Add(lblStatus);
            Controls.Add(txtStatus);
            Controls.Add(btnImportCsv);
            Controls.Add(btnGenerateTemplate);
            Controls.Add(btnLoadExample);
            Controls.Add(btnPlaceColumn);
            Controls.Add(grpProperties);
            Controls.Add(grpCoordinates);
            Controls.Add(lblTitle);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "ColumnPlacementForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ETABS Column Placement";
            Load += ColumnPlacementForm_Load;
            grpCoordinates.ResumeLayout(false);
            grpCoordinates.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numBaseElevation).EndInit();
            ((System.ComponentModel.ISupportInitialize)numY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numX).EndInit();
            grpProperties.ResumeLayout(false);
            grpProperties.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numRotationAngle).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpCoordinates;
        private System.Windows.Forms.NumericUpDown numX;
        private System.Windows.Forms.Label lblYCoord;
        private System.Windows.Forms.Label lblXCoord;
        private System.Windows.Forms.NumericUpDown numY;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.Label lblBaseElevationM;
        private System.Windows.Forms.Label lblBaseElevation;
        private System.Windows.Forms.NumericUpDown numBaseElevation;
        private System.Windows.Forms.GroupBox grpProperties;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.Label lblSection;
        private System.Windows.Forms.ComboBox cmbSection;
        private System.Windows.Forms.Label lblHeightM;
        private System.Windows.Forms.Label lblRotationDeg;
        private System.Windows.Forms.Label lblRotationAngle;
        private System.Windows.Forms.NumericUpDown numRotationAngle;
        private System.Windows.Forms.Label lblColumnLabel;
        private System.Windows.Forms.TextBox txtColumnLabel;
        private System.Windows.Forms.Button btnPlaceColumn;
        private System.Windows.Forms.Button btnLoadExample;
        private System.Windows.Forms.Button btnGenerateTemplate;
        private System.Windows.Forms.Button btnImportCsv;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label lblStatus;
    }
}
