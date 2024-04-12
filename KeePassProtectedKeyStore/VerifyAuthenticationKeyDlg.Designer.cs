namespace KeePassProtectedKeyStore
{
    partial class VerifyAuthenticationKeyDlg
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
            this.LabelHeader = new System.Windows.Forms.Label();
            this.ListBoxUserKeys = new System.Windows.Forms.ListBox();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LabelHeader
            // 
            this.LabelHeader.AutoSize = true;
            this.LabelHeader.Location = new System.Drawing.Point(14, 10);
            this.LabelHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelHeader.Name = "LabelHeader";
            this.LabelHeader.Size = new System.Drawing.Size(801, 29);
            this.LabelHeader.TabIndex = 0;
            this.LabelHeader.Text = "Verify the authentication key(s) you wish to convert to a protected key store:";
            // 
            // ListBoxUserKeys
            // 
            this.ListBoxUserKeys.FormattingEnabled = true;
            this.ListBoxUserKeys.ItemHeight = 29;
            this.ListBoxUserKeys.Location = new System.Drawing.Point(20, 49);
            this.ListBoxUserKeys.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ListBoxUserKeys.Name = "ListBoxUserKeys";
            this.ListBoxUserKeys.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.ListBoxUserKeys.Size = new System.Drawing.Size(932, 294);
            this.ListBoxUserKeys.TabIndex = 1;
            this.ListBoxUserKeys.SelectedIndexChanged += new System.EventHandler(this.ListBoxUserKeys_SelectedIndexChanged);
            // 
            // ButtonOK
            // 
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.Enabled = false;
            this.ButtonOK.Location = new System.Drawing.Point(710, 364);
            this.ButtonOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(117, 46);
            this.ButtonOK.TabIndex = 2;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(835, 364);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(117, 46);
            this.ButtonCancel.TabIndex = 3;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // VerifyAuthenticationKeyDlg
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(974, 432);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.ListBoxUserKeys);
            this.Controls.Add(this.LabelHeader);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VerifyAuthenticationKeyDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Verify Authentication Key(s) to Convert";
            this.Load += new System.EventHandler(this.SelectUserKeyDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelHeader;
        private System.Windows.Forms.ListBox ListBoxUserKeys;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
    }
}