using KeePass.UI;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    public partial class EncryptionMethodDialog : Form
    {
        public bool UserEncryption => RadioButtonUserProtect.Checked;

        public EncryptionMethodDialog()
        {
            InitializeComponent();
        }

        // Event handler when the form has loaded.
        private void EncryptionMethodDialog_Load(object sender, System.EventArgs e)
        {
            GlobalWindowManager.AddWindow(this);
        }

        // Event handler when the form has closed.
        private void EncryptionMethodDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }
    }
}
