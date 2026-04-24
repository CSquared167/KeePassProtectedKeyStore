using KeePassLib.Cryptography;
using KeePassProtectedKeyStore.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    public partial class CreatePasswordDlg : Form
    {
        // From the KeePass source code.
        private int QualityBitsWeak => 79;

        private bool CancelClose { get; set; } = false;

        public CreatePasswordDlg()
        {
            InitializeComponent();

            ToolTipForDlg.SetToolTip(CheckBoxShowHide, "Show/hide password");
            TextBoxPassword.MaxLength = EmergencyKeyRecoveryFile.AesKeySizeInBytes;
            TextBoxPassword2.MaxLength = EmergencyKeyRecoveryFile.AesKeySizeInBytes;

            UpdateUI();
        }

        // Returns the user's input password in a byte array, padded to the number of bytes required
        // for an AES encryption key.
        public byte[] Password =>
            EmergencyKeyRecoveryFile.MakeAesEncryptionKeyFromString(TextBoxPassword.TextEx.ReadUtf8());

        // Event handler when the user has changed the password.
        private void TextBoxPassword_TextChanged(object sender, EventArgs e) =>
            UpdateUI();

        // Event handler when the user has changed the re-entered password.
        private void TextBoxPassword2_TextChanged(object sender, EventArgs e) =>
            UpdateUI();

        // Event handler when the user clicks the show/hide button.
        private void CheckBoxShowHide_CheckedChanged(object sender, EventArgs e)
        {
            bool passwordHidden = TextBoxPassword.UseSystemPasswordChar;

            TextBoxPassword.EnableProtection(!passwordHidden);
            TextBoxPassword2.EnableProtection(!passwordHidden);
            TextBoxPassword2.Visible = !passwordHidden;
            LabelReEnterTextStringHeader.Enabled = TextBoxPassword2.Visible;

            UpdateUI();
        }

        // Event handler when the user clicks the OK button.
        private void ButtonOK_Click(object sender, EventArgs e) =>
            CancelClose = EstimatedQuality <= QualityBitsWeak &&
                Helper.DisplayMessage(Resources.CreatePasswordWeak,
                    string.Empty,
                    string.Empty,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == DialogResult.No;

        // Event handler when the form is closing.
        private void CreatePasswordDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = CancelClose;
            CancelClose = false;
        }

        // Method to update the UI elements on the form.
        private void UpdateUI()
        {
            bool passwordNotEmpty = TextBoxPassword.TextEx.Length > 0;
            bool populatingBothKeyStrings = passwordNotEmpty && TextBoxPassword2.Visible;

            // Both passwords are considered to be matching under the following conditions:
            // - Only TextBoxPassword is being populated, OR
            // - Both passwords match.
            bool passwordsMatch = !populatingBothKeyStrings ||
                TextBoxPassword.TextEx.ReadString() == TextBoxPassword2.TextEx.ReadString();

            // Get the estimated password strength quality.
            int estimatedQuality = EstimatedQuality;

            // Display the estimated quality.
            CtrlEstimatedQuality.Value = estimatedQuality;
            CtrlEstimatedQuality.ProgressText = estimatedQuality.ToString() + " bits";

            // Set TextBoxPassword2.BackColor to give a visual indicator whether the two passwords match.
            TextBoxPassword2.BackColor = passwordsMatch ?
                SystemColors.Window :
                Color.Pink;

            // Display the number of characters entered in TextBoxPassword.
            LabelNumberOfCharacters.Text = string.Format("{0} character{1}",
                TextBoxPassword.TextEx.Length.ToString(),
                TextBoxPassword.TextEx.Length == 1 ? string.Empty : "s");

            // Display the match status if both passwords are being populated.
            LabelMatchStatus.Text = populatingBothKeyStrings ?
                string.Format("Passwords{0}match", passwordsMatch ? " " : " do not ") :
                string.Empty;

            // Enable the OK button under the following conditions:
            // - TextBoxPassword contains one or more characters, AND
            // - Both passwords are considered to be matching.
            ButtonOK.Enabled = passwordNotEmpty && passwordsMatch;
        }

        // Calculate the estimated string quality. Algorithm based on the KeePass source code.
        private int EstimatedQuality
        {
            get
            {
                char[] passwordChars = TextBoxPassword.TextEx.ReadChars();
                uint estimatedKeyStringBits = QualityEstimation.EstimatePasswordBits(passwordChars);

                return Convert.ToInt32(estimatedKeyStringBits * 100 / 128);
            }
        }
    }
}
