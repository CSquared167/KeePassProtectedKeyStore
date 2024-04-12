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
    }
}
