using KeePass;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using KeePassLib.Utility;
using System;
using System.Reflection;
using System.Windows.Forms;


namespace KeePassProtectedKeyStore
{
    // This plugin uses Windows System.Security.Cryptography.ProtectedData methods to create protected key
    // stores. The cryptography methods use the computer's Trusted Platform Module (TPM) to encrypt and decrypt
    // the data. The encrypted files are stored in the AppData folder under a subfolder created by this plugin.
    //
    // The following functionality is provided:
    // - If the user has a database already open, this plugin can convert one or more existing authentication
    //   keys (master password, Windows user account, and/or a key/file provider) to a protected key store.
    //   After that, the "KeePassProtectedKeyStore" option under the "Key/file provider" authentication key
    //   can be used in place of the converted authentication key(s) that was/were converted.
    // - If a database is open that has a protected key store, the user can create an emergency key recovery
    //   file, in case the protected key store file is no longer available (e.g., the user purchases a new
    //   computer, needs to rebuild the computer, etc.).
    // - Should the protected key store file no longer be available, the user can import the emergency recovery
    //   key file to recreate the protected key store file.
    // - If the user has logged into a database for which a protected key store is the only authentication
    //   key, the user can choose to open the database and login automatically. This can occur under different
    //   circumstances, including when first launching KeePass, when attempting to reopen the database after it
    //   has been locked, when using the "File/Open" method to open a database, etc. The functionality was inspired
    //   by Jeremy Bourgin's KeePassAutoUnlock plugin (https://github.com/jeremy-bourgin/KeePassAutoUnlock).
    // - If the user creates a new database or changes the master key for an existing database, and the new
    //   master key includes the KeePassProtectedKeyStore key provider, this plugin will generate a new random
    //   key. In these cases it will be much more important for the user to create an emergency key recovery
    //   file, because the key will be unknown to the user.
    //
    // Once the TPM encrypts the data, the database can be opened only by the the same Windows user. The data
    // file cannot be copied to another computer, and it will no longer work if the Windows user account is
    // recreated. Unless the user knows the original authentication values ("Master password" being an example
    // of a known value, and "Windows user account" being an example of an unknown value), it is highly
    // recommended for the user to create an emergency key recovery file.
    //
    // If a shared database is being used (e.g., the database resides on a NAS filesystem and is accessed by
    // multiple computers), this plugin can be used on each computer to generate a protected key store specific to
    // that Windows user and computer. If an emergency key recovery file is created, it does not need to be created
    // more than once, and it can be imported on any other computer that requires it.
    //
    // It should be noted this plugin cannot be used in addition to the converted authentication key(s), only
    // instead of them. If the original authentication keys are used in addition to the KeePassProtectedKeyStore,
    // KeePass will consider them to be separate keys and will attempt to authenticate the database against both
    // keys, which will fail.

    public sealed class KeePassProtectedKeyStoreExt : Plugin
    {
        // Plugin name, to be used for display and other purposes.
        public static string PluginName { get; } = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

        // Key provider instance.
        private KeePassProtectedKeyStoreProvider KeyProviderInstance { get; } = new KeePassProtectedKeyStoreProvider();

        // PluginHost property, to hold the IPluginHost instance KeePass passes in from the
        // Initialize method.
        private IPluginHost PluginHost { get; set; } = null;

        // Overridden UpdateUrl property, to allow KeePass to notify users if a newer version
        // of this plugin exists.
        public override string UpdateUrl =>
            "https://raw.githubusercontent.com/CSquared167/KeePassProtectedKeyStore/master/VersionInfo.txt";

        // Overridden method to initialize the plugin.
        public override bool Initialize(IPluginHost host)
        {
            // Save the IPluginHost instance and add our key provider instance to the IPluginHost's
            // KeyProviderPool.
            PluginHost = host;
            PluginHost.KeyProviderPool?.Add(KeyProviderInstance);

            // Add event handlers with KeePass' GlobalWindowManager's WindowAdded and WindowRemoved
            // events.
            GlobalWindowManager.WindowAdded += GlobalWindowManager_WindowAdded;
            GlobalWindowManager.WindowRemoved += GlobalWindowManager_WindowRemoved;

            return PluginHost != null;
        }

