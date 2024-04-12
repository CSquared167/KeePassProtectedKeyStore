using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    public partial class VerifyPasswordDlg : Form
    {
        private byte[] PasswordHash { get; } = null;

        public VerifyPasswordDlg(string passwordHash)
        {
            InitializeComponent();

            ToolTipForDlg.SetToolTip(CheckBoxShowHide, "Show/hide password");
            TextBoxPassword.MaxLength = EmergencyKeyRecoveryFile.AesKeySizeInBytes;
            PasswordHash = Helper.StringToByteArray(passwordHash);

            UpdateUI();
        }

        // Returns the user's input password in a byte array, padded to the number of bytes required
        // for an AES encryption key.
        public byte[] Password =>
            EmergencyKeyRecoveryFile.MakeAesEncryptionKeyFromString(TextBoxPassword.TextEx.ReadUtf8());

        // Event handler when the user has changed the password.
        private void TextBoxUserKeyString_TextChanged(object sender, EventArgs e) =>
            UpdateUI();

        // Event handler when the user clicks the show/hide button.
        private void CheckBoxShowHide_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxPassword.EnableProtection(!TextBoxPassword.UseSystemPasswordChar);

            UpdateUI();
        }

        // Method to update the UI elements on the form.
        private void UpdateUI()
        {
            bool userKeyStringNotEmpty = TextBoxPassword.TextEx.Length > 0;

            // Check whether the password being entered matches with the MD5 hash value that was
            // passed in.
            byte[] pbHash = Helper.MD5HashData(Password);
            bool passwordMatches = Enumerable.SequenceEqual(pbHash, PasswordHash);

            // Set TextBoxUserKeyString.BackColor to give a visual indicator whether the password
            // matches the hash value.
            TextBoxPassword.BackColor = passwordMatches ?
                SystemColors.Window :
                Color.Pink;

            // Display the match status.
            LabelMatchStatus.Text = string.Format("Password {0}", passwordMatches ? "matches" : "does not match");

            // Enable the OK button if TextBoxPassword contains one or more characters, AND the
            // password matches the hash value.
            ButtonOK.Enabled = userKeyStringNotEmpty && passwordMatches;
        }
    }
}
