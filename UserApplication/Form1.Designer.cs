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
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.btInputFile = new System.Windows.Forms.Button();
            this.lbInputFilePath = new System.Windows.Forms.Label();
            this.lbOutputDirectory = new System.Windows.Forms.Label();
            this.btOutputDirectory = new System.Windows.Forms.Button();
            this.btSubmit = new System.Windows.Forms.Button();
            this.btClass = new System.Windows.Forms.Button();
            this.lbClass = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Number of Splits:";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(105, 111);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 3;
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
            this.btSubmit.Location = new System.Drawing.Point(342, 133);
            this.btSubmit.Name = "btSubmit";
            this.btSubmit.Size = new System.Drawing.Size(75, 23);
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
            // UserApplication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 168);
            this.Controls.Add(this.lbClass);
            this.Controls.Add(this.btClass);
            this.Controls.Add(this.btSubmit);
            this.Controls.Add(this.btOutputDirectory);
            this.Controls.Add(this.lbOutputDirectory);
            this.Controls.Add(this.lbInputFilePath);
            this.Controls.Add(this.btInputFile);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label2);
            this.Name = "UserApplication";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button btInputFile;
        private System.Windows.Forms.Label lbInputFilePath;
        private System.Windows.Forms.Label lbOutputDirectory;
        private System.Windows.Forms.Button btOutputDirectory;
        private System.Windows.Forms.Button btSubmit;
        private System.Windows.Forms.Button btClass;
        private System.Windows.Forms.Label lbClass;
    }
}