        // Overridden method to terminate the plugin. Remove all event handlers, and remove our key
        // provider instance from the IPluginHost's KeyProviderPool.
        public override void Terminate()
        {
            GlobalWindowManager.WindowAdded -= GlobalWindowManager_WindowAdded;
            GlobalWindowManager.WindowRemoved += GlobalWindowManager_WindowRemoved;
            PluginHost.KeyProviderPool?.Remove(KeyProviderInstance);

            base.Terminate();
        }

        // GlobalWindowManager's WindowAdded event handler.
        private void GlobalWindowManager_WindowAdded(object sender, GwmWindowEventArgs e)
        {
            if (e.Form is KeyPromptForm keyPromptForm)
            {
                // When the KeyPromptForm is being invoked, use reflection to get the instance's
                // private IOConnectionInfo member variable. This form will be invoked under
                // several circumstances, including when first launching KeePass, when attempting
                // to reopen the database after it has been locked, when using the "File/Open"
                // method to open a database, etc.
                FieldInfo fieldInfo = typeof(KeyPromptForm).GetField("m_ioInfo", BindingFlags.Instance | BindingFlags.NonPublic);
                IOConnectionInfo connectionInfo = fieldInfo?.GetValue(keyPromptForm) as IOConnectionInfo;

                // If the user has enabled this database for auto-login, complete the login, move
                // the form off the screen and simulate a click of the Cancel button. If the form
                // is not moved off the screen, it will briefly flicker on the screen before
                // disappearing.
                if (PerformAutoLogin(connectionInfo))
                {
                    keyPromptForm.SetDesktopLocation(-keyPromptForm.Size.Width, -keyPromptForm.Size.Height);
                    keyPromptForm.CancelButton.PerformClick();
                }
            }
        }

        // GlobalWindowManager's WindowRemoved event handler.
        private void GlobalWindowManager_WindowRemoved(object sender, GwmWindowEventArgs e)
        {
            if (e.Form is KeyPromptForm keyPromptForm)
            {
                // When the KeyPromptForm is being disposed, use reflection to get the instance's
                // private IOConnectionInfo member variable. Also get the instance's public
                // CompositeKey member variable.
                FieldInfo fieldInfo = typeof(KeyPromptForm).GetField("m_ioInfo", BindingFlags.Instance | BindingFlags.NonPublic);
                IOConnectionInfo connectionInfo = fieldInfo?.GetValue(keyPromptForm) as IOConnectionInfo;
                CompositeKey compositeKey = keyPromptForm.CompositeKey;

                // This is for a corner case that is not likely to happen, but it is here in case
                // it does happen. If the CompositeKey consists exclusively of a protected key store,
                // add it to the plugin configuration if it is not already there. This situation
                // could occur if the plugin configuration file was deleted or became corrupted,
                // and the user opens the database by selecting the KeePassProtectedKeyStore
                // authentication option manually. By adding it to the plugin configuration, the
                // user can then configure the database for for auto-login.
                if (!string.IsNullOrEmpty(connectionInfo?.Path) &&
                        ProtectedKeyStore.HasProtectedKeyStore(compositeKey, out bool exclusive) && exclusive)
                    PluginConfiguration.AddAutoLogin(connectionInfo.Path);
            }
            else if (e.Form is KeyCreationForm keyCreationForm)
            {
                // When the KeyCreationForm is being disposed, use reflection to get the instance's
                // private IOConnectionInfo member variable. Also get the instance's public
                // CompositeKey member variable and check whether it includes a protected key store.
                FieldInfo fieldInfo = typeof(KeyCreationForm).GetField("m_ioInfo", BindingFlags.Instance | BindingFlags.NonPublic);
                string dbPath = (fieldInfo?.GetValue(keyCreationForm) as IOConnectionInfo)?.Path;
                CompositeKey compositeKey = keyCreationForm.CompositeKey;
                IUserKey userKey = ProtectedKeyStore.FindProtectedKeyStore(compositeKey, out bool exclusive);

                if (!string.IsNullOrEmpty(dbPath) && userKey != null)
                {
                    // If the new composite key includes a protected key store, create a new encrypted protected
                    // key store. If it is the only authentication key, add it to the plugin configuration as an
                    // auto-login database.
                    byte[] pbData = userKey.KeyData.ReadData();

                    if (ProtectedKeyStore.CreateProtectedKeyStoreForNewMasterKey(dbPath, pbData, exclusive) && exclusive)
                        PluginConfiguration.AddAutoLogin(dbPath);

                    // Zero-out the unencrypted key data buffer.
                    MemUtil.ZeroArray(pbData);
                }
            }
        }

