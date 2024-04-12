using KeePassLib.Keys;
using System;

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
        public override string Name => KeePassProtectedKeyStoreExt.PluginName;

        // Overridden method to return the protected key store.
        public override byte[] GetKey(KeyProviderQueryContext ctx)
        {
            byte[] pbData;

            if (ctx.CreatingNewKey)
            {
                // If creating a new key, generate a random sequence of binary data. Because this key
                // will not be known to the user, it will be important for the user to create an
                // emergency key recovery file, in case this protected key store is lost.
                pbData = new byte[NewKeyLength];
                new Random().NextBytes(pbData);
            }
            else
                pbData = ProtectedKeyStore.GetProtectedKeyStoreForDatabase(ctx.DatabasePath);

            return pbData;
        }
    }
}
