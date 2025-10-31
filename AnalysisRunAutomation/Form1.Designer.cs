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
            tabControl = new TabControl();
            tabSetup = new TabPage();
            tabWalls = new TabPage();
            tabAnalysis = new TabPage();
            tabLogs = new TabPage();

            // Setup tab controls
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

            // Analysis tab controls
            btnCreateLoads = new Button();
            btnCreateBoundary = new Button();
            btnCreateDiaphragms = new Button();
            btnApplyMesh = new Button();
            btnAssignSections = new Button();
            btnAssignLoads = new Button();
            btnSetupMass = new Button();
            btnRunAnalysis = new Button();
            btnRunWorkflow = new Button();

            // Logs tab controls
            txtStatus = new TextBox();

            tabControl.SuspendLayout();
            tabSetup.SuspendLayout();
            tabAnalysis.SuspendLayout();
            tabLogs.SuspendLayout();
            grpMaterials.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numConcreteStrength).BeginInit();
            grpSections.SuspendLayout();
            SuspendLayout();

            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            lblTitle.Location = new Point(12, 11);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(400, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "ETABS Automation (Analysis + Walls)";

            //
            // tabControl
            //
            tabControl.Controls.Add(tabSetup);
            tabControl.Controls.Add(tabWalls);
            tabControl.Controls.Add(tabAnalysis);
            tabControl.Controls.Add(tabLogs);
            tabControl.Location = new Point(12, 50);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(760, 790);
            tabControl.TabIndex = 1;

            //
            // tabSetup
            //
            tabSetup.Controls.Add(grpMaterials);
            tabSetup.Controls.Add(grpSections);
            tabSetup.Location = new Point(4, 29);
            tabSetup.Name = "tabSetup";
            tabSetup.Padding = new Padding(3);
            tabSetup.Size = new Size(752, 757);
            tabSetup.TabIndex = 0;
            tabSetup.Text = "Setup";
            tabSetup.UseVisualStyleBackColor = true;

            //
            // grpMaterials
            //
            grpMaterials.Controls.Add(lblMPa);
            grpMaterials.Controls.Add(numConcreteStrength);
            grpMaterials.Controls.Add(lblConcreteStrength);
            grpMaterials.Location = new Point(17, 20);
            grpMaterials.Name = "grpMaterials";
            grpMaterials.Size = new Size(300, 100);
            grpMaterials.TabIndex = 0;
            grpMaterials.TabStop = false;
            grpMaterials.Text = "Material Properties";

            //
            // lblMPa
            //
            lblMPa.AutoSize = true;
            lblMPa.Location = new Point(240, 44);
            lblMPa.Name = "lblMPa";
            lblMPa.Size = new Size(37, 20);
            lblMPa.Text = "MPa";

            //
            // numConcreteStrength
            //
            numConcreteStrength.Location = new Point(150, 41);
            numConcreteStrength.Minimum = new decimal(new int[] { 15, 0, 0, 0 });
            numConcreteStrength.Name = "numConcreteStrength";
            numConcreteStrength.Size = new Size(80, 27);
            numConcreteStrength.Value = new decimal(new int[] { 25, 0, 0, 0 });

            //
            // lblConcreteStrength
            //
            lblConcreteStrength.AutoSize = true;
            lblConcreteStrength.Location = new Point(15, 44);
            lblConcreteStrength.Name = "lblConcreteStrength";
            lblConcreteStrength.Size = new Size(131, 20);
            lblConcreteStrength.Text = "Concrete Strength:";

            //
            // grpSections
            //
            grpSections.Controls.Add(chkWalls);
            grpSections.Controls.Add(chkSlabs);
            grpSections.Controls.Add(chkBeams);
            grpSections.Controls.Add(chkColumns);
            grpSections.Controls.Add(btnCreateSections);
            grpSections.Location = new Point(17, 140);
            grpSections.Name = "grpSections";
            grpSections.Size = new Size(300, 180);
            grpSections.TabIndex = 1;
            grpSections.TabStop = false;
            grpSections.Text = "Section Creation";

            //
            // chkWalls
            //
            chkWalls.AutoSize = true;
            chkWalls.Checked = true;
            chkWalls.CheckState = CheckState.Checked;
            chkWalls.Location = new Point(110, 60);
            chkWalls.Name = "chkWalls";
            chkWalls.Size = new Size(66, 24);
            chkWalls.Text = "Walls";
            chkWalls.UseVisualStyleBackColor = true;

            //
            // chkSlabs
            //
            chkSlabs.AutoSize = true;
            chkSlabs.Checked = true;
            chkSlabs.CheckState = CheckState.Checked;
            chkSlabs.Location = new Point(18, 60);
            chkSlabs.Name = "chkSlabs";
            chkSlabs.Size = new Size(66, 24);
            chkSlabs.Text = "Slabs";
            chkSlabs.UseVisualStyleBackColor = true;

            //
            // chkBeams
            //
            chkBeams.AutoSize = true;
            chkBeams.Checked = true;
            chkBeams.CheckState = CheckState.Checked;
            chkBeams.Location = new Point(110, 30);
            chkBeams.Name = "chkBeams";
            chkBeams.Size = new Size(75, 24);
            chkBeams.Text = "Beams";
            chkBeams.UseVisualStyleBackColor = true;

            //
            // chkColumns
            //
            chkColumns.AutoSize = true;
            chkColumns.Checked = true;
            chkColumns.CheckState = CheckState.Checked;
            chkColumns.Location = new Point(18, 30);
            chkColumns.Name = "chkColumns";
            chkColumns.Size = new Size(88, 24);
            chkColumns.Text = "Columns";
            chkColumns.UseVisualStyleBackColor = true;

            //
            // btnCreateSections
            //
            btnCreateSections.Location = new Point(18, 110);
            btnCreateSections.Name = "btnCreateSections";
            btnCreateSections.Size = new Size(250, 50);
            btnCreateSections.Text = "Create Sections";
            btnCreateSections.UseVisualStyleBackColor = true;
            btnCreateSections.Click += btnCreateSections_Click;

            //
            // tabWalls
            //
            tabWalls.Location = new Point(4, 29);
            tabWalls.Name = "tabWalls";
            tabWalls.Padding = new Padding(3);
            tabWalls.Size = new Size(752, 757);
            tabWalls.TabIndex = 1;
            tabWalls.Text = "Walls";
            tabWalls.UseVisualStyleBackColor = true;

            //
            // tabAnalysis
            //
            tabAnalysis.Controls.Add(btnCreateLoads);
            tabAnalysis.Controls.Add(btnCreateBoundary);
            tabAnalysis.Controls.Add(btnCreateDiaphragms);
            tabAnalysis.Controls.Add(btnApplyMesh);
            tabAnalysis.Controls.Add(btnAssignSections);
            tabAnalysis.Controls.Add(btnAssignLoads);
            tabAnalysis.Controls.Add(btnSetupMass);
            tabAnalysis.Controls.Add(btnRunAnalysis);
            tabAnalysis.Controls.Add(btnRunWorkflow);
            tabAnalysis.Location = new Point(4, 29);
            tabAnalysis.Name = "tabAnalysis";
            tabAnalysis.Size = new Size(752, 757);
            tabAnalysis.TabIndex = 2;
            tabAnalysis.Text = "Analysis";
            tabAnalysis.UseVisualStyleBackColor = true;

            //
            // btnCreateLoads
            //
            btnCreateLoads.Location = new Point(20, 20);
            btnCreateLoads.Name = "btnCreateLoads";
            btnCreateLoads.Size = new Size(250, 40);
            btnCreateLoads.Text = "Create Load Patterns";
            btnCreateLoads.UseVisualStyleBackColor = true;
            btnCreateLoads.Click += btnCreateLoads_Click;

            //
            // btnCreateBoundary
            //
            btnCreateBoundary.Location = new Point(20, 70);
            btnCreateBoundary.Name = "btnCreateBoundary";
            btnCreateBoundary.Size = new Size(250, 40);
            btnCreateBoundary.Text = "Apply Fixed Boundary Conditions";
            btnCreateBoundary.UseVisualStyleBackColor = true;
            btnCreateBoundary.Click += btnApplyBCs_Click;

            //
            // btnCreateDiaphragms
            //
            btnCreateDiaphragms.Location = new Point(20, 120);
            btnCreateDiaphragms.Name = "btnCreateDiaphragms";
            btnCreateDiaphragms.Size = new Size(250, 40);
            btnCreateDiaphragms.Text = "Create Diaphragms";
            btnCreateDiaphragms.UseVisualStyleBackColor = true;
            btnCreateDiaphragms.Click += btnCreateDiaphragms_Click;

            //
            // btnApplyMesh
            //
            btnApplyMesh.Location = new Point(20, 170);
            btnApplyMesh.Name = "btnApplyMesh";
            btnApplyMesh.Size = new Size(250, 40);
            btnApplyMesh.Text = "Apply 1m Mesh";
            btnApplyMesh.UseVisualStyleBackColor = true;
            btnApplyMesh.Click += btnApplyMesh_Click;

            //
            // btnAssignSections
            //
            btnAssignSections.Location = new Point(20, 220);
            btnAssignSections.Name = "btnAssignSections";
            btnAssignSections.Size = new Size(250, 40);
            btnAssignSections.Text = "Assign Sections Intelligently";
            btnAssignSections.UseVisualStyleBackColor = true;
            btnAssignSections.Click += btnAssignSections_Click;

            //
            // btnAssignLoads
            //
            btnAssignLoads.Location = new Point(20, 270);
            btnAssignLoads.Name = "btnAssignLoads";
            btnAssignLoads.Size = new Size(250, 40);
            btnAssignLoads.Text = "Assign Loads to Slabs";
            btnAssignLoads.UseVisualStyleBackColor = true;
            btnAssignLoads.Click += btnAssignLoads_Click;

            //
            // btnSetupMass
            //
            btnSetupMass.Location = new Point(20, 320);
            btnSetupMass.Name = "btnSetupMass";
            btnSetupMass.Size = new Size(250, 40);
            btnSetupMass.Text = "Setup Seismic Mass";
            btnSetupMass.UseVisualStyleBackColor = true;
            btnSetupMass.Click += btnSetupMass_Click;

            //
            // btnRunAnalysis
            //
            btnRunAnalysis.BackColor = Color.LightGreen;
            btnRunAnalysis.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnRunAnalysis.Location = new Point(20, 380);
            btnRunAnalysis.Name = "btnRunAnalysis";
            btnRunAnalysis.Size = new Size(250, 50);
            btnRunAnalysis.Text = "▶ RUN ANALYSIS";
            btnRunAnalysis.UseVisualStyleBackColor = false;
            btnRunAnalysis.Click += btnRunAnalysis_Click;

            //
            // btnRunWorkflow
            //
            btnRunWorkflow.BackColor = Color.LightBlue;
            btnRunWorkflow.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold);
            btnRunWorkflow.Location = new Point(20, 450);
            btnRunWorkflow.Name = "btnRunWorkflow";
            btnRunWorkflow.Size = new Size(250, 60);
            btnRunWorkflow.Text = "Run Complete Workflow";
            btnRunWorkflow.UseVisualStyleBackColor = false;
            btnRunWorkflow.Click += btnRunWorkflow_Click;

            //
            // tabLogs
            //
            tabLogs.Controls.Add(txtStatus);
            tabLogs.Location = new Point(4, 29);
            tabLogs.Name = "tabLogs";
            tabLogs.Size = new Size(752, 757);
            tabLogs.TabIndex = 3;
            tabLogs.Text = "Logs";
            tabLogs.UseVisualStyleBackColor = true;

            //
            // txtStatus
            //
            txtStatus.Dock = DockStyle.Fill;
            txtStatus.Font = new Font("Consolas", 9F);
            txtStatus.Location = new Point(3, 3);
            txtStatus.Multiline = true;
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.ScrollBars = ScrollBars.Vertical;
            txtStatus.Size = new Size(746, 751);
            txtStatus.TabIndex = 0;

            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 861);
            Controls.Add(tabControl);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "Form1";
            Text = "ETABS Automation Plugin";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            tabControl.ResumeLayout(false);
            tabSetup.ResumeLayout(false);
            tabAnalysis.ResumeLayout(false);
            tabLogs.ResumeLayout(false);
            tabLogs.PerformLayout();
            grpMaterials.ResumeLayout(false);
            grpMaterials.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numConcreteStrength).EndInit();
            grpSections.ResumeLayout(false);
            grpSections.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private TabControl tabControl;
        private TabPage tabSetup;
        private TabPage tabWalls;
        private TabPage tabAnalysis;
        private TabPage tabLogs;

        // Setup tab controls
        private GroupBox grpMaterials;
        private Label lblConcreteStrength;
        private NumericUpDown numConcreteStrength;
        private Label lblMPa;
        private GroupBox grpSections;
        private CheckBox chkColumns;
        private CheckBox chkBeams;
        private CheckBox chkSlabs;
        private CheckBox chkWalls;
        private Button btnCreateSections;

        // Analysis tab controls
        private Button btnCreateLoads;
        private Button btnCreateBoundary;
        private Button btnCreateDiaphragms;
        private Button btnApplyMesh;
        private Button btnAssignSections;
        private Button btnAssignLoads;
        private Button btnSetupMass;
        private Button btnRunAnalysis;
        private Button btnRunWorkflow;

        // Logs tab control
        private TextBox txtStatus;
    }
}
