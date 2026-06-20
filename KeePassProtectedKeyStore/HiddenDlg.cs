using KeePass.UI;
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

        // Event handler when the form has loaded.
        private void HiddenDlg_Load(object sender, EventArgs e)
        {
            GlobalWindowManager.AddWindow(this);

            // Move the dialog outside of the desktop, effectively making it "hidden".
            SetDesktopLocation(-Size.Width, -Size.Height);
        }

        // Event handler when the form has closed.
        private void HiddenDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }
    }
}
