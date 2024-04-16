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
    }
}
