using System;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    public partial class HiddenDlg : Form
    {
        public HiddenDlg()
        {
            InitializeComponent();
        }

        private void HiddenDlg_Load(object sender, EventArgs e)
        {
            // Move the dialog outside of the desktop, effectively making it "hidden".
            SetDesktopLocation(-Size.Width, -Size.Height);
        }
    }
}