        // Method to perform an auto-login of the database specified in the IConnectionInfo instance.
        private bool PerformAutoLogin(IOConnectionInfo connectionInfo)
        {
            bool success = false;

            // Check whether auto-login is set and enabled for this database.
            if (PluginConfiguration.Instance.IsAutoLoginSet(connectionInfo?.Path, out bool autoLoginEnabled) && autoLoginEnabled)
            {
                byte[] pbData = null;

                try
                {
                    // Create a KeyProviderQueryContext variable and call our provider class to get
                    // the protected key store.
                    KeyProviderQueryContext ctx = new KeyProviderQueryContext(connectionInfo, false, false);

                    pbData = KeyProviderInstance.GetKey(ctx);
                    if (pbData != null)
                    {
                        // If getting the key was successful, create a new CompositeKey instance
                        // and use it to open the database.
                        CompositeKey compositeKey = new CompositeKey();

                        compositeKey.AddUserKey(new KcpCustomKey(PluginName, pbData, false));
                        PluginHost.Database.Open(connectionInfo, compositeKey, null);

                        PwDocument ds = Program.MainForm.DocumentManager.ActiveDocument;
                        PwDatabase pd = ds.Database;

                        // Check whether the database was successfully opened. This could happen in
                        // some situations, for example if the user had a database selected for auto-
                        // login and subsequently changed the master key. The plugin configuration
                        // will still have the database flagged for auto-login, but that is okay.
                        // This method will return false, allowing KeePass to display the KeyPromptForm.
                        if (pd.IsOpen)
                        {
                            // Check whether a last selected group exists. If the user creates one or
                            // more password groups, selects one of them and eventually exits KeePass,
                            // KeePass will navigate the tree control to the last selected group the
                            // next time the user opens the database. We need to obtain the last
                            // selected group here, or else KeePass will display the root folder of
                            // the database.
                            PwGroup pgSelect = !pd.LastSelectedGroup.Equals(PwUuid.Zero) ?
                                pd.RootGroup.FindGroup(pd.LastSelectedGroup, true) :
                                null;

                            // If KeePass is being opened after the database has been locked, reset
                            // the LockedIoc variable with an empty database path.
                            if (ds.LockedIoc.Path.Length > 0)
                                ds.LockedIoc = new IOConnectionInfo();

                            // Update KeePass' UI, telling it to navigate to the last selected group.
                            PluginHost.MainWindow.UpdateUI(false, null, true, pgSelect, true, pgSelect, false);

                            success = true;
                        }
                    }
                }
                catch
                {
                    // Do not notify the user of any errors. Simply return false and have KeePass
                    // display the KeyPromptForm. If anything above throws an exception, it is likely
                    // the manual login will end up throwing the same exception, and KeePass itself
                    // can display a meaningful message to the user.
                }

                // Because pbData contains the unencrypted key, we need to clear the array so it does
                // not persist in memory.
                if (pbData != null)
                    MemUtil.ZeroArray(pbData);
            }

            return success;
        }

        // Overridden method to return the menu item/subitems for our plugin.
        public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
        {
            ToolStripMenuItem tsmi = t == PluginMenuType.Main ? new ToolStripMenuItem() : null;

            // Do not add a menu item if the PluginMenuType is not PluginMenuType.Main, or if
            // a memory error occcurs allocating a new ToolStripMenuItem instance.
            if (tsmi != null)
            {
                tsmi.Text = string.Format("{0} Options...", PluginName);
                tsmi.Click += OnMenuItemClick;
            }

            return tsmi;
        }

        // Menu item click handler. Show the Options dialog.
        private void OnMenuItemClick(object sender, EventArgs e) =>
            new OptionsDlg().ShowDialog();
    }
}
