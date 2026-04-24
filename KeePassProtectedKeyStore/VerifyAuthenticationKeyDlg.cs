using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Utility;
using KeePassProtectedKeyStore.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    public partial class VerifyAuthenticationKeyDlg : Form
    {
        // The CompositeKey instance for this dialog, as passed into the constructor.
        private CompositeKey CompositeKeyInstance { get; } = null;

        // Dictionary containing the list box item index as the key and the IUserKey
        // corresponding to the item index as the value.
        private Dictionary<int, IUserKey> UserKeyDictionary { get; } = new Dictionary<int, IUserKey>();

        private int KeyFileProviderListBoxIndex { get; set; } = -1;

        private bool KeyFileProviderWarningGiven { get; set; } = false;

        // Returns the key data based on the verification key(s) selected by the user. Algorithm
        // based on KeePass source code.
        public byte[] SelectedUserKeyData
        {
            get
            {
                // Enumerate through the selected indices and build an iterator including all valid
                // KeyData elements for the selected IUserKeys.
                IEnumerable<ProtectedBinary> selectedUserKeys = ListBoxUserKeys
                    .SelectedIndices
                    .Cast<int>()
                    .Select(idx => UserKeyDictionary[idx].KeyData)
                    .Where(pb => pb != null);

                // Sum up the lengths of the individual KeyData elements to determine the overall length.
                int userKeyDataLength = selectedUserKeys
                    .Sum(pb => (int)pb.Length);

                // Allocate a byte array large enough to hold all of the KeyData buffers.
                byte[] pbData = new byte[userKeyDataLength];
                int offset = 0;

                foreach (ProtectedBinary keyData in selectedUserKeys)
                {
                    // Get an unencrypted copy of the IUserKey's KeyData, copy it to the output array,
                    // update the offset into the output array, and wipe out the unencrypted key data.
                    byte[] pbKeyData = keyData.ReadData();

                    Array.Copy(pbKeyData, 0, pbData, offset, pbKeyData.Length);
                    offset += pbKeyData.Length;
                    MemUtil.ZeroArray(pbKeyData);
                }

                return pbData;
            }
        }

        // Property to indicate whether all existing authentication keys are being converted to a
        // protected key store.
        public bool Exclusive =>
            ListBoxUserKeys.SelectedIndices.Count == ListBoxUserKeys.Items.Count;

        public VerifyAuthenticationKeyDlg(CompositeKey compositeKey)
        {
            InitializeComponent();

            CompositeKeyInstance = compositeKey;
        }

        // Event handler for when the dialog is loaded.
        private void SelectUserKeyDlg_Load(object sender, EventArgs e)
        {
            // For each IUserKey in CompositeKeyInstance, add a descriptive name for the key in
            // the list box and add the list box index/IUserKey to UserKeyDictionary.
            foreach (IUserKey userKey in CompositeKeyInstance.UserKeys)
            {
                string displayText = GetUserKeyTypeDescription(userKey);
                int idx = ListBoxUserKeys.Items.Add(displayText);

                UserKeyDictionary[idx] = userKey;

                if (userKey is KcpKeyFile || userKey is KcpCustomKey)
                {
                    ListBoxUserKeys.SelectedIndex = idx;
                    KeyFileProviderListBoxIndex = idx;
                }
            }

            // If the list box contains only one item, select it automatically and enable the
            // OK button. If more than one list box item exists, we will leave it to the user
            // to select the one to be converted to a protected key store.
            if (CompositeKeyInstance.UserKeyCount == 1)
            {
                ListBoxUserKeys.SelectedIndex = 0;
                ButtonOK.Enabled = true;
            }
        }

        // Event handler for when the list box selected item changes. Enable/disable the OK button
        // depending on whether an item in the list box has been selected.
        private void ListBoxUserKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (KeyFileProviderListBoxIndex >= 0 && !ListBoxUserKeys.SelectedIndices.Contains(KeyFileProviderListBoxIndex))
            {
                if (!KeyFileProviderWarningGiven)
                {
                    Helper.DisplayMessage(Resources.KeyFileProviderWarning,
                        GetUserKeyTypeDescription(UserKeyDictionary[KeyFileProviderListBoxIndex]),
                        Helper.PluginName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    KeyFileProviderWarningGiven = true;
                }

                ListBoxUserKeys.SelectedIndices.Add(KeyFileProviderListBoxIndex);
            }

            ButtonOK.Enabled = ListBoxUserKeys.SelectedIndices.Count >= 0;
        }
            
        // Method to return a text string describing the specified IUserKey type.
        private static string GetUserKeyTypeDescription(IUserKey userKey)
        {
            string displayText;

            if (userKey is KcpPassword)
                displayText = "Master password";
            else if (userKey is KcpKeyFile file)
                displayText = string.Format("Key file ({0})", file.Path);
            else if (userKey is KcpCustomKey key)
                displayText = string.Format("Key provider ({0})", key.Name);
            else if (userKey is KcpUserAccount)
                displayText = "Windows user account";
            else
                displayText = string.Format("Unknown authentication key ({0})", userKey.GetType().ToString());

            return displayText;
        }
    }
}
