using KeePass;
using KeePassLib.Keys;
using KeePassLib.Utility;
using KeePassLib;
using KeePassProtectedKeyStore.Properties;
using System;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    internal static class ProtectedKeyStore
    {
        // Additional entropy to increase the complexity of the encryption.
        private static byte[] Entropy { get; } = new byte[]
        {
            0xF7, 0x72, 0x93, 0x27, 0x62, 0xAF, 0x4F, 0x2B,
            0x87, 0x32, 0xE8, 0x0B, 0x92, 0x33, 0x0A, 0x06
        };

        // Method to determine whether this database already has a protected key store authentication
        // method. Returns the IUserKey if one exists, or else it returns null.
        public static IUserKey FindProtectedKeyStore(CompositeKey compositeKey, out bool exclusive)
        {
            IUserKey result = compositeKey?.GetUserKey(typeof(KcpCustomKey)) is KcpCustomKey customKey
                    && customKey.Name.ToLower() == KeePassProtectedKeyStoreExt.PluginName.ToLower() ?
                customKey :
                null;

            exclusive = compositeKey?.UserKeyCount == 1;
            return result;
        }

        // Method to determine whether a protected key store is present in the specified CompositeKey.
        public static bool HasProtectedKeyStore(CompositeKey compositeKey, out bool exclusive) =>
            FindProtectedKeyStore(compositeKey, out exclusive) != null;

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
                    KeePassProtectedKeyStoreExt.PluginName,
                    string.Empty,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Asterisk) == DialogResult.Yes)
                EmergencyKeyRecoveryFile.CreateEmergencyRecoveryKeyFile(dbPath, pbData, exclusive);

            return result;
        }

        // Method to process a request to convert existing authentication key(s) to a protected
        // key store.
        public static bool ConvertToProtectedKeyStore(out bool exclusive)
        {
            PwDatabase pd = Program.MainForm.ActiveDatabase;
            string dbPath = pd.IOConnectionInfo.Path;
            byte[] pbData = GetSelectedCompositeKeyData(pd.MasterKey, out exclusive);
            bool result = false;

            // If pbData == null, the user canceled the operation.
            if (pbData != null)
            {
                // Create a protected key store and save the encrypted key in a file. If the file was saved
                // successfully and the user selected "Yes" when asked whether to create an emergency recovery
                // key file, proceed with creating it.
                result = CreateAndStoreProtectedKeyStore(dbPath, pbData);
                if (result && Helper.DisplayMessage(Resources.ProtectedKeyStoreCreatedAfterConversion,
                        KeePassProtectedKeyStoreExt.PluginName,
                        string.Empty,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Asterisk) == DialogResult.Yes)
                    EmergencyKeyRecoveryFile.CreateEmergencyRecoveryKeyFile(dbPath, pbData, exclusive);

                // Wipe out the unencrypted copy of the data.
                MemUtil.ZeroArray(pbData);
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

        // Method to encrypt the specified byte data and save it in a file. ProtectedData.Protect will throw an
        // exception if anything goes wrong (e.g., encryption failed, OS does not support this method, etc.),
        // which means AppDataStore.SetProtectedKeyStore will not be called.
        public static bool CreateAndStoreProtectedKeyStore(string dbPath, byte[] pbData)
        {
            bool result = false;

            try
            {
                // Utilize the computer's Trusted Platform Module (TPM) to encrypt the data. The encrypted key is
                // valid only for the currently logged-in user and will not be valid if the user's computer is
                // rebuilt or if the user purchases a new computer. In such cases, the user should have previously
                // created an emergency key recovery file and can import it if this key is no longer valid.
                byte[] pbProtectedKey = ProtectedData.Protect(pbData, Entropy, DataProtectionScope.CurrentUser);

                // Attempt to save the protected key store in a file.
                result = AppDataStore.SetProtectedKeyStore(dbPath, pbProtectedKey);
            }
            catch (Exception exc)
            {
                // In these cases, exc.Message will be sufficient to display to the user.
                Helper.DisplayMessage(Resources.ProtectedKeyStoreCreateError, exc.Message);
            }

            return result;
        }

        // Method to return a protected key store for the specified database. AppDataStore.GetProtectedKeyStore
        // will display a popup to the user if it cannot find a key file. ProtectedData.Unprotect will throw
        // an exception if a decryption error occurs, causing the caught exception to display a popup and for
        // this method to return null. KeePass assumes we will notify the user of any errors.
        public static byte[] GetProtectedKeyStoreForDatabase(string dbPath)
        {
            byte[] pbProtectedKey = AppDataStore.GetProtectedKeyStore(dbPath);
            byte[] pbData = null;

            try
            {
                if (pbProtectedKey != null)
                    pbData = ProtectedData.Unprotect(pbProtectedKey, Entropy, DataProtectionScope.CurrentUser);
            }
            catch (Exception exc)
            {
                Helper.DisplayMessage(Resources.ProtectedKeyStoreDecryptError, exc.Message);
            }

            return pbData;
        }
    }
}
