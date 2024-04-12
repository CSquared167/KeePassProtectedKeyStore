using KeePass;
using KeePassLib.Keys;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using KeePassLib.Utility;
using System.Diagnostics;

namespace KeePassProtectedKeyStore
{
    public partial class OptionsDlg : Form
    {
        public OptionsDlg()
        {
            InitializeComponent();

            PwDatabase pd = Program.MainForm.ActiveDatabase;
            string dbPath = pd?.IOConnectionInfo?.Path ?? string.Empty;
            CompositeKey compositeKey = pd?.MasterKey;
            uint userKeyCount = compositeKey?.UserKeyCount ?? 0;
            bool existingProtectedKeyStore = ProtectedKeyStore.HasProtectedKeyStore(compositeKey, out bool exclusive);

            // Enable ButtonConvert only if a database is already open and either does not already have a
            // protected key store or has additional authentication keys.
            ButtonConvert.Enabled = !string.IsNullOrEmpty(dbPath) && userKeyCount > 0 && (!existingProtectedKeyStore || !exclusive);

            // Enable ButtonCreateEmergencyFile only if a database has a protected key store.
            ButtonCreateEmergencyFile.Enabled = existingProtectedKeyStore;

            // Set the checkbox (or not) to indicate whether auto-login is enabled by default.
            CheckBoxAutoLoginByDefault.Checked = PluginConfiguration.Instance.AutoLoginByDefault;

            // Setting CheckBoxAutoLoginByDefault.Checked fires an "CheckedChanged" event. Because we are
            // initializing the checkbox and not updating it, the "CheckedChanged" event handler is enabled
            // now rather than at design time, so we don't end up updating/saving the configuration file
            // needlessly.
            CheckBoxAutoLoginByDefault.CheckedChanged += CheckBoxAutoLoginByDefault_CheckedChanged;

            // Populate the auto-login listbox.
            PopulateAutoLoginsListBox();
        }

        // Handler for when the user requests to convert existing authentication key(s) to a protected
        // key store.
        private void ButtonConvert_Click(object sender, EventArgs e)
        {
            if (ProtectedKeyStore.ConvertToProtectedKeyStore(out bool exclusive) && exclusive)
                AddAutoLogin(Program.MainForm.ActiveDatabase.IOConnectionInfo.Path);
        }

        // Handler for when the user requests to create an emergency key recovery file.
        private void ButtonCreateEmergencyFile_Click(object sender, EventArgs e)
        {
            PwDatabase pd = Program.MainForm.ActiveDatabase;
            CompositeKey compositeKey = pd.MasterKey;
            byte[] pbData = ProtectedKeyStore.FindProtectedKeyStore(compositeKey, out bool exclusive).KeyData.ReadData();

            EmergencyKeyRecoveryFile.CreateEmergencyRecoveryKeyFile(pd.IOConnectionInfo.Path, pbData, exclusive);
            MemUtil.ZeroArray(pbData);
        }

        // Handler for when the user requests to import an emergency key recovery file.
        private void ButtonImportEmergencyFile_Click(object sender, EventArgs e)
        {
            if (EmergencyKeyRecoveryFile.ImportEmergencyRecoveryKeyFile(out string dbPath, out bool exclusive) && exclusive)
                AddAutoLogin(dbPath);
        }

        // Handler for when the user changes the checkbox to indicate whether auto-login
        // is enabled by default when a protected key store is created.
        private void CheckBoxAutoLoginByDefault_CheckedChanged(object sender, EventArgs e) =>
            PluginConfiguration.Instance.AutoLoginByDefault = CheckBoxAutoLoginByDefault.Checked;

        // Handler for when the user clicks the Help button.
        private void ButtonHelp_Click(object sender, EventArgs e) =>
            Process.Start("https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/README.md#options");

        // Handler for when the user checks/unchecks an item in the auto-login list box.
        private void CheckedListBoxAutoLogin_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Dictionary<string, bool> autoLoginMap = PluginConfiguration.Instance.AutoLoginMap;

            autoLoginMap[Convert.ToString(CheckedListBoxAutoLogin.Items[e.Index])] = e.NewValue == CheckState.Checked;
            PluginConfiguration.Instance.AutoLoginMap = autoLoginMap;
        }

        // Method to populate the auto-logins list box.
        private void PopulateAutoLoginsListBox()
        {
            Dictionary<string, bool> autoLoginMap = PluginConfiguration.Instance.AutoLoginMap;

            // SetItemChecked fires an "ItemCheck" event. Because we are initializing the items in the
            // list box and not updating them, we need to turn off the "ItemCheck" event handler so we
            // don't end up updating/saving the configuration file needlessly.
            CheckedListBoxAutoLogin.ItemCheck -= CheckedListBoxAutoLogin_ItemCheck;

            CheckedListBoxAutoLogin.Items.Clear();
            foreach (string dbPathAutoLogin in autoLoginMap.Keys)
            {
                int idx = CheckedListBoxAutoLogin.Items.Add(dbPathAutoLogin);

                CheckedListBoxAutoLogin.SetItemChecked(idx, autoLoginMap[dbPathAutoLogin]);
            }

            // Start handling "ItemCheck" events again.
            CheckedListBoxAutoLogin.ItemCheck += CheckedListBoxAutoLogin_ItemCheck;
        }

        // Method to add a new auto-login database to the configuration file and to the auto-login
        // list box.
        private void AddAutoLogin(string dbPath)
        {
            if (PluginConfiguration.AddAutoLogin(dbPath))
                PopulateAutoLoginsListBox();
        }
    }
}
