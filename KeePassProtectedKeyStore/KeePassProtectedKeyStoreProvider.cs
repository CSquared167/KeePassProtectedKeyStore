using KeePassLib.Keys;
using System;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    public sealed class KeePassProtectedKeyStoreProvider : KeyProvider
    {
        // Number of bytes to allocate for new key.
        private static int NewKeyLength => 256;

        // Overridden property, set to "true" to indicate the returned key should be used as-is. When a key
        // provider returns false for this property, the caller of GetKey() computes a SHA256 hash of the data
        // before attempting to use it. In this case, our key was already hashed, so KeePass does not need to
        // do any additional processing. 
        public override bool DirectKey => true;

        // Overridden property, set to "true" to indicate GetKey() might display a popup.
        public override bool GetKeyMightShowGui => true;

        // Overridden property to return the name of the key provider. This name is displayed in the combo
        // box associated with the "Key file/provider" authentication key.
        public override string Name => Helper.PluginName;

        // Overridden method to return the protected key store.
        public override byte[] GetKey(KeyProviderQueryContext ctx)
        {
            byte[] pbData = null;

            // Initialize the helper variables.
            Helper.CreateNewKeyRequestingDefaultKey = false;
            Helper.CreateNewKeyUsingExistingKey = false;

            if (ctx.CreatingNewKey)
            {
                bool createNewKey = false;

                // Prompt the user whether to use the default protected key store or an individual one.
                using (ProtectedKeyStoreTypeDialog dlg = new ProtectedKeyStoreTypeDialog())
                {
                    createNewKey = dlg.ShowDialog() == DialogResult.OK;
                    if (createNewKey)
                    {
                        // Set the helper variables to whether the user wants to use the default protected
                        // key store, and whether a protected key store already exists. Attempt to get an
                        // existing key based on the user's preferences.
                        Helper.CreateNewKeyRequestingDefaultKey = !dlg.IndividualProtectedKeyStore;
                        pbData = ProtectedKeyStore.GetProtectedKeyStore(Helper.CreateNewKeyRequestingDefaultKey ?
                            Helper.DefaultProtectedKeyStoreName :
                            ctx.DatabasePath);
                        Helper.CreateNewKeyUsingExistingKey = pbData != null;
                    }
                }

                // If createNewKey is false, the user canceled out of the dialog.  If pbData is null, generate
                // a random sequence of binary data. Because this key will not be known to the user, it will
                // be important for the user to create an emergency key recovery file, in case the protected
                // key store is lost.
                if (createNewKey && pbData == null)
                {
                    pbData = new byte[NewKeyLength];
                    new Random().NextBytes(pbData);
                }
            }
            else
            {
                // Attempt to get the key specifically for the specified database, and attempt to get the
                // default key if a database-specific key isn't found. If neither is found, null will be
                // returned. This will happen in cases where the user specifies this plugin when entering
                // the master key, but a protected user key never existed for this database.
                Helper.OpenExistingKeyUsingDefaultKey = false;
                pbData = ProtectedKeyStore.GetProtectedKeyStore(ctx.DatabasePath);
                if (pbData == null)
                {
                    pbData = ProtectedKeyStore.GetProtectedKeyStore(Helper.DefaultProtectedKeyStoreName);
                    Helper.OpenExistingKeyUsingDefaultKey = pbData != null;
                }
            }

            return pbData;
        }
    }
}
