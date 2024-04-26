using KeePassLib.Utility;
using KeePassProtectedKeyStore.Properties;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace KeePassProtectedKeyStore
{
    internal static class AppDataStore
    {
        // Protected key store file extension.
        private static string ProtectedKeyStoreFileExtension => ".bin";

        // AppData Company path for this plugin
        private static string KeePassProtectedKeyStoreCompanyPath { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>().Company);

        // AppData Product path for this plugin
        private static string KeePassProtectedKeyStoreProductPath { get; } =
            Path.Combine(KeePassProtectedKeyStoreCompanyPath, Helper.PluginName);

        // AppData configuration file path for this plugin
        private static string PluginConfigurationFileName =>
            Path.Combine(KeePassProtectedKeyStoreProductPath, "config.xml");

        // Method to return a list of protected key store filenames.
        public static string[] GetProtectedKeyStores(string protectedKeyStoreSubFolder)
        {
            string protectedKeyStoreProductPath = GetProtectedKeyStoreProductPath(protectedKeyStoreSubFolder);

            return Directory.Exists(protectedKeyStoreProductPath) ?
                Directory.GetFiles(protectedKeyStoreProductPath, string.Format("*{0}", ProtectedKeyStoreFileExtension))
                    .Select(path => Path.GetFileName(path))
                    .ToArray() :
                new string[0];
        }

        // Method to return the protected key store for the specified database.
        public static byte[] GetProtectedKeyStore(string dbPath, string protectedKeyStoreSubFolder)
        {
            string protectedKeyStoreFilePath = BuildProtectedKeyStoreFilePath(dbPath, protectedKeyStoreSubFolder);
            byte[] pbProtectedKeyStore = null;

            try
            {
                pbProtectedKeyStore = File.ReadAllBytes(protectedKeyStoreFilePath);
            }
            catch
            {
                // Do not display a message or take any other action if, for example, the
                // protected key store file doesn't exist. KeePass itself will eventually
                // display a meaningful error message to the user.
            }

            return pbProtectedKeyStore;
        }

        // Method to store the protected key store in the AppData store.
        public static bool SetProtectedKeyStore(string dbPath, byte[] pbUnprotectedKey, byte[] pbProtectedKey, string protectedKeyStoreSubFolder)
        {
            string protectedKeyStoreFilePath = BuildProtectedKeyStoreFilePath(dbPath, protectedKeyStoreSubFolder);
            byte[] pbData = null;
            bool writeFile = pbUnprotectedKey != null && pbProtectedKey != null;
            bool result = false;

            if (writeFile && File.Exists(protectedKeyStoreFilePath))
            {
                // A protected key store file already exists. Attempt to get the unencrypted contents.
                pbData = ProtectedKeyStore.GetProtectedKeyStore(dbPath);
                if (pbData != null)
                {
                    // If the existing file has the same key as the new one, tell the user that a new
                    // file does not need to be created. If the keys are different, warn the user
                    // about replacing the existing file and ask whether to proceed.
                    DialogResult dialogResult = Enumerable.SequenceEqual(pbData, pbUnprotectedKey) ?
                        Helper.DisplayMessage(Resources.ProtectedKeyStoreExistsAndIsSame,
                            string.Empty,
                            string.Empty,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information) :
                        Helper.DisplayMessage(Resources.ProtectedKeyStoreExistsAndIsDifferent,
                            string.Empty,
                            string.Empty,
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2);

                    // Write the file only if the user responded yes to replacing the existing file.
                    writeFile = dialogResult == DialogResult.Yes;
                }
            }

            // Proceed only if a file does not exist or if the user agrees to overwrite it.
            if (writeFile)
            {
                try
                {
                    // Write the encrypted data to the specified filename. It is assumed the parent folder
                    // of protectedKeyStoreFilePath already exists.
                    File.WriteAllBytes(protectedKeyStoreFilePath, pbProtectedKey);
                    result = true;
                }
                catch (Exception exc)
                {
                    DisplayExceptionMessage(exc, "creating protected key store");
                }
            }

            // Because pbData contains the unencrypted key, we need to clear the array so it does
            // not persist in memory.
            if (pbData != null)
                MemUtil.ZeroArray(pbData);

            return result;
        }

        // Method to delete the protected key store for the specified database.
        public static bool DeleteProtectedKeyStore(string dbPath, string protectedKeyStoreSubFolder)
        {
            string protectedKeyStoreFilePath = BuildProtectedKeyStoreFilePath(dbPath, protectedKeyStoreSubFolder);
            bool result = true;

            try
            {
                File.Delete(protectedKeyStoreFilePath);
            }
            catch (FileNotFoundException)
            {
                // Ignore
            }
            catch (DirectoryNotFoundException)
            {
                // Ignore
            }
            catch (Exception exc)
            {
                DisplayExceptionMessage(exc, "deleting protected key store");
                result = false;
            }

            return result;
        }

        // Method to build the protected key store file path and filename.
        private static string BuildProtectedKeyStoreFilePath(string dbPath, string protectedKeyStoreSubFolder)
        {
            string protectedKeyStoreFileName = dbPath;
            string protectedKeyStoreProductPath = GetProtectedKeyStoreProductPath(protectedKeyStoreSubFolder);

            // Check whether dbPath contains the filename of the protected key store file itself.
            if (Path.GetExtension(dbPath).Trim().ToLower() != ProtectedKeyStoreFileExtension)
            {
                // Compute the MD5 hash of the database file path/name and use the hash value as the filename
                // for the key file. Use UTF32 encoding in case the database file path/name includes extended
                // characters. Convert the database path to lowercase so the hash is always computed
                // consistently for the same database path.
                byte[] pbHash = Helper.MD5HashData(Encoding.UTF32.GetBytes(dbPath.ToLower()));

                protectedKeyStoreFileName = Helper.ByteArrayToString(pbHash) + ProtectedKeyStoreFileExtension;
            }

            return Path.Combine(protectedKeyStoreProductPath, protectedKeyStoreFileName);
        }

        // Method to get the protected key store product path for the specified protected key store
        // subfolder. The folder hierarchy is created in case they do not exist. CreateDirectory will
        // not throw an exception if the folder already exists.
        private static string GetProtectedKeyStoreProductPath(string protectedKeyStoreSubFolder)
        {
            string protectedKeyStoreProductPath = Path.Combine(KeePassProtectedKeyStoreProductPath, protectedKeyStoreSubFolder);

            Directory.CreateDirectory(KeePassProtectedKeyStoreCompanyPath);
            Directory.CreateDirectory(KeePassProtectedKeyStoreProductPath);
            Directory.CreateDirectory(protectedKeyStoreProductPath);

            return protectedKeyStoreProductPath;
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
                // Write the plugin configuration file. It is assumed the parent folder
                // of PluginConfigurationFileName already exists.

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
