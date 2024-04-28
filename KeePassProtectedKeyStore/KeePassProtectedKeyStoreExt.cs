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
    // This plugin uses Windows Cryptography methods to create protected key stores. The cryptography methods
    // use the computer's Trusted Platform Module (TPM) to encrypt and decrypt the data. The encrypted files
    // are stored in the AppData folder under a subfolder created by this plugin. The user has the choice to
    // encrypt/decrypt the protected key store via either the Data Protection API (DPAPI) or Windows Hello,
    // depending on the user's desired level of security.
    //
    // The following functionality is provided:
    // - If the user has a database already open, this plugin can convert one or more existing authentication
    //   keys (master password, Windows user account, and/or a key/file provider) to a protected key store.
    //   After that, the "KeePassProtectedKeyStore" option under the "Key/file provider" authentication key
    //   can be used in place of the converted authentication key(s) that was/were converted. If Windows Hello
    //   is chosen as the encryption method, a Windows Hello prompt will be displayed whenever a protected key
    //   store is being accessed.
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
    //
    // Version 1.1.0 changes:
    // - Added support for default protected key store.
    // - If the user attempts to create a protected key store that already exists and contains the same key, the
    //   plugin will simply report this fact and will not overwrite it.
    // - If the user attempts to create a protected key store that already exists and contains a different key,
    //   the plugin will still ask whether to overwrite it but will give a warning about the potential consequences.
    // - Corrected an issue where if the master key is changed and no longer uses a protected key store, the
    //   protected key store and auto-login entries were not being removed.
    // - Corrected an issue in the plugin's Terminate method, to remove the "WindowRemoved" event handler correctly.
    // - Minor code refactoring not impacting functionality.
    //
    // Version 1.2.0 changes:
    // - Added support for Windows Hello encryption/decryption.
    // - Changed PluginConfiguration class auto-login methods from static to non-static.
    //
    // Version 1.2.1 changes:
    // - Corrected an issue where attempting to access files in the AppData folder would throw an error if the
    //   folder did not already exist (e.g., when loading the plugin for the first time and attempting to import
    //   an emergency key recovery file, an error would be displayed that the AppData folder did not exist).

    public sealed class KeePassProtectedKeyStoreExt : Plugin
    {
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
            GlobalWindowManager.WindowRemoved -= GlobalWindowManager_WindowRemoved;
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
                // and the protected key store is not the default key, add it to the plugin configuration
                // if it is not already there. This situation could occur if the plugin configuration file
                // was deleted or became corrupted, and the user opens the database by selecting the
                // KeePassProtectedKeyStore authentication option manually. By adding it to the plugin
                // configuration, the user can then configure it for for auto-login.
                if (!string.IsNullOrEmpty(connectionInfo?.Path) &&
                        !Helper.OpenExistingKeyUsingDefaultKey &&
                        ProtectedKeyStore.IsProtectedKeyStoreInCompositeKey(compositeKey, out bool exclusive) && exclusive)
                    PluginConfiguration.Instance.AddAutoLogin(connectionInfo.Path);

                // Reset helper variable.
                Helper.OpenExistingKeyUsingDefaultKey = false;
            }
            else if (e.Form is KeyCreationForm keyCreationForm)
            {
                // This code will be entered when the KeyCreationForm is being disposed. Use reflection to get
                // the instance's private IOConnectionInfo member variable.
                FieldInfo fieldInfo = typeof(KeyCreationForm).GetField("m_ioInfo", BindingFlags.Instance | BindingFlags.NonPublic);

                // Attempt to get the database path from the KeyCreationForm's IOConnectionInfo. If it is
                // null/empty, the user canceled out of the key creation form.
                string dbPathFromFieldInfo = (fieldInfo?.GetValue(keyCreationForm) as IOConnectionInfo)?.Path;

                if (!string.IsNullOrEmpty(dbPathFromFieldInfo))
                {
                    // Get the instance's public CompositeKey member variable and check whether it includes a
                    // protected key store.
                    CompositeKey compositeKey = keyCreationForm.CompositeKey;
                    IUserKey userKey = ProtectedKeyStore.FindProtectedKeyStoreInCompositeKey(compositeKey, out bool exclusive);
                    PluginConfiguration pluginConfiguration = PluginConfiguration.Instance;

                    // Initial criterion for auto-login is that a protected key store must be the only
                    // authentication method.
                    bool addAutoLogin = exclusive;

                    // Get the correct database path to associate with the protected key store, depending on
                    // whether the user requested to use the default key when creating the master key.
                    string dbPath = Helper.CreateNewKeyRequestingDefaultKey ?
                        Helper.DefaultProtectedKeyStoreName :
                        dbPathFromFieldInfo;

                    // Remove the auto-login entry for this database from the plugin configuration. It will be
                    // added back down below if required.
                    pluginConfiguration.RemoveAutoLogin(dbPathFromFieldInfo);

                    // If the new master key doesn't contain a protected key store, or if the user requested to
                    // use the default key, delete the protected key store file associated with this database
                    // (if it exists).
                    if (userKey == null || Helper.CreateNewKeyRequestingDefaultKey)
                        EncryptionEngine.NewInstance.DeleteProtectedKeyStoreFiles(new string[] { dbPathFromFieldInfo });

                    if (!Helper.CreateNewKeyUsingExistingKey && userKey != null)
                    {
                        byte[] pbData = userKey.KeyData.ReadData();

                        // Attempt to create the new protected key store. Do not create an auto-login entry if
                        // the protected key store failed to be created.
                        addAutoLogin &= ProtectedKeyStore.CreateProtectedKeyStoreForNewMasterKey(dbPath, pbData, exclusive);

                        // Because pbData contains the unencrypted key, we need to clear the array so it does
                        // not persist in memory.
                        MemUtil.ZeroArray(pbData);
                    }

                    // Add an auto-login entry if it meets all of the criteria.
                    if (addAutoLogin)
                        pluginConfiguration.AddAutoLogin(dbPath);
                }

                // Reset helper variables.
                Helper.CreateNewKeyRequestingDefaultKey = false;
                Helper.CreateNewKeyUsingExistingKey = false;
            }
        }

        // Method to perform an auto-login of the database specified in the IConnectionInfo instance, or auto-login
        // using the default protected key store.
        private bool PerformAutoLogin(IOConnectionInfo connectionInfo) =>
            PerformAutoLogin(connectionInfo, connectionInfo?.Path) ||
            PerformAutoLogin(connectionInfo, Helper.DefaultProtectedKeyStoreName);

        // Method to perform an auto-login of the database specified in the IConnectionInfo instance, using the
        // database path specified in dbPath.
        private bool PerformAutoLogin(IOConnectionInfo connectionInfo, string dbPath)
        {
            bool success = false;

            // Check whether auto-login is set and enabled for this database.
            if (PluginConfiguration.Instance.IsAutoLoginSet(dbPath, out bool autoLoginEnabled) && autoLoginEnabled)
            {
                byte[] pbData = null;

                try
                {
                    pbData = ProtectedKeyStore.GetProtectedKeyStore(dbPath);
                    if (pbData != null)
                    {
                        // If getting the key was successful, create a new CompositeKey instance
                        // and use it to open the database.
                        CompositeKey compositeKey = new CompositeKey();

                        compositeKey.AddUserKey(new KcpCustomKey(Helper.PluginName, pbData, false));
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
                tsmi.Text = string.Format("{0} Options...", Helper.PluginName);
                tsmi.Click += OnMenuItemClick;
            }

            return tsmi;
        }

        // Menu item click handler. Show the Options dialog.
        private void OnMenuItemClick(object sender, EventArgs e) =>
            new OptionsDlg().ShowDialog();
    }
}
