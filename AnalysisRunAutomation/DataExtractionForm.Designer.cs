namespace ETABS_Plugin
{
    partial class DataExtractionForm
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
            grpExtractionOptions = new System.Windows.Forms.GroupBox();
            btnRunDiagnostics = new System.Windows.Forms.Button();
            btnExtractBaseReactions = new System.Windows.Forms.Button();
            btnExtractProjectInfo = new System.Windows.Forms.Button();
            btnExtractStoryInfo = new System.Windows.Forms.Button();
            btnExtractGridInfo = new System.Windows.Forms.Button();
            btnExtractFrameModifiers = new System.Windows.Forms.Button();
            btnExtractAreaModifiers = new System.Windows.Forms.Button();
            btnExtractWallElements = new System.Windows.Forms.Button();
            btnExtractColumnElements = new System.Windows.Forms.Button();
            btnExtractModalPeriods = new System.Windows.Forms.Button();
            btnExtractModalMassRatios = new System.Windows.Forms.Button();
            btnExtractStoryDrifts = new System.Windows.Forms.Button();
            btnExtractBaseShear = new System.Windows.Forms.Button();
            btnExtractCompositeColumnDesign = new System.Windows.Forms.Button();
            btnExtractQuantitiesSummary = new System.Windows.Forms.Button();
            lblPlaceholder = new System.Windows.Forms.Label();
            txtStatus = new System.Windows.Forms.TextBox();
            lblStatus = new System.Windows.Forms.Label();
            btnClearStatus = new System.Windows.Forms.Button();
            grpExtractionOptions.SuspendLayout();
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
            lblTitle.Text = "Data Extraction";
            //
            // grpExtractionOptions
            //
            grpExtractionOptions.Controls.Add(btnRunDiagnostics);
            grpExtractionOptions.Controls.Add(btnExtractAll);
            grpExtractionOptions.Controls.Add(btnExtractBaseReactions);
            grpExtractionOptions.Controls.Add(btnExtractProjectInfo);
            grpExtractionOptions.Controls.Add(btnExtractStoryInfo);
            grpExtractionOptions.Controls.Add(btnExtractGridInfo);
            grpExtractionOptions.Controls.Add(btnExtractFrameModifiers);
            grpExtractionOptions.Controls.Add(btnExtractAreaModifiers);
            grpExtractionOptions.Controls.Add(btnExtractWallElements);
            grpExtractionOptions.Controls.Add(btnExtractColumnElements);
            grpExtractionOptions.Controls.Add(btnExtractModalPeriods);
            grpExtractionOptions.Controls.Add(btnExtractModalMassRatios);
            grpExtractionOptions.Controls.Add(btnExtractStoryDrifts);
            grpExtractionOptions.Controls.Add(btnExtractBaseShear);
            grpExtractionOptions.Controls.Add(btnExtractCompositeColumnDesign);
            grpExtractionOptions.Controls.Add(btnExtractQuantitiesSummary);
            grpExtractionOptions.Controls.Add(lblPlaceholder);
            grpExtractionOptions.Location = new System.Drawing.Point(12, 42);
            grpExtractionOptions.Name = "grpExtractionOptions";
            grpExtractionOptions.Size = new System.Drawing.Size(460, 820);
            grpExtractionOptions.TabIndex = 1;
            grpExtractionOptions.TabStop = false;
            grpExtractionOptions.Text = "Extraction Options";
            //
            // btnRunDiagnostics
            //
            btnRunDiagnostics.BackColor = System.Drawing.Color.Orange;
            btnRunDiagnostics.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            btnRunDiagnostics.Location = new System.Drawing.Point(15, 30);
            btnRunDiagnostics.Name = "btnRunDiagnostics";
            btnRunDiagnostics.Size = new System.Drawing.Size(430, 40);
            btnRunDiagnostics.TabIndex = 0;
            btnRunDiagnostics.Text = "🔍 Run Model Diagnostics (Check What's Available)";
            btnRunDiagnostics.UseVisualStyleBackColor = false;
            btnRunDiagnostics.Click += btnRunDiagnostics_Click;
            //
            // btnExtractAll
            //
            btnExtractAll.BackColor = System.Drawing.Color.LimeGreen;
            btnExtractAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractAll.Location = new System.Drawing.Point(15, 80);
            btnExtractAll.Name = "btnExtractAll";
            btnExtractAll.Size = new System.Drawing.Size(430, 40);
            btnExtractAll.TabIndex = 1;
            btnExtractAll.Text = "⚡ EXTRACT ALL DATA (Save All to Folder)";
            btnExtractAll.UseVisualStyleBackColor = false;
            btnExtractAll.Click += btnExtractAll_Click;
            //
            // btnExtractBaseReactions
            //
            btnExtractBaseReactions.BackColor = System.Drawing.Color.LightSkyBlue;
            btnExtractBaseReactions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractBaseReactions.Location = new System.Drawing.Point(15, 130);
            btnExtractBaseReactions.Name = "btnExtractBaseReactions";
            btnExtractBaseReactions.Size = new System.Drawing.Size(430, 40);
            btnExtractBaseReactions.TabIndex = 0;
            btnExtractBaseReactions.Text = "Extract Base Reactions (All Load Cases)";
            btnExtractBaseReactions.UseVisualStyleBackColor = false;
            btnExtractBaseReactions.Click += btnExtractBaseReactions_Click;
            //
            // btnExtractProjectInfo
            //
            btnExtractProjectInfo.BackColor = System.Drawing.Color.LightCyan;
            btnExtractProjectInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractProjectInfo.Location = new System.Drawing.Point(15, 180);
            btnExtractProjectInfo.Name = "btnExtractProjectInfo";
            btnExtractProjectInfo.Size = new System.Drawing.Size(430, 40);
            btnExtractProjectInfo.TabIndex = 2;
            btnExtractProjectInfo.Text = "Extract Project/Model Information";
            btnExtractProjectInfo.UseVisualStyleBackColor = false;
            btnExtractProjectInfo.Click += btnExtractProjectInfo_Click;
            //
            // btnExtractStoryInfo
            //
            btnExtractStoryInfo.BackColor = System.Drawing.Color.LightYellow;
            btnExtractStoryInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractStoryInfo.Location = new System.Drawing.Point(15, 230);
            btnExtractStoryInfo.Name = "btnExtractStoryInfo";
            btnExtractStoryInfo.Size = new System.Drawing.Size(430, 40);
            btnExtractStoryInfo.TabIndex = 4;
            btnExtractStoryInfo.Text = "Extract Story/Level Information";
            btnExtractStoryInfo.UseVisualStyleBackColor = false;
            btnExtractStoryInfo.Click += btnExtractStoryInfo_Click;
            //
            // btnExtractGridInfo
            //
            btnExtractGridInfo.BackColor = System.Drawing.Color.LightGreen;
            btnExtractGridInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractGridInfo.Location = new System.Drawing.Point(15, 280);
            btnExtractGridInfo.Name = "btnExtractGridInfo";
            btnExtractGridInfo.Size = new System.Drawing.Size(430, 40);
            btnExtractGridInfo.TabIndex = 6;
            btnExtractGridInfo.Text = "Extract Grid System Information";
            btnExtractGridInfo.UseVisualStyleBackColor = false;
            btnExtractGridInfo.Click += btnExtractGridInfo_Click;
            //
            // btnExtractFrameModifiers
            //
            btnExtractFrameModifiers.BackColor = System.Drawing.Color.LightSalmon;
            btnExtractFrameModifiers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractFrameModifiers.Location = new System.Drawing.Point(15, 330);
            btnExtractFrameModifiers.Name = "btnExtractFrameModifiers";
            btnExtractFrameModifiers.Size = new System.Drawing.Size(430, 40);
            btnExtractFrameModifiers.TabIndex = 8;
            btnExtractFrameModifiers.Text = "Extract Frame Property Modifiers";
            btnExtractFrameModifiers.UseVisualStyleBackColor = false;
            btnExtractFrameModifiers.Click += btnExtractFrameModifiers_Click;
            //
            // btnExtractAreaModifiers
            //
            btnExtractAreaModifiers.BackColor = System.Drawing.Color.LightPink;
            btnExtractAreaModifiers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractAreaModifiers.Location = new System.Drawing.Point(15, 380);
            btnExtractAreaModifiers.Name = "btnExtractAreaModifiers";
            btnExtractAreaModifiers.Size = new System.Drawing.Size(430, 40);
            btnExtractAreaModifiers.TabIndex = 9;
            btnExtractAreaModifiers.Text = "Extract Area Property Modifiers";
            btnExtractAreaModifiers.UseVisualStyleBackColor = false;
            btnExtractAreaModifiers.Click += btnExtractAreaModifiers_Click;
            //
            // btnExtractWallElements
            //
            btnExtractWallElements.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            btnExtractWallElements.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractWallElements.Location = new System.Drawing.Point(15, 430);
            btnExtractWallElements.Name = "btnExtractWallElements";
            btnExtractWallElements.Size = new System.Drawing.Size(430, 40);
            btnExtractWallElements.TabIndex = 11;
            btnExtractWallElements.Text = "Extract Wall Elements (Geometry + Location)";
            btnExtractWallElements.UseVisualStyleBackColor = false;
            btnExtractWallElements.Click += btnExtractWallElements_Click;
            //
            // btnExtractColumnElements
            //
            btnExtractColumnElements.BackColor = System.Drawing.Color.LightSteelBlue;
            btnExtractColumnElements.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractColumnElements.Location = new System.Drawing.Point(15, 480);
            btnExtractColumnElements.Name = "btnExtractColumnElements";
            btnExtractColumnElements.Size = new System.Drawing.Size(430, 40);
            btnExtractColumnElements.TabIndex = 12;
            btnExtractColumnElements.Text = "Extract Column Elements (Geometry + Location)";
            btnExtractColumnElements.UseVisualStyleBackColor = false;
            btnExtractColumnElements.Click += btnExtractColumnElements_Click;
            //
            // btnExtractModalPeriods
            //
            btnExtractModalPeriods.BackColor = System.Drawing.Color.LightCyan;
            btnExtractModalPeriods.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractModalPeriods.Location = new System.Drawing.Point(15, 530);
            btnExtractModalPeriods.Name = "btnExtractModalPeriods";
            btnExtractModalPeriods.Size = new System.Drawing.Size(430, 40);
            btnExtractModalPeriods.TabIndex = 13;
            btnExtractModalPeriods.Text = "Extract Modal Periods";
            btnExtractModalPeriods.UseVisualStyleBackColor = false;
            btnExtractModalPeriods.Click += btnExtractModalPeriods_Click;
            //
            // btnExtractModalMassRatios
            //
            btnExtractModalMassRatios.BackColor = System.Drawing.Color.LightBlue;
            btnExtractModalMassRatios.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractModalMassRatios.Location = new System.Drawing.Point(15, 580);
            btnExtractModalMassRatios.Name = "btnExtractModalMassRatios";
            btnExtractModalMassRatios.Size = new System.Drawing.Size(430, 40);
            btnExtractModalMassRatios.TabIndex = 14;
            btnExtractModalMassRatios.Text = "Extract Modal Participating Mass Ratios";
            btnExtractModalMassRatios.UseVisualStyleBackColor = false;
            btnExtractModalMassRatios.Click += btnExtractModalMassRatios_Click;
            //
            // btnExtractStoryDrifts
            //
            btnExtractStoryDrifts.BackColor = System.Drawing.Color.LightPink;
            btnExtractStoryDrifts.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractStoryDrifts.Location = new System.Drawing.Point(15, 630);
            btnExtractStoryDrifts.Name = "btnExtractStoryDrifts";
            btnExtractStoryDrifts.Size = new System.Drawing.Size(430, 40);
            btnExtractStoryDrifts.TabIndex = 15;
            btnExtractStoryDrifts.Text = "Extract Story Drifts";
            btnExtractStoryDrifts.UseVisualStyleBackColor = false;
            btnExtractStoryDrifts.Click += btnExtractStoryDrifts_Click;
            //
            // btnExtractBaseShear
            //
            btnExtractBaseShear.BackColor = System.Drawing.Color.PaleGreen;
            btnExtractBaseShear.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractBaseShear.Location = new System.Drawing.Point(15, 680);
            btnExtractBaseShear.Name = "btnExtractBaseShear";
            btnExtractBaseShear.Size = new System.Drawing.Size(430, 40);
            btnExtractBaseShear.TabIndex = 16;
            btnExtractBaseShear.Text = "Extract Base Shear (Static & Response Spectrum)";
            btnExtractBaseShear.UseVisualStyleBackColor = false;
            btnExtractBaseShear.Click += btnExtractBaseShear_Click;
            //
            // btnExtractCompositeColumnDesign
            //
            btnExtractCompositeColumnDesign.BackColor = System.Drawing.Color.LightYellow;
            btnExtractCompositeColumnDesign.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractCompositeColumnDesign.Location = new System.Drawing.Point(15, 730);
            btnExtractCompositeColumnDesign.Name = "btnExtractCompositeColumnDesign";
            btnExtractCompositeColumnDesign.Size = new System.Drawing.Size(430, 40);
            btnExtractCompositeColumnDesign.TabIndex = 17;
            btnExtractCompositeColumnDesign.Text = "Extract Composite Column Design (DCR & PMM Ratios)";
            btnExtractCompositeColumnDesign.UseVisualStyleBackColor = false;
            btnExtractCompositeColumnDesign.Click += btnExtractCompositeColumnDesign_Click;
            //
            // btnExtractQuantitiesSummary
            //
            btnExtractQuantitiesSummary.BackColor = System.Drawing.Color.LightSalmon;
            btnExtractQuantitiesSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractQuantitiesSummary.Location = new System.Drawing.Point(15, 780);
            btnExtractQuantitiesSummary.Name = "btnExtractQuantitiesSummary";
            btnExtractQuantitiesSummary.Size = new System.Drawing.Size(430, 40);
            btnExtractQuantitiesSummary.TabIndex = 18;
            btnExtractQuantitiesSummary.Text = "Extract Quantities Summary (Materials)";
            btnExtractQuantitiesSummary.UseVisualStyleBackColor = false;
            btnExtractQuantitiesSummary.Click += btnExtractQuantitiesSummary_Click;
            //
            // lblPlaceholder
            //
            lblPlaceholder.AutoSize = true;
            lblPlaceholder.ForeColor = System.Drawing.Color.Gray;
            lblPlaceholder.Location = new System.Drawing.Point(15, 285);
            lblPlaceholder.Name = "lblPlaceholder";
            lblPlaceholder.Size = new System.Drawing.Size(0, 15);
            lblPlaceholder.TabIndex = 13;
            lblPlaceholder.Visible = false;
            //
            // txtStatus
            //
            txtStatus.Location = new System.Drawing.Point(12, 893);
            txtStatus.Multiline = true;
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtStatus.Size = new System.Drawing.Size(460, 200);
            txtStatus.TabIndex = 2;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Location = new System.Drawing.Point(12, 875);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(42, 15);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Status:";
            //
            // btnClearStatus
            //
            btnClearStatus.Location = new System.Drawing.Point(390, 870);
            btnClearStatus.Name = "btnClearStatus";
            btnClearStatus.Size = new System.Drawing.Size(82, 23);
            btnClearStatus.TabIndex = 4;
            btnClearStatus.Text = "Clear";
            btnClearStatus.UseVisualStyleBackColor = true;
            btnClearStatus.Click += btnClearStatus_Click;
            //
            // DataExtractionForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(484, 1105);
            Controls.Add(btnClearStatus);
            Controls.Add(lblStatus);
            Controls.Add(txtStatus);
            Controls.Add(grpExtractionOptions);
            Controls.Add(lblTitle);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "DataExtractionForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ETABS Data Extraction";
            Load += DataExtractionForm_Load;
            grpExtractionOptions.ResumeLayout(false);
            grpExtractionOptions.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpExtractionOptions;
        private System.Windows.Forms.Button btnRunDiagnostics;
        private System.Windows.Forms.Button btnExtractAll;
        private System.Windows.Forms.Button btnExtractBaseReactions;
        private System.Windows.Forms.Button btnExtractProjectInfo;
        private System.Windows.Forms.Button btnExtractStoryInfo;
        private System.Windows.Forms.Button btnExtractGridInfo;
        private System.Windows.Forms.Button btnExtractFrameModifiers;
        private System.Windows.Forms.Button btnExtractAreaModifiers;
        private System.Windows.Forms.Button btnExtractWallElements;
        private System.Windows.Forms.Button btnExtractColumnElements;
        private System.Windows.Forms.Button btnExtractModalPeriods;
        private System.Windows.Forms.Button btnExtractModalMassRatios;
        private System.Windows.Forms.Button btnExtractStoryDrifts;
        private System.Windows.Forms.Button btnExtractBaseShear;
        private System.Windows.Forms.Button btnExtractCompositeColumnDesign;
        private System.Windows.Forms.Button btnExtractQuantitiesSummary;
        private System.Windows.Forms.Label lblPlaceholder;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClearStatus;
    }
}
