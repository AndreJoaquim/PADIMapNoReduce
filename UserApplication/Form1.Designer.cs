namespace UserApplication
{
    partial class UserApplication
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
            this.labelNrSplits = new System.Windows.Forms.Label();
            this.tbNrSplits = new System.Windows.Forms.TextBox();
            this.btInputFile = new System.Windows.Forms.Button();
            this.lbInputFilePath = new System.Windows.Forms.Label();
            this.lbOutputDirectory = new System.Windows.Forms.Label();
            this.btOutputDirectory = new System.Windows.Forms.Button();
            this.btSubmit = new System.Windows.Forms.Button();
            this.btClass = new System.Windows.Forms.Button();
            this.lbClass = new System.Windows.Forms.Label();
            this.labelEntryUrl = new System.Windows.Forms.Label();
            this.tbEntryUrl = new System.Windows.Forms.TextBox();
            this.labelClassName = new System.Windows.Forms.Label();
            this.tbClassName = new System.Windows.Forms.TextBox();
            this.labelProgress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelNrSplits
            // 
            this.labelNrSplits.AutoSize = true;
            this.labelNrSplits.Location = new System.Drawing.Point(22, 136);
            this.labelNrSplits.Name = "labelNrSplits";
            this.labelNrSplits.Size = new System.Drawing.Size(87, 13);
            this.labelNrSplits.TabIndex = 2;
            this.labelNrSplits.Text = "Number of Splits:";
            // 
            // tbNrSplits
            // 
            this.tbNrSplits.Location = new System.Drawing.Point(118, 133);
            this.tbNrSplits.Name = "tbNrSplits";
            this.tbNrSplits.Size = new System.Drawing.Size(100, 20);
            this.tbNrSplits.TabIndex = 3;
            // 
            // btInputFile
            // 
            this.btInputFile.Location = new System.Drawing.Point(12, 12);
            this.btInputFile.Name = "btInputFile";
            this.btInputFile.Size = new System.Drawing.Size(97, 23);
            this.btInputFile.TabIndex = 6;
            this.btInputFile.Text = "Input File";
            this.btInputFile.UseVisualStyleBackColor = true;
            this.btInputFile.Click += new System.EventHandler(this.btInputFile_Click);
            // 
            // lbInputFilePath
            // 
            this.lbInputFilePath.AutoSize = true;
            this.lbInputFilePath.Location = new System.Drawing.Point(115, 17);
            this.lbInputFilePath.Name = "lbInputFilePath";
            this.lbInputFilePath.Size = new System.Drawing.Size(31, 13);
            this.lbInputFilePath.TabIndex = 7;
            this.lbInputFilePath.Text = "none";
            // 
            // lbOutputDirectory
            // 
            this.lbOutputDirectory.AutoSize = true;
            this.lbOutputDirectory.Location = new System.Drawing.Point(115, 46);
            this.lbOutputDirectory.Name = "lbOutputDirectory";
            this.lbOutputDirectory.Size = new System.Drawing.Size(31, 13);
            this.lbOutputDirectory.TabIndex = 8;
            this.lbOutputDirectory.Text = "none";
            // 
            // btOutputDirectory
            // 
            this.btOutputDirectory.Location = new System.Drawing.Point(12, 41);
            this.btOutputDirectory.Name = "btOutputDirectory";
            this.btOutputDirectory.Size = new System.Drawing.Size(97, 23);
            this.btOutputDirectory.TabIndex = 9;
            this.btOutputDirectory.Text = "Output Directory";
            this.btOutputDirectory.UseVisualStyleBackColor = true;
            this.btOutputDirectory.Click += new System.EventHandler(this.btOutputDirectory_Click);
            // 
            // btSubmit
            // 
            this.btSubmit.Location = new System.Drawing.Point(243, 149);
            this.btSubmit.Name = "btSubmit";
            this.btSubmit.Size = new System.Drawing.Size(92, 35);
            this.btSubmit.TabIndex = 10;
            this.btSubmit.Text = "Submit";
            this.btSubmit.UseVisualStyleBackColor = true;
            this.btSubmit.Click += new System.EventHandler(this.btSubmit_Click);
            // 
            // btClass
            // 
            this.btClass.Location = new System.Drawing.Point(12, 72);
            this.btClass.Name = "btClass";
            this.btClass.Size = new System.Drawing.Size(97, 23);
            this.btClass.TabIndex = 11;
            this.btClass.Text = "Class : IMap";
            this.btClass.UseVisualStyleBackColor = true;
            this.btClass.Click += new System.EventHandler(this.btClass_Click);
            // 
            // lbClass
            // 
            this.lbClass.AutoSize = true;
            this.lbClass.Location = new System.Drawing.Point(115, 77);
            this.lbClass.Name = "lbClass";
            this.lbClass.Size = new System.Drawing.Size(31, 13);
            this.lbClass.TabIndex = 12;
            this.lbClass.Text = "none";
            // 
            // labelEntryUrl
            // 
            this.labelEntryUrl.AutoSize = true;
            this.labelEntryUrl.Location = new System.Drawing.Point(22, 167);
            this.labelEntryUrl.Name = "labelEntryUrl";
            this.labelEntryUrl.Size = new System.Drawing.Size(59, 13);
            this.labelEntryUrl.TabIndex = 13;
            this.labelEntryUrl.Text = "Entry URL:";
            // 
            // tbEntryUrl
            // 
            this.tbEntryUrl.Location = new System.Drawing.Point(118, 164);
            this.tbEntryUrl.Name = "tbEntryUrl";
            this.tbEntryUrl.Size = new System.Drawing.Size(100, 20);
            this.tbEntryUrl.TabIndex = 14;
            // 
            // labelClassName
            // 
            this.labelClassName.AutoSize = true;
            this.labelClassName.Location = new System.Drawing.Point(22, 109);
            this.labelClassName.Name = "labelClassName";
            this.labelClassName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelClassName.Size = new System.Drawing.Size(64, 13);
            this.labelClassName.TabIndex = 15;
            this.labelClassName.Text = "Class name:";
            // 
            // tbClassName
            // 
            this.tbClassName.Location = new System.Drawing.Point(118, 106);
            this.tbClassName.Name = "tbClassName";
            this.tbClassName.Size = new System.Drawing.Size(100, 20);
            this.tbClassName.TabIndex = 16;
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(136, 218);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(90, 13);
            this.labelProgress.TabIndex = 17;
            this.labelProgress.Text = "Job in progress ...";
            // 
            // UserApplication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 260);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.tbClassName);
            this.Controls.Add(this.labelClassName);
            this.Controls.Add(this.tbEntryUrl);
            this.Controls.Add(this.labelEntryUrl);
            this.Controls.Add(this.lbClass);
            this.Controls.Add(this.btClass);
            this.Controls.Add(this.btSubmit);
            this.Controls.Add(this.btOutputDirectory);
            this.Controls.Add(this.lbOutputDirectory);
            this.Controls.Add(this.lbInputFilePath);
            this.Controls.Add(this.btInputFile);
            this.Controls.Add(this.tbNrSplits);
            this.Controls.Add(this.labelNrSplits);
            this.Name = "UserApplication";
            this.Text = "PADIMapNoReduce by group 7";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelNrSplits;
        private System.Windows.Forms.TextBox tbNrSplits;
        private System.Windows.Forms.Button btInputFile;
        private System.Windows.Forms.Label lbInputFilePath;
        private System.Windows.Forms.Label lbOutputDirectory;
        private System.Windows.Forms.Button btOutputDirectory;
        private System.Windows.Forms.Button btSubmit;
        private System.Windows.Forms.Button btClass;
        private System.Windows.Forms.Label lbClass;
        private System.Windows.Forms.Label labelEntryUrl;
        private System.Windows.Forms.TextBox tbEntryUrl;
        private System.Windows.Forms.Label labelClassName;
        private System.Windows.Forms.TextBox tbClassName;
        private System.Windows.Forms.Label labelProgress;
    }
}

