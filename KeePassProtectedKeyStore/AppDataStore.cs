using KeePassProtectedKeyStore.Properties;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    internal static class AppDataStore
    {
        // AppData Company path for this plugin
        private static string KeePassProtectedKeyStoreCompanyPath { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>().Company);

        // AppData Product path for this plugin
        private static string KeePassProtectedKeyStoreProductPath { get; } =
            Path.Combine(KeePassProtectedKeyStoreCompanyPath, KeePassProtectedKeyStoreExt.PluginName);

        // AppData configuration file path for this plugin
        private static string PluginConfigurationFileName =>
            Path.Combine(KeePassProtectedKeyStoreProductPath, "config.xml");

        // Method to return the protected key store for the specified database.
        public static byte[] GetProtectedKeyStore(string dbPath)
        {
            string protectedKeyStoreFilePath = BuildProtectedKeyStoreFilePath(dbPath);
            byte[] pbProtectedKeyStore = null;

            try
            {
                pbProtectedKeyStore = File.ReadAllBytes(protectedKeyStoreFilePath);
            }
            catch (Exception exc)
            {
                // The most likely scenario for getting here is if a protected key store file doesn't exist.
				// This will happen in cases where the user selects this plugin as the "Key/file provider"
				// authentication method without having created the protected key store file.
                DisplayExceptionMessage(exc, "accessing protected key store");
            }

            return pbProtectedKeyStore;
        }

        // Method to store the protected key store in the AppData store.
        public static bool SetProtectedKeyStore(string dbPath, byte[] pbProtectedKeyStore)
        {
            string protectedKeyStoreFilePath = BuildProtectedKeyStoreFilePath(dbPath);
            bool result = false;

            // Proceed only if a file does not exist for the specified database or if the user agrees
            // to overwrite the existing one.
            if (!File.Exists(protectedKeyStoreFilePath) ||
                Helper.DisplayMessage(Resources.OverwriteProtectedKeyStoreFilePrompt,
                    string.Empty,
                    string.Empty,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                try
                {
                    // Create the company and product subfolders (the calls to CreateDirectory will not
                    // fail if the folders already exist), and write the encrypted data to the specified
                    // filename.
                    Directory.CreateDirectory(KeePassProtectedKeyStoreCompanyPath);
                    Directory.CreateDirectory(KeePassProtectedKeyStoreProductPath);
                    File.WriteAllBytes(protectedKeyStoreFilePath, pbProtectedKeyStore);
                    result = true;
                }
                catch (Exception exc)
                {
                    DisplayExceptionMessage(exc, "creating protected key store");
                }
            }

            return result;
        }

        // Method to build the protected key store file path and filename.
        private static string BuildProtectedKeyStoreFilePath(string dbPath)
        {
            // Compute the MD5 hash of the database file path/name and use the hash value + ".bin" as the
            // filename for the key file. Use UTF32 encoding in case the database file path/name includes
            // extended characters. Convert the database path to lowercase so the hash is always computed
            // consistently for the same database path.
            byte[] pbHash = Helper.MD5HashData(Encoding.UTF32.GetBytes(dbPath.ToLower()));
            string protectedKeyStoreFileName = Helper.ByteArrayToString(pbHash) + ".bin";

            return Path.Combine(KeePassProtectedKeyStoreProductPath, protectedKeyStoreFileName);
        }

        // Method to read the plugin configuration file.
        public static string GetPluginConfiguration()
        {
            string result = string.Empty;

            try
            {
                result = File.ReadAllText(PluginConfigurationFileName);
            }
            catch (FileNotFoundException)
            {
                // This will happen before the file is created. Do not alert the user.
            }
            catch (DirectoryNotFoundException)
            {
                // This will happen before the file's directory is created. Do not alert the user.
            }
            catch (Exception exc)
            {
                // For all other exceptions, alert the user.
                DisplayExceptionMessage(exc, "reading configuration");
            }

            return result;
        }

        // Method to write the plugin configuration file.
        public static int SetPluginConfiguration(string xml)
        {
            int result = 0;

            try
            {
                // Create the company and product subfolders (the calls to CreateDirectory will not
                // fail if the folders already exist), and write the plugin configuration file.
                Directory.CreateDirectory(KeePassProtectedKeyStoreCompanyPath);
                Directory.CreateDirectory(KeePassProtectedKeyStoreProductPath);
                File.WriteAllText(PluginConfigurationFileName, xml);
            }
            catch (Exception exc)
            {
                DisplayExceptionMessage(exc, "writing configuration");

                // Return the HRESULT contained in the exception.
                result = exc.HResult;
            }

            return result;
        }

        // Method to format and display the exception message to the user.
        //
        // The exception's Message parameter may include information about the specific filename.
        // Users don't need to know this information, because it does not mean anything to them.
        // Instead, the HResult parameter is used to get a string representation of the error,
        // which will be more generic and will not include the filename.
        private static void DisplayExceptionMessage(Exception exc, string operationType) =>
            Helper.DisplayMessage(Resources.AppDataStoreExceptionMessage,
                operationType,
                Helper.FormatExceptionMessageFromHResult(exc),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
    }
}
