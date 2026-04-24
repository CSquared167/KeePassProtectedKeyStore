using KeePass;
using KeePassLib.Keys;
using KeePassLib.Utility;
using KeePassLib;
using KeePassProtectedKeyStore.Properties;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    internal static class ProtectedKeyStore
    {
        // Method to determine whether this database already has a protected key store authentication
        // method. Returns the IUserKey if one exists, or else it returns null.
        public static IUserKey FindProtectedKeyStoreInCompositeKey(CompositeKey compositeKey, out bool exclusive)
        {
            IUserKey result = compositeKey?.GetUserKey(typeof(KcpCustomKey)) is KcpCustomKey customKey
                    && customKey.Name.ToLower() == Helper.PluginName.ToLower() ?
                customKey :
                null;

            exclusive = result != null && compositeKey?.UserKeyCount == 1;
            return result;
        }

        // Method to determine whether a protected key store is present in the specified CompositeKey.
        public static bool IsProtectedKeyStoreInCompositeKey(CompositeKey compositeKey, out bool exclusive) =>
            FindProtectedKeyStoreInCompositeKey(compositeKey, out exclusive) != null;

        // Method to create a new protected key store, either as part of creating a new database or changing
        // the master key of an existing database. In such cases, the user will have selected the
        // "KeePassProtectedKeyStore" key provider as part of the new master key.
        public static bool CreateProtectedKeyStoreForNewMasterKey(string dbPath, byte[] pbData, bool exclusive)
        {
            // Create a protected key store and save the encrypted key in a file. If the file was saved
            // successfully and the user selected "Yes" when asked whether to create an emergency recovery
            // key file, proceed with creating it.
            bool result = CreateAndStoreProtectedKeyStore(dbPath, pbData);

            if (result && Helper.DisplayMessage(Resources.ProtectedKeyStoreCreatedNew,
                    Helper.PluginName,
                    string.Empty,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Asterisk) == DialogResult.Yes)
                EmergencyKeyRecoveryFile.CreateEmergencyRecoveryKeyFile(dbPath, pbData, exclusive);

            return result;
        }

        // Method to process a request to convert existing authentication key(s) to a protected
        // key store.
        public static bool ConvertToProtectedKeyStore(out string dbPath, out bool exclusive)
        {
            bool result = false;

            dbPath = string.Empty;
            exclusive = false;

            using (ProtectedKeyStoreTypeDialog dlg = new ProtectedKeyStoreTypeDialog())
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PwDatabase pd = Program.MainForm.ActiveDatabase;
                    byte[] pbData = GetSelectedCompositeKeyData(pd.MasterKey, out exclusive);

                    // If pbData == null, the user canceled the operation.
                    if (pbData != null)
                    {
                        // Get the database path associated with the protected key store. If the user chose the
                        // default protected key store, use the default database name.
                        dbPath = dlg.IndividualProtectedKeyStore ? pd.IOConnectionInfo.Path : Helper.DefaultProtectedKeyStoreName;

                        // Create a protected key store and save the encrypted key in a file. If the file was saved
                        // successfully and the user selected "Yes" when asked whether to create an emergency recovery
                        // key file, proceed with creating it.
                        result = CreateAndStoreProtectedKeyStore(dbPath, pbData);
                        if (result && Helper.DisplayMessage(Resources.ProtectedKeyStoreCreatedAfterConversion,
                                Helper.PluginName,
                                string.Empty,
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information) == DialogResult.Yes)
                            EmergencyKeyRecoveryFile.CreateEmergencyRecoveryKeyFile(dbPath, pbData, exclusive);

                        // Wipe out the unencrypted copy of the data.
                        MemUtil.ZeroArray(pbData);
                    }
                }

            return result;
        }

        // Method to get the user key for the selected authentication key(s). The method returns null
        // if the user cancels out of the dialog to select the authentication key(s). It also
        // returns whether the key data is exclusively a protected key store (i.e., all authentication
        // keys were converted to a protected key store).
        private static byte[] GetSelectedCompositeKeyData(CompositeKey compositeKey, out bool exclusive)
        {
            exclusive = false;

            byte[] pbData = null;
            using (VerifyAuthenticationKeyDlg dlg = new VerifyAuthenticationKeyDlg(compositeKey))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    pbData = dlg.SelectedUserKeyData;
                    exclusive = dlg.Exclusive;
                }
            }

            return pbData;
        }

        // Method to encrypt the specified byte data and save it in a file. The EncryptionEngine instance will
        // handle any exceptions and will return false if the operation did not complete.
        public static bool CreateAndStoreProtectedKeyStore(string dbPath, byte[] pbData, EncryptionEngine encryptionEngine = null) =>
            (encryptionEngine ?? EncryptionEngine.NewInstance).Encrypt(dbPath, pbData);

        // Method to return a protected key store for the specified database. The EncryptionEngine instance will
        // handle any exceptions and will return null if the operation did not complete.
        public static byte[] GetProtectedKeyStore(string dbPath, EncryptionEngine encryptionEngine = null) =>
            (encryptionEngine ?? EncryptionEngine.NewInstance).Decrypt(dbPath);
    }
}
