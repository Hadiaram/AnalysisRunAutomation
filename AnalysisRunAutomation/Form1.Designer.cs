namespace ETABS_Plugin
{
    partial class Form1
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
            lblTitle = new Label();
            grpMaterials = new GroupBox();
            lblMPa = new Label();
            numConcreteStrength = new NumericUpDown();
            lblConcreteStrength = new Label();
            grpSections = new GroupBox();
            chkWalls = new CheckBox();
            chkSlabs = new CheckBox();
            chkBeams = new CheckBox();
            chkColumns = new CheckBox();
            btnCreateSections = new Button();
            btnCreateLoads = new Button();
            btnCreateBoundary = new Button();
            btnCreateDiaphragms = new Button();
            btnApplyMesh = new Button();
            btnAssignSections = new Button();
            btnAssignLoads = new Button();
            btnSetupMass = new Button();
            btnRunAnalysis = new Button();
            btnRunWorkflow = new Button();
            txtStatus = new TextBox();
            lblStatus = new Label();
            grpMaterials.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numConcreteStrength).BeginInit();
            grpSections.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(12, 11);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(294, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "ETABS Workflow Automation";
            // 
            // grpMaterials
            // 
            grpMaterials.Controls.Add(lblMPa);
            grpMaterials.Controls.Add(numConcreteStrength);
            grpMaterials.Controls.Add(lblConcreteStrength);
            grpMaterials.Location = new Point(17, 62);
            grpMaterials.Margin = new Padding(3, 4, 3, 4);
            grpMaterials.Name = "grpMaterials";
            grpMaterials.Padding = new Padding(3, 4, 3, 4);
            grpMaterials.Size = new Size(300, 100);
            grpMaterials.TabIndex = 1;
            grpMaterials.TabStop = false;
            grpMaterials.Text = "Material Properties";
            // 
            // lblMPa
            // 
            lblMPa.AutoSize = true;
            lblMPa.Location = new Point(240, 44);
            lblMPa.Name = "lblMPa";
            lblMPa.Size = new Size(37, 20);
            lblMPa.TabIndex = 2;
            lblMPa.Text = "MPa";
            // 
            // numConcreteStrength
            // 
            numConcreteStrength.Location = new Point(150, 41);
            numConcreteStrength.Margin = new Padding(3, 4, 3, 4);
            numConcreteStrength.Minimum = new decimal(new int[] { 15, 0, 0, 0 });
            numConcreteStrength.Name = "numConcreteStrength";
            numConcreteStrength.Size = new Size(80, 27);
            numConcreteStrength.TabIndex = 1;
            numConcreteStrength.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // lblConcreteStrength
            // 
            lblConcreteStrength.AutoSize = true;
            lblConcreteStrength.Location = new Point(15, 44);
            lblConcreteStrength.Name = "lblConcreteStrength";
            lblConcreteStrength.Size = new Size(131, 20);
            lblConcreteStrength.TabIndex = 0;
            lblConcreteStrength.Text = "Concrete Strength:";
            // 
            // grpSections
            // 
            grpSections.Controls.Add(chkWalls);
            grpSections.Controls.Add(chkSlabs);
            grpSections.Controls.Add(chkBeams);
            grpSections.Controls.Add(chkColumns);
            grpSections.Controls.Add(btnCreateSections);
            grpSections.Controls.Add(btnCreateLoads);
            grpSections.Controls.Add(btnCreateBoundary);
            grpSections.Controls.Add(btnCreateDiaphragms);
            grpSections.Controls.Add(btnApplyMesh);
            grpSections.Controls.Add(btnAssignSections);
            grpSections.Controls.Add(btnAssignLoads);
            grpSections.Controls.Add(btnSetupMass);
            grpSections.Controls.Add(btnRunAnalysis);
            grpSections.Location = new Point(17, 188);
            grpSections.Margin = new Padding(3, 4, 3, 4);
            grpSections.Name = "grpSections";
            grpSections.Padding = new Padding(3, 4, 3, 4);
            grpSections.Size = new Size(300, 410);
            grpSections.TabIndex = 2;
            grpSections.TabStop = false;
            grpSections.Text = "Workflow Steps";
            // 
            // chkWalls
            // 
            chkWalls.AutoSize = true;
            chkWalls.Checked = true;
            chkWalls.CheckState = CheckState.Checked;
            chkWalls.Location = new Point(110, 60);
            chkWalls.Margin = new Padding(3, 4, 3, 4);
            chkWalls.Name = "chkWalls";
            chkWalls.Size = new Size(66, 24);
            chkWalls.TabIndex = 4;
            chkWalls.Text = "Walls";
            chkWalls.UseVisualStyleBackColor = true;
            // 
            // chkSlabs
            // 
            chkSlabs.AutoSize = true;
            chkSlabs.Checked = true;
            chkSlabs.CheckState = CheckState.Checked;
            chkSlabs.Location = new Point(18, 60);
            chkSlabs.Margin = new Padding(3, 4, 3, 4);
            chkSlabs.Name = "chkSlabs";
            chkSlabs.Size = new Size(66, 24);
            chkSlabs.TabIndex = 3;
            chkSlabs.Text = "Slabs";
            chkSlabs.UseVisualStyleBackColor = true;
            // 
            // chkBeams
            // 
            chkBeams.AutoSize = true;
            chkBeams.Checked = true;
            chkBeams.CheckState = CheckState.Checked;
            chkBeams.Location = new Point(110, 30);
            chkBeams.Margin = new Padding(3, 4, 3, 4);
            chkBeams.Name = "chkBeams";
            chkBeams.Size = new Size(75, 24);
            chkBeams.TabIndex = 2;
            chkBeams.Text = "Beams";
            chkBeams.UseVisualStyleBackColor = true;
            // 
            // chkColumns
            // 
            chkColumns.AutoSize = true;
            chkColumns.Checked = true;
            chkColumns.CheckState = CheckState.Checked;
            chkColumns.Location = new Point(18, 30);
            chkColumns.Margin = new Padding(3, 4, 3, 4);
            chkColumns.Name = "chkColumns";
            chkColumns.Size = new Size(88, 24);
            chkColumns.TabIndex = 1;
            chkColumns.Text = "Columns";
            chkColumns.UseVisualStyleBackColor = true;
            // 
            // btnCreateSections
            // 
            btnCreateSections.Location = new Point(18, 100);
            btnCreateSections.Margin = new Padding(3, 4, 3, 4);
            btnCreateSections.Name = "btnCreateSections";
            btnCreateSections.Size = new Size(120, 38);
            btnCreateSections.TabIndex = 5;
            btnCreateSections.Text = "Create Sections";
            btnCreateSections.UseVisualStyleBackColor = true;
            btnCreateSections.Click += btnCreateSections_Click;
            // 
            // btnCreateLoads
            // 
            btnCreateLoads.Location = new Point(148, 100);
            btnCreateLoads.Margin = new Padding(3, 4, 3, 4);
            btnCreateLoads.Name = "btnCreateLoads";
            btnCreateLoads.Size = new Size(120, 38);
            btnCreateLoads.TabIndex = 6;
            btnCreateLoads.Text = "Create Loads";
            btnCreateLoads.UseVisualStyleBackColor = true;
            btnCreateLoads.Click += btnCreateLoads_Click;
            // 
            // btnCreateBoundary
            // 
            btnCreateBoundary.Location = new Point(148, 145);
            btnCreateBoundary.Margin = new Padding(3, 4, 3, 4);
            btnCreateBoundary.Name = "btnCreateBoundary";
            btnCreateBoundary.Size = new Size(120, 38);
            btnCreateBoundary.TabIndex = 8;
            btnCreateBoundary.Text = "Apply Fixed BC";
            btnCreateBoundary.UseVisualStyleBackColor = true;
            btnCreateBoundary.Click += btnApplyBCs_Click;
            // 
            // btnCreateDiaphragms
            // 
            btnCreateDiaphragms.Location = new Point(18, 190);
            btnCreateDiaphragms.Margin = new Padding(3, 4, 3, 4);
            btnCreateDiaphragms.Name = "btnCreateDiaphragms";
            btnCreateDiaphragms.Size = new Size(250, 38);
            btnCreateDiaphragms.TabIndex = 9;
            btnCreateDiaphragms.Text = "Create Diaphragms";
            btnCreateDiaphragms.UseVisualStyleBackColor = true;
            btnCreateDiaphragms.Click += btnCreateDiaphragms_Click;
            // 
            // btnApplyMesh
            // 
            btnApplyMesh.Location = new Point(18, 145);
            btnApplyMesh.Margin = new Padding(3, 4, 3, 4);
            btnApplyMesh.Name = "btnApplyMesh";
            btnApplyMesh.Size = new Size(120, 38);
            btnApplyMesh.TabIndex = 7;
            btnApplyMesh.Text = "Apply 1m Mesh";
            btnApplyMesh.UseVisualStyleBackColor = true;
            btnApplyMesh.Click += btnApplyMesh_Click;
            // 
            // btnAssignSections
            // 
            btnAssignSections.Location = new Point(18, 235);
            btnAssignSections.Margin = new Padding(3, 4, 3, 4);
            btnAssignSections.Name = "btnAssignSections";
            btnAssignSections.Size = new Size(250, 38);
            btnAssignSections.TabIndex = 10;
            btnAssignSections.Text = "Assign Sections";
            btnAssignSections.UseVisualStyleBackColor = true;
            btnAssignSections.Click += btnAssignSections_Click;
            // 
            // btnAssignLoads
            // 
            btnAssignLoads.Location = new Point(18, 280);
            btnAssignLoads.Margin = new Padding(3, 4, 3, 4);
            btnAssignLoads.Name = "btnAssignLoads";
            btnAssignLoads.Size = new Size(120, 38);
            btnAssignLoads.TabIndex = 11;
            btnAssignLoads.Text = "Assign Loads";
            btnAssignLoads.UseVisualStyleBackColor = true;
            btnAssignLoads.Click += btnAssignLoads_Click;
            // 
            // btnSetupMass
            // 
            btnSetupMass.Location = new Point(148, 280);
            btnSetupMass.Margin = new Padding(3, 4, 3, 4);
            btnSetupMass.Name = "btnSetupMass";
            btnSetupMass.Size = new Size(120, 38);
            btnSetupMass.TabIndex = 12;
            btnSetupMass.Text = "Setup Mass";
            btnSetupMass.UseVisualStyleBackColor = true;
            btnSetupMass.Click += btnSetupMass_Click;
            // 
            // btnRunAnalysis
            // 
            btnRunAnalysis.BackColor = Color.LightGreen;
            btnRunAnalysis.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnRunAnalysis.Location = new Point(18, 325);
            btnRunAnalysis.Margin = new Padding(3, 4, 3, 4);
            btnRunAnalysis.Name = "btnRunAnalysis";
            btnRunAnalysis.Size = new Size(250, 45);
            btnRunAnalysis.TabIndex = 13;
            btnRunAnalysis.Text = "▶ RUN ANALYSIS";
            btnRunAnalysis.UseVisualStyleBackColor = false;
            btnRunAnalysis.Click += btnRunAnalysis_Click;
            // 
            // btnRunWorkflow
            // 
            btnRunWorkflow.BackColor = Color.LightBlue;
            btnRunWorkflow.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnRunWorkflow.Location = new Point(17, 623);
            btnRunWorkflow.Margin = new Padding(3, 4, 3, 4);
            btnRunWorkflow.Name = "btnRunWorkflow";
            btnRunWorkflow.Size = new Size(300, 50);
            btnRunWorkflow.TabIndex = 3;
            btnRunWorkflow.Text = "Run Complete Workflow";
            btnRunWorkflow.UseVisualStyleBackColor = false;
            btnRunWorkflow.Click += btnRunWorkflow_Click;
            // 
            // txtStatus
            // 
            txtStatus.Location = new Point(17, 723);
            txtStatus.Margin = new Padding(3, 4, 3, 4);
            txtStatus.Multiline = true;
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.ScrollBars = ScrollBars.Vertical;
            txtStatus.Size = new Size(300, 149);
            txtStatus.TabIndex = 4;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(17, 697);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(52, 20);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Status:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(340, 897);
            Controls.Add(lblStatus);
            Controls.Add(txtStatus);
            Controls.Add(btnRunWorkflow);
            Controls.Add(grpSections);
            Controls.Add(grpMaterials);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "Form1";
            Text = "ETABS Workflow Plugin";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            grpMaterials.ResumeLayout(false);
            grpMaterials.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numConcreteStrength).EndInit();
            grpSections.ResumeLayout(false);
            grpSections.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpMaterials;
        private System.Windows.Forms.Label lblConcreteStrength;
        private System.Windows.Forms.NumericUpDown numConcreteStrength;
        private System.Windows.Forms.Label lblMPa;
        private System.Windows.Forms.GroupBox grpSections;
        private System.Windows.Forms.Button btnCreateSections;
        private System.Windows.Forms.Button btnCreateLoads;
        private System.Windows.Forms.Button btnCreateBoundary;
        private System.Windows.Forms.Button btnCreateDiaphragms;
        private System.Windows.Forms.Button btnApplyMesh;
        private System.Windows.Forms.Button btnAssignSections;
        private System.Windows.Forms.Button btnAssignLoads;
        private System.Windows.Forms.Button btnSetupMass;
        private System.Windows.Forms.Button btnRunAnalysis;
        private System.Windows.Forms.CheckBox chkColumns;
        private System.Windows.Forms.CheckBox chkBeams;
        private System.Windows.Forms.CheckBox chkSlabs;
        private System.Windows.Forms.CheckBox chkWalls;
        private System.Windows.Forms.Button btnRunWorkflow;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label lblStatus;
    }
}