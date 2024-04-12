using KeePass.UI;

namespace KeePassProtectedKeyStore
{
    partial class VerifyPasswordDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerifyPasswordDlg));
            this.LabelEnterTextStringHeader = new System.Windows.Forms.Label();
            this.TextBoxPassword = new KeePass.UI.SecureTextBoxEx();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ToolTipForDlg = new System.Windows.Forms.ToolTip(this.components);
            this.CheckBoxShowHide = new System.Windows.Forms.CheckBox();
            this.LabelMatchStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LabelEnterTextStringHeader
            // 
            this.LabelEnterTextStringHeader.AutoSize = true;
            this.LabelEnterTextStringHeader.Location = new System.Drawing.Point(13, 33);
            this.LabelEnterTextStringHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelEnterTextStringHeader.Name = "LabelEnterTextStringHeader";
            this.LabelEnterTextStringHeader.Size = new System.Drawing.Size(748, 29);
            this.LabelEnterTextStringHeader.TabIndex = 0;
            this.LabelEnterTextStringHeader.Text = "Verify the password associated with this emergency key recovery file:";
            // 
            // TextBoxPassword
            // 
            this.TextBoxPassword.Location = new System.Drawing.Point(18, 75);
            this.TextBoxPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TextBoxPassword.Name = "TextBoxPassword";
            this.TextBoxPassword.Size = new System.Drawing.Size(854, 35);
            this.TextBoxPassword.TabIndex = 2;
            this.TextBoxPassword.TextChanged += new System.EventHandler(this.TextBoxUserKeyString_TextChanged);
            // 
            // ButtonOK
            // 
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.Enabled = false;
            this.ButtonOK.Location = new System.Drawing.Point(719, 178);
            this.ButtonOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(117, 39);
            this.ButtonOK.TabIndex = 7;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(844, 178);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(117, 39);
            this.ButtonCancel.TabIndex = 8;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // CheckBoxShowHide
            // 
            this.CheckBoxShowHide.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBoxShowHide.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CheckBoxShowHide.BackgroundImage")));
            this.CheckBoxShowHide.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.CheckBoxShowHide.Location = new System.Drawing.Point(892, 71);
            this.CheckBoxShowHide.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CheckBoxShowHide.Name = "CheckBoxShowHide";
            this.CheckBoxShowHide.Size = new System.Drawing.Size(69, 41);
            this.CheckBoxShowHide.TabIndex = 3;
            this.CheckBoxShowHide.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBoxShowHide.UseVisualStyleBackColor = true;
            this.CheckBoxShowHide.CheckedChanged += new System.EventHandler(this.CheckBoxShowHide_CheckedChanged);
            // 
            // LabelMatchStatus
            // 
            this.LabelMatchStatus.Location = new System.Drawing.Point(13, 130);
            this.LabelMatchStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelMatchStatus.Name = "LabelMatchStatus";
            this.LabelMatchStatus.Size = new System.Drawing.Size(657, 29);
            this.LabelMatchStatus.TabIndex = 12;
            // 
            // VerifyPasswordDlg
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(974, 229);
            this.Controls.Add(this.LabelMatchStatus);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.CheckBoxShowHide);
            this.Controls.Add(this.TextBoxPassword);
            this.Controls.Add(this.LabelEnterTextStringHeader);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VerifyPasswordDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Emergency Key Recovery File";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelEnterTextStringHeader;
        private KeePass.UI.SecureTextBoxEx TextBoxPassword;
        private System.Windows.Forms.CheckBox CheckBoxShowHide;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.ToolTip ToolTipForDlg;
        private System.Windows.Forms.Label LabelMatchStatus;
    }
}