namespace ETABS_Plugin
{
    partial class DesignCheckForm
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
            grpDesignOptions = new System.Windows.Forms.GroupBox();
            scrollablePanel = new System.Windows.Forms.Panel();
            grpSteel = new System.Windows.Forms.GroupBox();
            btnExtractSteelResults = new System.Windows.Forms.Button();
            btnRunSteelDesign = new System.Windows.Forms.Button();
            grpConcrete = new System.Windows.Forms.GroupBox();
            btnExtractConcreteBeams = new System.Windows.Forms.Button();
            btnExtractConcreteColumns = new System.Windows.Forms.Button();
            btnRunConcreteDesign = new System.Windows.Forms.Button();
            grpComposite = new System.Windows.Forms.GroupBox();
            btnExtractCompositeBeams = new System.Windows.Forms.Button();
            btnRunCompositeDesign = new System.Windows.Forms.Button();
            grpSlab = new System.Windows.Forms.GroupBox();
            btnExtractSlabResults = new System.Windows.Forms.Button();
            btnRunSlabDesign = new System.Windows.Forms.Button();
            txtStatus = new System.Windows.Forms.TextBox();
            lblStatus = new System.Windows.Forms.Label();
            btnClearStatus = new System.Windows.Forms.Button();
            grpDesignOptions.SuspendLayout();
            scrollablePanel.SuspendLayout();
            grpSteel.SuspendLayout();
            grpConcrete.SuspendLayout();
            grpComposite.SuspendLayout();
            grpSlab.SuspendLayout();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblTitle.Location = new System.Drawing.Point(12, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(257, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Design Check / Code Verification";
            //
            // grpDesignOptions
            //
            grpDesignOptions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            grpDesignOptions.Controls.Add(scrollablePanel);
            grpDesignOptions.Location = new System.Drawing.Point(12, 42);
            grpDesignOptions.Name = "grpDesignOptions";
            grpDesignOptions.Size = new System.Drawing.Size(460, 520);
            grpDesignOptions.TabIndex = 1;
            grpDesignOptions.TabStop = false;
            grpDesignOptions.Text = "Design Check Options";
            //
            // scrollablePanel
            //
            scrollablePanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            scrollablePanel.AutoScroll = true;
            scrollablePanel.Controls.Add(grpSteel);
            scrollablePanel.Controls.Add(grpConcrete);
            scrollablePanel.Controls.Add(grpComposite);
            scrollablePanel.Controls.Add(grpSlab);
            scrollablePanel.Location = new System.Drawing.Point(6, 22);
            scrollablePanel.Name = "scrollablePanel";
            scrollablePanel.Size = new System.Drawing.Size(448, 492);
            scrollablePanel.TabIndex = 0;
            //
            // grpSteel
            //
            grpSteel.Controls.Add(btnExtractSteelResults);
            grpSteel.Controls.Add(btnRunSteelDesign);
            grpSteel.Location = new System.Drawing.Point(10, 10);
            grpSteel.Name = "grpSteel";
            grpSteel.Size = new System.Drawing.Size(415, 100);
            grpSteel.TabIndex = 0;
            grpSteel.TabStop = false;
            grpSteel.Text = "Steel Frame Design";
            //
            // btnExtractSteelResults
            //
            btnExtractSteelResults.BackColor = System.Drawing.Color.LightSkyBlue;
            btnExtractSteelResults.Location = new System.Drawing.Point(10, 60);
            btnExtractSteelResults.Name = "btnExtractSteelResults";
            btnExtractSteelResults.Size = new System.Drawing.Size(395, 30);
            btnExtractSteelResults.TabIndex = 1;
            btnExtractSteelResults.Text = "Extract Steel Results to CSV";
            btnExtractSteelResults.UseVisualStyleBackColor = false;
            btnExtractSteelResults.Click += btnExtractSteelResults_Click;
            //
            // btnRunSteelDesign
            //
            btnRunSteelDesign.BackColor = System.Drawing.Color.LimeGreen;
            btnRunSteelDesign.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            btnRunSteelDesign.Location = new System.Drawing.Point(10, 25);
            btnRunSteelDesign.Name = "btnRunSteelDesign";
            btnRunSteelDesign.Size = new System.Drawing.Size(395, 30);
            btnRunSteelDesign.TabIndex = 0;
            btnRunSteelDesign.Text = "▶ Run Steel Frame Design Check";
            btnRunSteelDesign.UseVisualStyleBackColor = false;
            btnRunSteelDesign.Click += btnRunSteelDesign_Click;
            //
            // grpConcrete
            //
            grpConcrete.Controls.Add(btnExtractConcreteBeams);
            grpConcrete.Controls.Add(btnExtractConcreteColumns);
            grpConcrete.Controls.Add(btnRunConcreteDesign);
            grpConcrete.Location = new System.Drawing.Point(10, 120);
            grpConcrete.Name = "grpConcrete";
            grpConcrete.Size = new System.Drawing.Size(415, 130);
            grpConcrete.TabIndex = 1;
            grpConcrete.TabStop = false;
            grpConcrete.Text = "Concrete Frame Design";
            //
            // btnExtractConcreteBeams
            //
            btnExtractConcreteBeams.BackColor = System.Drawing.Color.LightSkyBlue;
            btnExtractConcreteBeams.Location = new System.Drawing.Point(210, 60);
            btnExtractConcreteBeams.Name = "btnExtractConcreteBeams";
            btnExtractConcreteBeams.Size = new System.Drawing.Size(195, 30);
            btnExtractConcreteBeams.TabIndex = 2;
            btnExtractConcreteBeams.Text = "Extract Beam Results";
            btnExtractConcreteBeams.UseVisualStyleBackColor = false;
            btnExtractConcreteBeams.Click += btnExtractConcreteBeams_Click;
            //
            // btnExtractConcreteColumns
            //
            btnExtractConcreteColumns.BackColor = System.Drawing.Color.LightSkyBlue;
            btnExtractConcreteColumns.Location = new System.Drawing.Point(10, 60);
            btnExtractConcreteColumns.Name = "btnExtractConcreteColumns";
            btnExtractConcreteColumns.Size = new System.Drawing.Size(195, 30);
            btnExtractConcreteColumns.TabIndex = 1;
            btnExtractConcreteColumns.Text = "Extract Column Results";
            btnExtractConcreteColumns.UseVisualStyleBackColor = false;
            btnExtractConcreteColumns.Click += btnExtractConcreteColumns_Click;
            //
            // btnRunConcreteDesign
            //
            btnRunConcreteDesign.BackColor = System.Drawing.Color.LimeGreen;
            btnRunConcreteDesign.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            btnRunConcreteDesign.Location = new System.Drawing.Point(10, 25);
            btnRunConcreteDesign.Name = "btnRunConcreteDesign";
            btnRunConcreteDesign.Size = new System.Drawing.Size(395, 30);
            btnRunConcreteDesign.TabIndex = 0;
            btnRunConcreteDesign.Text = "▶ Run Concrete Frame Design Check";
            btnRunConcreteDesign.UseVisualStyleBackColor = false;
            btnRunConcreteDesign.Click += btnRunConcreteDesign_Click;
            //
            // grpComposite
            //
            grpComposite.Controls.Add(btnExtractCompositeBeams);
            grpComposite.Controls.Add(btnRunCompositeDesign);
            grpComposite.Location = new System.Drawing.Point(10, 260);
            grpComposite.Name = "grpComposite";
            grpComposite.Size = new System.Drawing.Size(415, 100);
            grpComposite.TabIndex = 2;
            grpComposite.TabStop = false;
            grpComposite.Text = "Composite Beam Design";
            //
            // btnExtractCompositeBeams
            //
            btnExtractCompositeBeams.BackColor = System.Drawing.Color.LightSkyBlue;
            btnExtractCompositeBeams.Location = new System.Drawing.Point(10, 60);
            btnExtractCompositeBeams.Name = "btnExtractCompositeBeams";
            btnExtractCompositeBeams.Size = new System.Drawing.Size(395, 30);
            btnExtractCompositeBeams.TabIndex = 1;
            btnExtractCompositeBeams.Text = "Extract Composite Beam Results";
            btnExtractCompositeBeams.UseVisualStyleBackColor = false;
            btnExtractCompositeBeams.Click += btnExtractCompositeBeams_Click;
            //
            // btnRunCompositeDesign
            //
            btnRunCompositeDesign.BackColor = System.Drawing.Color.LimeGreen;
            btnRunCompositeDesign.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            btnRunCompositeDesign.Location = new System.Drawing.Point(10, 25);
            btnRunCompositeDesign.Name = "btnRunCompositeDesign";
            btnRunCompositeDesign.Size = new System.Drawing.Size(395, 30);
            btnRunCompositeDesign.TabIndex = 0;
            btnRunCompositeDesign.Text = "▶ Run Composite Beam Design Check";
            btnRunCompositeDesign.UseVisualStyleBackColor = false;
            btnRunCompositeDesign.Click += btnRunCompositeDesign_Click;
            //
            // grpSlab
            //
            grpSlab.Controls.Add(btnExtractSlabResults);
            grpSlab.Controls.Add(btnRunSlabDesign);
            grpSlab.Location = new System.Drawing.Point(10, 370);
            grpSlab.Name = "grpSlab";
            grpSlab.Size = new System.Drawing.Size(415, 100);
            grpSlab.TabIndex = 3;
            grpSlab.TabStop = false;
            grpSlab.Text = "Slab Design";
            //
            // btnExtractSlabResults
            //
            btnExtractSlabResults.BackColor = System.Drawing.Color.LightSkyBlue;
            btnExtractSlabResults.Location = new System.Drawing.Point(10, 60);
            btnExtractSlabResults.Name = "btnExtractSlabResults";
            btnExtractSlabResults.Size = new System.Drawing.Size(395, 30);
            btnExtractSlabResults.TabIndex = 1;
            btnExtractSlabResults.Text = "Extract Slab Results to CSV";
            btnExtractSlabResults.UseVisualStyleBackColor = false;
            btnExtractSlabResults.Click += btnExtractSlabResults_Click;
            //
            // btnRunSlabDesign
            //
            btnRunSlabDesign.BackColor = System.Drawing.Color.LimeGreen;
            btnRunSlabDesign.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            btnRunSlabDesign.Location = new System.Drawing.Point(10, 25);
            btnRunSlabDesign.Name = "btnRunSlabDesign";
            btnRunSlabDesign.Size = new System.Drawing.Size(395, 30);
            btnRunSlabDesign.TabIndex = 0;
            btnRunSlabDesign.Text = "▶ Run Slab Design Check";
            btnRunSlabDesign.UseVisualStyleBackColor = false;
            btnRunSlabDesign.Click += btnRunSlabDesign_Click;
            //
            // txtStatus
            //
            txtStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtStatus.BackColor = System.Drawing.Color.Black;
            txtStatus.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txtStatus.ForeColor = System.Drawing.Color.Lime;
            txtStatus.Location = new System.Drawing.Point(478, 67);
            txtStatus.Multiline = true;
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtStatus.Size = new System.Drawing.Size(494, 470);
            txtStatus.TabIndex = 2;
            txtStatus.WordWrap = false;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblStatus.Location = new System.Drawing.Point(478, 42);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(53, 15);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Status:";
            //
            // btnClearStatus
            //
            btnClearStatus.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnClearStatus.Location = new System.Drawing.Point(867, 543);
            btnClearStatus.Name = "btnClearStatus";
            btnClearStatus.Size = new System.Drawing.Size(105, 25);
            btnClearStatus.TabIndex = 4;
            btnClearStatus.Text = "Clear Status";
            btnClearStatus.UseVisualStyleBackColor = true;
            btnClearStatus.Click += btnClearStatus_Click;
            //
            // DesignCheckForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(984, 574);
            Controls.Add(btnClearStatus);
            Controls.Add(lblStatus);
            Controls.Add(txtStatus);
            Controls.Add(grpDesignOptions);
            Controls.Add(lblTitle);
            MinimumSize = new System.Drawing.Size(1000, 600);
            Name = "DesignCheckForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ETABS Design Check";
            Load += DesignCheckForm_Load;
            grpDesignOptions.ResumeLayout(false);
            scrollablePanel.ResumeLayout(false);
            grpSteel.ResumeLayout(false);
            grpConcrete.ResumeLayout(false);
            grpComposite.ResumeLayout(false);
            grpSlab.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpDesignOptions;
        private System.Windows.Forms.Panel scrollablePanel;
        private System.Windows.Forms.GroupBox grpSteel;
        private System.Windows.Forms.Button btnExtractSteelResults;
        private System.Windows.Forms.Button btnRunSteelDesign;
        private System.Windows.Forms.GroupBox grpConcrete;
        private System.Windows.Forms.Button btnExtractConcreteBeams;
        private System.Windows.Forms.Button btnExtractConcreteColumns;
        private System.Windows.Forms.Button btnRunConcreteDesign;
        private System.Windows.Forms.GroupBox grpComposite;
        private System.Windows.Forms.Button btnExtractCompositeBeams;
        private System.Windows.Forms.Button btnRunCompositeDesign;
        private System.Windows.Forms.GroupBox grpSlab;
        private System.Windows.Forms.Button btnExtractSlabResults;
        private System.Windows.Forms.Button btnRunSlabDesign;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClearStatus;
    }
}
