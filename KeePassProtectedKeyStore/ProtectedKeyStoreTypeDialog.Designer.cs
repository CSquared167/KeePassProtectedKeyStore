namespace KeePassProtectedKeyStore
{
    partial class ProtectedKeyStoreTypeDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProtectedKeyStoreTypeDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.RadioButtonDefault = new System.Windows.Forms.RadioButton();
            this.RadioButtonIndividual = new System.Windows.Forms.RadioButton();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(701, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please specify the type of protected key store you want to create:";
            // 
            // RadioButtonDefault
            // 
            this.RadioButtonDefault.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.RadioButtonDefault.Checked = true;
            this.RadioButtonDefault.Location = new System.Drawing.Point(18, 70);
            this.RadioButtonDefault.Name = "RadioButtonDefault";
            this.RadioButtonDefault.Size = new System.Drawing.Size(946, 139);
            this.RadioButtonDefault.TabIndex = 1;
            this.RadioButtonDefault.TabStop = true;
            this.RadioButtonDefault.Text = resources.GetString("RadioButtonDefault.Text");
            this.RadioButtonDefault.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.RadioButtonDefault.UseVisualStyleBackColor = true;
            // 
            // RadioButtonIndividual
            // 
            this.RadioButtonIndividual.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.RadioButtonIndividual.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RadioButtonIndividual.Location = new System.Drawing.Point(18, 215);
            this.RadioButtonIndividual.Name = "RadioButtonIndividual";
            this.RadioButtonIndividual.Size = new System.Drawing.Size(946, 143);
            this.RadioButtonIndividual.TabIndex = 2;
            this.RadioButtonIndividual.Text = resources.GetString("RadioButtonIndividual.Text");
            this.RadioButtonIndividual.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.RadioButtonIndividual.UseVisualStyleBackColor = true;
            // 
            // ButtonOK
            // 
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.Location = new System.Drawing.Point(724, 388);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(117, 39);
            this.ButtonOK.TabIndex = 3;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(847, 388);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(117, 39);
            this.ButtonCancel.TabIndex = 4;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ProtectedKeyStoreTypeDialog
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(1014, 449);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.RadioButtonIndividual);
            this.Controls.Add(this.RadioButtonDefault);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProtectedKeyStoreTypeDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Protected Key Store Type";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton RadioButtonDefault;
        private System.Windows.Forms.RadioButton RadioButtonIndividual;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
    }
}