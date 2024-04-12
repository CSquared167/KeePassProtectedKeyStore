using KeePass.UI;

namespace KeePassProtectedKeyStore
{
    partial class CreatePasswordDlg
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreatePasswordDlg));
            this.LabelEnterTextStringHeader = new System.Windows.Forms.Label();
            this.LabelNote = new System.Windows.Forms.Label();
            this.TextBoxPassword = new KeePass.UI.SecureTextBoxEx();
            this.LabelReEnterTextStringHeader = new System.Windows.Forms.Label();
            this.TextBoxPassword2 = new KeePass.UI.SecureTextBoxEx();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ToolTipForDlg = new System.Windows.Forms.ToolTip(this.components);
            this.LabelEstimatedQualityHeader = new System.Windows.Forms.Label();
            this.CtrlEstimatedQuality = new KeePass.UI.QualityProgressBar();
            this.LabelMatchStatus = new System.Windows.Forms.Label();
            this.LabelNumberOfCharacters = new System.Windows.Forms.Label();
            this.CheckBoxShowHide = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // LabelEnterTextStringHeader
            // 
            this.LabelEnterTextStringHeader.AutoSize = true;
            this.LabelEnterTextStringHeader.Location = new System.Drawing.Point(13, 23);
            this.LabelEnterTextStringHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelEnterTextStringHeader.Name = "LabelEnterTextStringHeader";
            this.LabelEnterTextStringHeader.Size = new System.Drawing.Size(893, 29);
            this.LabelEnterTextStringHeader.TabIndex = 0;
            this.LabelEnterTextStringHeader.Text = "Enter a password that will be used to protect the emergency key recovery file dat" +
    "a: ";
            // 
            // LabelNote
            // 
            this.LabelNote.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelNote.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LabelNote.Location = new System.Drawing.Point(13, 282);
            this.LabelNote.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelNote.Name = "LabelNote";
            this.LabelNote.Size = new System.Drawing.Size(948, 64);
            this.LabelNote.TabIndex = 1;
            this.LabelNote.Text = "NOTE: Make sure to remember this password or save a copy of it in a secure place." +
    " You will need it if you ever need to import the emergency key recovery file.";
            // 
            // TextBoxPassword
            // 
            this.TextBoxPassword.Location = new System.Drawing.Point(18, 55);
            this.TextBoxPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TextBoxPassword.Name = "TextBoxPassword";
            this.TextBoxPassword.Size = new System.Drawing.Size(854, 35);
            this.TextBoxPassword.TabIndex = 2;
            this.TextBoxPassword.TextChanged += new System.EventHandler(this.TextBoxPassword_TextChanged);
            // 
            // LabelReEnterTextStringHeader
            // 
            this.LabelReEnterTextStringHeader.AutoSize = true;
            this.LabelReEnterTextStringHeader.Location = new System.Drawing.Point(13, 103);
            this.LabelReEnterTextStringHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelReEnterTextStringHeader.Name = "LabelReEnterTextStringHeader";
            this.LabelReEnterTextStringHeader.Size = new System.Drawing.Size(224, 29);
            this.LabelReEnterTextStringHeader.TabIndex = 4;
            this.LabelReEnterTextStringHeader.Text = "Re-enter password:";
            // 
            // TextBoxPassword2
            // 
            this.TextBoxPassword2.Location = new System.Drawing.Point(18, 135);
            this.TextBoxPassword2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TextBoxPassword2.Name = "TextBoxPassword2";
            this.TextBoxPassword2.Size = new System.Drawing.Size(854, 35);
            this.TextBoxPassword2.TabIndex = 5;
            this.TextBoxPassword2.TextChanged += new System.EventHandler(this.TextBoxPassword2_TextChanged);
            // 
            // ButtonOK
            // 
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.Enabled = false;
            this.ButtonOK.Location = new System.Drawing.Point(719, 363);
            this.ButtonOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(117, 39);
            this.ButtonOK.TabIndex = 7;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(844, 363);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(117, 39);
            this.ButtonCancel.TabIndex = 8;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // LabelEstimatedQualityHeader
            // 
            this.LabelEstimatedQualityHeader.AutoSize = true;
            this.LabelEstimatedQualityHeader.Location = new System.Drawing.Point(13, 183);
            this.LabelEstimatedQualityHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelEstimatedQualityHeader.Name = "LabelEstimatedQualityHeader";
            this.LabelEstimatedQualityHeader.Size = new System.Drawing.Size(201, 29);
            this.LabelEstimatedQualityHeader.TabIndex = 9;
            this.LabelEstimatedQualityHeader.Text = "Estimated quality:";
            // 
            // CtrlEstimatedQuality
            // 
            this.CtrlEstimatedQuality.Location = new System.Drawing.Point(18, 215);
            this.CtrlEstimatedQuality.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CtrlEstimatedQuality.Name = "CtrlEstimatedQuality";
            this.CtrlEstimatedQuality.Size = new System.Drawing.Size(854, 35);
            this.CtrlEstimatedQuality.TabIndex = 10;
            // 
            // LabelMatchStatus
            // 
            this.LabelMatchStatus.Location = new System.Drawing.Point(572, 103);
            this.LabelMatchStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelMatchStatus.Name = "LabelMatchStatus";
            this.LabelMatchStatus.Size = new System.Drawing.Size(300, 29);
            this.LabelMatchStatus.TabIndex = 11;
            this.LabelMatchStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // LabelNumberOfCharacters
            // 
            this.LabelNumberOfCharacters.Location = new System.Drawing.Point(565, 183);
            this.LabelNumberOfCharacters.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelNumberOfCharacters.Name = "LabelNumberOfCharacters";
            this.LabelNumberOfCharacters.Size = new System.Drawing.Size(307, 29);
            this.LabelNumberOfCharacters.TabIndex = 12;
            this.LabelNumberOfCharacters.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // CheckBoxShowHide
            // 
            this.CheckBoxShowHide.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBoxShowHide.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CheckBoxShowHide.BackgroundImage")));
            this.CheckBoxShowHide.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.CheckBoxShowHide.Location = new System.Drawing.Point(892, 51);
            this.CheckBoxShowHide.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CheckBoxShowHide.Name = "CheckBoxShowHide";
            this.CheckBoxShowHide.Size = new System.Drawing.Size(69, 41);
            this.CheckBoxShowHide.TabIndex = 3;
            this.CheckBoxShowHide.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBoxShowHide.UseVisualStyleBackColor = true;
            this.CheckBoxShowHide.CheckedChanged += new System.EventHandler(this.CheckBoxShowHide_CheckedChanged);
            // 
            // CreatePasswordDlg
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(974, 419);
            this.Controls.Add(this.LabelNumberOfCharacters);
            this.Controls.Add(this.LabelMatchStatus);
            this.Controls.Add(this.CtrlEstimatedQuality);
            this.Controls.Add(this.LabelEstimatedQualityHeader);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.TextBoxPassword2);
            this.Controls.Add(this.LabelReEnterTextStringHeader);
            this.Controls.Add(this.CheckBoxShowHide);
            this.Controls.Add(this.TextBoxPassword);
            this.Controls.Add(this.LabelNote);
            this.Controls.Add(this.LabelEnterTextStringHeader);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreatePasswordDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Emergency Key Recovery File";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CreatePasswordDlg_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelEnterTextStringHeader;
        private System.Windows.Forms.Label LabelNote;
        private KeePass.UI.SecureTextBoxEx TextBoxPassword;
        private System.Windows.Forms.CheckBox CheckBoxShowHide;
        private System.Windows.Forms.Label LabelReEnterTextStringHeader;
        private KeePass.UI.SecureTextBoxEx TextBoxPassword2;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.ToolTip ToolTipForDlg;
        private System.Windows.Forms.Label LabelEstimatedQualityHeader;
        private QualityProgressBar CtrlEstimatedQuality;
        private System.Windows.Forms.Label LabelMatchStatus;
        private System.Windows.Forms.Label LabelNumberOfCharacters;
    }
}