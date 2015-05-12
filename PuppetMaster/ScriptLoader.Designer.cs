namespace PuppetMaster {
    partial class ScriptLoader {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.bt_execute = new System.Windows.Forms.Button();
            this.bt_loadScript = new System.Windows.Forms.Button();
            this.tb_command = new System.Windows.Forms.TextBox();
            this.tb_result = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // bt_execute
            // 
            this.bt_execute.Location = new System.Drawing.Point(543, 39);
            this.bt_execute.Name = "bt_execute";
            this.bt_execute.Size = new System.Drawing.Size(75, 23);
            this.bt_execute.TabIndex = 0;
            this.bt_execute.Text = "Execute";
            this.bt_execute.UseVisualStyleBackColor = true;
            this.bt_execute.Click += new System.EventHandler(this.bt_execute_Click);
            // 
            // bt_loadScript
            // 
            this.bt_loadScript.Location = new System.Drawing.Point(12, 12);
            this.bt_loadScript.Name = "bt_loadScript";
            this.bt_loadScript.Size = new System.Drawing.Size(606, 23);
            this.bt_loadScript.TabIndex = 1;
            this.bt_loadScript.Text = "Load Script";
            this.bt_loadScript.UseVisualStyleBackColor = true;
            this.bt_loadScript.Click += new System.EventHandler(this.bt_loadScript_Click);
            // 
            // tb_command
            // 
            this.tb_command.Location = new System.Drawing.Point(13, 41);
            this.tb_command.Name = "tb_command";
            this.tb_command.Size = new System.Drawing.Size(524, 20);
            this.tb_command.TabIndex = 2;
            // 
            // tb_result
            // 
            this.tb_result.Location = new System.Drawing.Point(13, 68);
            this.tb_result.Multiline = true;
            this.tb_result.Name = "tb_result";
            this.tb_result.Size = new System.Drawing.Size(605, 135);
            this.tb_result.TabIndex = 3;
            // 
            // ScriptLoader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 215);
            this.Controls.Add(this.tb_result);
            this.Controls.Add(this.tb_command);
            this.Controls.Add(this.bt_loadScript);
            this.Controls.Add(this.bt_execute);
            this.Name = "ScriptLoader";
            this.Text = "ScriptLoader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bt_execute;
        private System.Windows.Forms.Button bt_loadScript;
        private System.Windows.Forms.TextBox tb_command;
        private System.Windows.Forms.TextBox tb_result;
    }
}