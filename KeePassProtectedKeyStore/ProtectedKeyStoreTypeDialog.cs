using KeePass.UI;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    public partial class ProtectedKeyStoreTypeDialog : Form
    {
        public bool IndividualProtectedKeyStore => RadioButtonIndividual.Checked;

        public ProtectedKeyStoreTypeDialog()
        {
            InitializeComponent();
        }

        // Event handler when the form has loaded.
        private void ProtectedKeyStoreTypeDialog_Load(object sender, System.EventArgs e)
        {
            GlobalWindowManager.AddWindow(this);
        }

        // Event handler when the form has closed.
        private void ProtectedKeyStoreTypeDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }
    }
}
