namespace KeePassProtectedKeyStore
{
    partial class OptionsDlg
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
            this.ButtonConvert = new System.Windows.Forms.Button();
            this.ButtonCreateEmergencyFile = new System.Windows.Forms.Button();
            this.ButtonImportEmergencyFile = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.CheckedListBoxAutoLogin = new System.Windows.Forms.CheckedListBox();
            this.ButtonHelp = new System.Windows.Forms.Button();
            this.GroupBoxAutoLogin = new System.Windows.Forms.GroupBox();
            this.CheckBoxAutoLoginByDefault = new System.Windows.Forms.CheckBox();
            this.GroupBoxAutoLogin.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonConvert
            // 
            this.ButtonConvert.Location = new System.Drawing.Point(50, 35);
            this.ButtonConvert.Name = "ButtonConvert";
            this.ButtonConvert.Size = new System.Drawing.Size(810, 50);
            this.ButtonConvert.TabIndex = 1;
            this.ButtonConvert.Text = "Convert Existing Authentication Key(s) to a Protected Key Store...";
            this.ButtonConvert.UseVisualStyleBackColor = true;
            this.ButtonConvert.Click += new System.EventHandler(this.ButtonConvert_Click);
            // 
            // ButtonCreateEmergencyFile
            // 
            this.ButtonCreateEmergencyFile.Location = new System.Drawing.Point(50, 105);
            this.ButtonCreateEmergencyFile.Name = "ButtonCreateEmergencyFile";
            this.ButtonCreateEmergencyFile.Size = new System.Drawing.Size(810, 50);
            this.ButtonCreateEmergencyFile.TabIndex = 2;
            this.ButtonCreateEmergencyFile.Text = "Create an Emergency Key Recovery File...";
            this.ButtonCreateEmergencyFile.UseVisualStyleBackColor = true;
            this.ButtonCreateEmergencyFile.Click += new System.EventHandler(this.ButtonCreateEmergencyFile_Click);
            // 
            // ButtonImportEmergencyFile
            // 
            this.ButtonImportEmergencyFile.Location = new System.Drawing.Point(50, 175);
            this.ButtonImportEmergencyFile.Name = "ButtonImportEmergencyFile";
            this.ButtonImportEmergencyFile.Size = new System.Drawing.Size(810, 50);
            this.ButtonImportEmergencyFile.TabIndex = 3;
            this.ButtonImportEmergencyFile.Text = "Import an Emergency Key Recovery File...";
            this.ButtonImportEmergencyFile.UseVisualStyleBackColor = true;
            this.ButtonImportEmergencyFile.Click += new System.EventHandler(this.ButtonImportEmergencyFile_Click);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(25, 95);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(587, 29);
            this.Label1.TabIndex = 1;
            this.Label1.Text = "Enable/disable auto-login for the following databases:";
            // 
            // CheckedListBoxAutoLogin
            // 
            this.CheckedListBoxAutoLogin.CheckOnClick = true;
            this.CheckedListBoxAutoLogin.FormattingEnabled = true;
            this.CheckedListBoxAutoLogin.HorizontalScrollbar = true;
            this.CheckedListBoxAutoLogin.Location = new System.Drawing.Point(25, 135);
            this.CheckedListBoxAutoLogin.Name = "CheckedListBoxAutoLogin";
            this.CheckedListBoxAutoLogin.Size = new System.Drawing.Size(755, 196);
            this.CheckedListBoxAutoLogin.TabIndex = 2;
            this.CheckedListBoxAutoLogin.ThreeDCheckBoxes = true;
            this.CheckedListBoxAutoLogin.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxAutoLogin_ItemCheck);
            // 
            // ButtonHelp
            // 
            this.ButtonHelp.Location = new System.Drawing.Point(50, 605);
            this.ButtonHelp.Name = "ButtonHelp";
            this.ButtonHelp.Size = new System.Drawing.Size(150, 40);
            this.ButtonHelp.TabIndex = 6;
            this.ButtonHelp.Text = "Help";
            this.ButtonHelp.UseVisualStyleBackColor = true;
            this.ButtonHelp.Click += new System.EventHandler(this.ButtonHelp_Click);
            // 
            // GroupBoxAutoLogin
            // 
            this.GroupBoxAutoLogin.Controls.Add(this.CheckBoxAutoLoginByDefault);
            this.GroupBoxAutoLogin.Controls.Add(this.CheckedListBoxAutoLogin);
            this.GroupBoxAutoLogin.Controls.Add(this.Label1);
            this.GroupBoxAutoLogin.Location = new System.Drawing.Point(50, 250);
            this.GroupBoxAutoLogin.Name = "GroupBoxAutoLogin";
            this.GroupBoxAutoLogin.Size = new System.Drawing.Size(810, 340);
            this.GroupBoxAutoLogin.TabIndex = 7;
            this.GroupBoxAutoLogin.TabStop = false;
            this.GroupBoxAutoLogin.Text = "Auto-Login Options:";
            // 
            // CheckBoxAutoLoginByDefault
            // 
            this.CheckBoxAutoLoginByDefault.AutoSize = true;
            this.CheckBoxAutoLoginByDefault.Location = new System.Drawing.Point(25, 45);
            this.CheckBoxAutoLoginByDefault.Name = "CheckBoxAutoLoginByDefault";
            this.CheckBoxAutoLoginByDefault.Size = new System.Drawing.Size(748, 33);
            this.CheckBoxAutoLoginByDefault.TabIndex = 0;
            this.CheckBoxAutoLoginByDefault.Text = "Enable auto-login by default when a protected key store is created";
            this.CheckBoxAutoLoginByDefault.UseVisualStyleBackColor = true;
            // 
            // OptionsDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 659);
            this.Controls.Add(this.GroupBoxAutoLogin);
            this.Controls.Add(this.ButtonHelp);
            this.Controls.Add(this.ButtonImportEmergencyFile);
            this.Controls.Add(this.ButtonCreateEmergencyFile);
            this.Controls.Add(this.ButtonConvert);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "KeePassProtectedKeyStore Options";
            this.GroupBoxAutoLogin.ResumeLayout(false);
            this.GroupBoxAutoLogin.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonConvert;
        private System.Windows.Forms.Button ButtonCreateEmergencyFile;
        private System.Windows.Forms.Button ButtonImportEmergencyFile;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.CheckedListBox CheckedListBoxAutoLogin;
        private System.Windows.Forms.Button ButtonHelp;
        private System.Windows.Forms.GroupBox GroupBoxAutoLogin;
        private System.Windows.Forms.CheckBox CheckBoxAutoLoginByDefault;
    }
}