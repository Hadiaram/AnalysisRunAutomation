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
            btnExtractBaseReactions = new System.Windows.Forms.Button();
            btnExtractProjectInfo = new System.Windows.Forms.Button();
            btnExtractStoryInfo = new System.Windows.Forms.Button();
            btnExtractGridInfo = new System.Windows.Forms.Button();
            btnExtractFrameModifiers = new System.Windows.Forms.Button();
            btnExtractAreaModifiers = new System.Windows.Forms.Button();
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
            grpExtractionOptions.Controls.Add(btnExtractBaseReactions);
            grpExtractionOptions.Controls.Add(btnExtractProjectInfo);
            grpExtractionOptions.Controls.Add(btnExtractStoryInfo);
            grpExtractionOptions.Controls.Add(btnExtractGridInfo);
            grpExtractionOptions.Controls.Add(btnExtractFrameModifiers);
            grpExtractionOptions.Controls.Add(btnExtractAreaModifiers);
            grpExtractionOptions.Controls.Add(lblPlaceholder);
            grpExtractionOptions.Location = new System.Drawing.Point(12, 42);
            grpExtractionOptions.Name = "grpExtractionOptions";
            grpExtractionOptions.Size = new System.Drawing.Size(460, 320);
            grpExtractionOptions.TabIndex = 1;
            grpExtractionOptions.TabStop = false;
            grpExtractionOptions.Text = "Extraction Options";
            //
            // btnExtractBaseReactions
            //
            btnExtractBaseReactions.BackColor = System.Drawing.Color.LightSkyBlue;
            btnExtractBaseReactions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnExtractBaseReactions.Location = new System.Drawing.Point(15, 30);
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
            btnExtractProjectInfo.Location = new System.Drawing.Point(15, 80);
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
            btnExtractStoryInfo.Location = new System.Drawing.Point(15, 130);
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
            btnExtractGridInfo.Location = new System.Drawing.Point(15, 180);
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
            btnExtractFrameModifiers.Location = new System.Drawing.Point(15, 230);
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
            btnExtractAreaModifiers.Location = new System.Drawing.Point(15, 280);
            btnExtractAreaModifiers.Name = "btnExtractAreaModifiers";
            btnExtractAreaModifiers.Size = new System.Drawing.Size(430, 40);
            btnExtractAreaModifiers.TabIndex = 9;
            btnExtractAreaModifiers.Text = "Extract Area Property Modifiers";
            btnExtractAreaModifiers.UseVisualStyleBackColor = false;
            btnExtractAreaModifiers.Click += btnExtractAreaModifiers_Click;
            //
            // lblPlaceholder
            //
            lblPlaceholder.AutoSize = true;
            lblPlaceholder.ForeColor = System.Drawing.Color.Gray;
            lblPlaceholder.Location = new System.Drawing.Point(15, 285);
            lblPlaceholder.Name = "lblPlaceholder";
            lblPlaceholder.Size = new System.Drawing.Size(0, 15);
            lblPlaceholder.TabIndex = 10;
            lblPlaceholder.Visible = false;
            //
            // txtStatus
            //
            txtStatus.Location = new System.Drawing.Point(12, 393);
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
            lblStatus.Location = new System.Drawing.Point(12, 375);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(42, 15);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Status:";
            //
            // btnClearStatus
            //
            btnClearStatus.Location = new System.Drawing.Point(390, 370);
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
            ClientSize = new System.Drawing.Size(484, 605);
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
        private System.Windows.Forms.Button btnExtractBaseReactions;
        private System.Windows.Forms.Button btnExtractProjectInfo;
        private System.Windows.Forms.Button btnExtractStoryInfo;
        private System.Windows.Forms.Button btnExtractGridInfo;
        private System.Windows.Forms.Button btnExtractFrameModifiers;
        private System.Windows.Forms.Button btnExtractAreaModifiers;
        private System.Windows.Forms.Label lblPlaceholder;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClearStatus;
    }
}
