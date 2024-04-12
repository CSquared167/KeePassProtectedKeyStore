using KeePassLib.Security;
using KeePassLib.Utility;
using KeePassProtectedKeyStore.Properties;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml;

namespace KeePassProtectedKeyStore
{
    internal class EmergencyKeyRecoveryFile
    {
        private XmlDocument Document { get; } = new XmlDocument();

        private static string RootNodeName => KeePassProtectedKeyStoreExt.PluginName;

        private static string VersionNodeName => "Version";

        private static string DBPathNodeName => "DBPath";

        private static string KeyDataNodeName => "KeyData";

        private static string KeyDataHashNodeName => "KeyDataHash";

        private static string KeyDataAdditionalNodeName => "KeyDataAdditional";

        private static string KeyDataAdditionalHashNodeName => "KeyDataAdditionalHash";

        private static string ExclusiveNodeName => "Exclusive";

        private static string CurrentVersion => "1.0";

        private static int AesKeySizeInBits => 256;

        public static int AesKeySizeInBytes => AesKeySizeInBits >> 3;

        public string LastError { get; set; } = string.Empty;

        public ProtectedBinary UserEncryptionKey { private get; set; } = null;

        // Emergency key recovery file filter, to show the uset in the File Open/File Save dialogs.
        private static string EmergencyRecoveryKeyFileFilter { get; } =
            string.Format(Resources.EmergencyRecoveryKeyFileFilter,
                KeePassProtectedKeyStoreExt.PluginName);

        // Used for display purposes to identify emergency key file type.
        private static string EmergencyRecoveryKeyFileFileType { get; } =
            string.Format("{0} Emergency Key Recovery File",
            KeePassProtectedKeyStoreExt.PluginName);

        // Additional entropy to increase the complexity of the encryption.
        private static byte[] Entropy { get; } = new byte[]
        {
            0x76, 0x82, 0x62, 0xB7, 0x7F, 0x8D, 0x41, 0xDB,
            0x85, 0xBD, 0x81, 0x4A, 0x59, 0x62, 0xA4, 0x02
        };

        // Constructor. Input is a user-specified encryption key (or null if we are expected to
        // provide the encryption key). The below code creates a default XmlDocument with all of the
        // required nodes.
        public EmergencyKeyRecoveryFile(byte[] pbUserEncrypionKey = null)
        {
            XmlDeclaration xmlDeclaration = Document.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlNode rootNode = Document.CreateElement(RootNodeName);
            XmlNode versionNode = Document.CreateElement(VersionNodeName);
            XmlNode dbPathNodeName = Document.CreateElement(DBPathNodeName);
            XmlNode keyDataNode = Document.CreateElement(KeyDataNodeName);
            XmlNode keyDataHashNode = Document.CreateElement(KeyDataHashNodeName);
            XmlNode keyDataAdditionalNode = Document.CreateElement(KeyDataAdditionalNodeName);
            XmlNode keyDataAdditionalHashNode = Document.CreateElement(KeyDataAdditionalHashNodeName);
            XmlNode exclusiveNode = Document.CreateElement(ExclusiveNodeName);

            versionNode.InnerText = CurrentVersion;
            rootNode.AppendChild(versionNode);
            rootNode.AppendChild(dbPathNodeName);
            rootNode.AppendChild(keyDataNode);
            rootNode.AppendChild(keyDataHashNode);
            rootNode.AppendChild(keyDataAdditionalNode);
            rootNode.AppendChild(keyDataAdditionalHashNode);
            rootNode.AppendChild(exclusiveNode);
            Document.AppendChild(xmlDeclaration);
            Document.AppendChild(rootNode);

            if (pbUserEncrypionKey != null)
                UserEncryptionKey = new ProtectedBinary(true, pbUserEncrypionKey);
        }

        // Method to load an XML file from the given path.
        public bool Load(string xmlFilePath)
        {
            bool success = false;

            // Initialize LastError to am empty string.
            LastError = string.Empty;

            try
            {
                Document.RemoveAll();
                Document.Load(xmlFilePath);
                success = Version == CurrentVersion &&
                    !string.IsNullOrEmpty(DBPath) &&
                    !string.IsNullOrEmpty(KeyData) &&
                    !string.IsNullOrEmpty(KeyDataHash) &&
                    !string.IsNullOrEmpty(ExclusiveNode?.InnerText) &&
                    (!string.IsNullOrEmpty(KeyDataAdditional) ||
                    !string.IsNullOrEmpty(KeyDataAdditionalHash));
            }
            catch (XmlException)
            {
                // For Xml exceptions, allow LastError to be set to the message indicating the
                // file is not in the correct format.
            }
            catch (Exception exc)
            {
                // For all other exceptions, set LastError to the exception message.
                LastError = exc.Message;
            }

            if (!success && string.IsNullOrEmpty(LastError))
                LastError = string.Format(Resources.EmergencyRecoveryKeyFileBadFormat,
                    KeePassProtectedKeyStoreExt.PluginName);

            return success;
        }

        // Method to store the XML document to the given path.
        public bool Store(string xmlFilePath)
        {
            bool success = false;

            // Initialize LastError to am empty string.
            LastError = string.Empty;

            try
            {
                Document.Save(xmlFilePath);
                success = true;
            }
            catch (Exception exc)
            {
                // For all exceptions, set LastError to the exception message.
                LastError = exc.Message;
            }

            return success;
        }

        // Version property.
        public string Version
        {
            get => VersionNode?.InnerText ?? string.Empty;
            private set => VersionNode.InnerText = value;
        }

        // KeyData property.
        public string KeyData
        {
            get => KeyDataNode?.InnerText ?? string.Empty;
            private set => KeyDataNode.InnerText = value;
        }

        // KeyDataHash property.
        public string KeyDataHash
        {
            get => KeyDataHashNode?.InnerText ?? string.Empty;
            private set => KeyDataHashNode.InnerText = value;
        }

        // KeyDataAdditional property.
        public string KeyDataAdditional
        {
            get => KeyDataAdditionalNode?.InnerText ?? string.Empty;
            private set => KeyDataAdditionalNode.InnerText = value;
        }

        // KeyDataAdditionalHash property.
        public string KeyDataAdditionalHash
        {
            get => KeyDataAdditionalHashNode?.InnerText ?? string.Empty;
            private set => KeyDataAdditionalHashNode.InnerText = value;
        }

        // DBPath property.
        public string DBPath
        {
            get => DBPathNode?.InnerText ?? string.Empty;
            set => DBPathNode.InnerText = value;
        }

        // KeyDataArray property.
        public byte[] KeyDataArray
        {
            get => GetKeyDataArray();
            set => SetKeyDataArray(value);
        }

        // Exclusive property.
        public bool Exclusive
        {
            get => Convert.ToBoolean(ExclusiveNode?.InnerText ?? false.ToString());
            set => ExclusiveNode.InnerText = value.ToString();
        }

        // Method to return the document's root node.
        private XmlNode RootNode =>
            Document?.SelectSingleNode(RootNodeName);

        // Method to return the document's Version node.
        private XmlNode VersionNode =>
            RootNode?.SelectSingleNode(VersionNodeName);

        // Method to return the document's DBPath node.
        private XmlNode DBPathNode =>
            RootNode?.SelectSingleNode(DBPathNodeName);

        // Method to return the document's KeyData node.
        private XmlNode KeyDataNode =>
            RootNode?.SelectSingleNode(KeyDataNodeName);

        // Method to return the document's KeyDataHash node.
        private XmlNode KeyDataHashNode =>
            RootNode?.SelectSingleNode(KeyDataHashNodeName);

        // Method to return the document's KeyDataAdditional node.
        private XmlNode KeyDataAdditionalNode =>
            RootNode?.SelectSingleNode(KeyDataAdditionalNodeName);

        // Method to return the document's KeyDataAdditionalHash node.
        private XmlNode KeyDataAdditionalHashNode =>
            RootNode?.SelectSingleNode(KeyDataAdditionalHashNodeName);

        // Method to return the document's Exclusive node.
        private XmlNode ExclusiveNode =>
            RootNode?.SelectSingleNode(ExclusiveNodeName);

        // Method to create an emergency key recovery file.
        public static void CreateEmergencyRecoveryKeyFile(string dbPath, byte[] pbData, bool exclusive)
        {
            byte[] pbUserEncryptionKey = null;

            try
            {
                PluginConfiguration pluginConfiguration = PluginConfiguration.Instance;
                bool createFile = false;
                bool userEncryption = false;

                using (EncryptionMethodDialog dlg = new EncryptionMethodDialog())
                {
                    createFile = dlg.ShowDialog() == DialogResult.OK;
                    userEncryption = dlg.UserEncryption;
                }

                if (createFile && userEncryption)
                    using (CreatePasswordDlg dlg = new CreatePasswordDlg())
                    {
                        createFile = dlg.ShowDialog() == DialogResult.OK;
                        if (createFile)
                            pbUserEncryptionKey = dlg.Password;
                    }

                if (createFile)
                {
                    // Initialize an EmergencyRecoveryKeyFile instance with the database name and the
                    // key data.
                    EmergencyKeyRecoveryFile keyFile = new EmergencyKeyRecoveryFile(pbUserEncryptionKey)
                    {
                        DBPath = dbPath,
                        KeyDataArray = pbData,
                        Exclusive = exclusive
                    };

                    keyFile.KeyDataArray = pbData;

                    // Prompt the user to save the file.
                    using (SaveFileDialog dlg = new SaveFileDialog
                    {
                        CheckFileExists = false,
                        CheckPathExists = true,
                        DefaultExt = ".xml",
                        FileName = string.Format("{0}.{1}.xml",
                            KeePassProtectedKeyStoreExt.PluginName,
                            nameof(EmergencyKeyRecoveryFile)),
                        Filter = EmergencyRecoveryKeyFileFilter,
                        InitialDirectory = pluginConfiguration.LastDirectory,
                        OverwritePrompt = true,
                        Title = string.Format("Save {0}", EmergencyRecoveryKeyFileFileType)
                    })
                    {
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            // Set the LastDirectory configuration property so any subsequent file open/save
                            // operations will initialize to the same directory.
                            pluginConfiguration.LastDirectory = Path.GetDirectoryName(dlg.FileName);

                            // Save the emergency key recovery file with the specified folder and filename.
                            // Let the user know if the file saved successfully, and display an error message
                            // if it failed.
                            if (keyFile.Store(dlg.FileName))
                                Helper.DisplayMessage(userEncryption ?
                                        Resources.EmergencyRecoveryKeyFileCreatedUserEncryption :
                                        Resources.EmergencyRecoveryKeyFileCreated,
                                    EmergencyRecoveryKeyFileFileType,
                                    string.Empty,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                            else
                                Helper.DisplayMessage(Resources.EmergencyRecoveryKeyFileCreateFailed,
                                    EmergencyRecoveryKeyFileFileType,
                                    keyFile.LastError);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                // An exception could be thrown, for example, if an error occurred while initializing
                // the EmergencyRecoveryKeyFile instance. While exc.Message could be vague and confusing
                // to the user, converting the HRESULT to an error message gives a more user-friendly
                // message.
                Helper.DisplayMessage(Resources.EmergencyRecoveryKeyFileCreateFailed,
                    EmergencyRecoveryKeyFileFileType,
                    Helper.FormatExceptionMessageFromHResult(exc));
            }

            if (pbUserEncryptionKey != null)
                MemUtil.ZeroArray(pbUserEncryptionKey);
        }

        // Method to import an emergency key recovery file.
        public static bool ImportEmergencyRecoveryKeyFile(out string dbPath, out bool exclusive)
        {
            EmergencyKeyRecoveryFile keyFile = new EmergencyKeyRecoveryFile();
            PluginConfiguration pluginConfiguration = PluginConfiguration.Instance;
            byte[] pbData = null;
            bool result = false;

            try
            {
                // Prompt the user to open the emergency key recovery file.
                using (OpenFileDialog dlg = new OpenFileDialog
                {
                    CheckPathExists = true,
                    CheckFileExists = true,
                    Filter = EmergencyRecoveryKeyFileFilter,
                    InitialDirectory = pluginConfiguration.LastDirectory,
                    Multiselect = false,
                    Title = string.Format("Select {0}",
                        EmergencyRecoveryKeyFileFileType)
                })
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        // Set the LastDirectory configuration property so any subsequent file open/save
                        // operations will initialize to the same directory.
                        pluginConfiguration.LastDirectory = Path.GetDirectoryName(dlg.FileName);

                        // If the user selects OK, attempt to load the file into the EmergencyRecoveryKeyFile
                        // instance. If it fails to load (e.g., not an emergency key recovery file, file
                        // contents are corrupted, etc.) or if the key data is invalid (again, due to file
                        // corruption or some other issue), display an appropriate error to the user.
                        if (!keyFile.Load(dlg.FileName))
                            ShowImportError(keyFile.LastError);
                        else
                        {
                            bool getKeyData = true;

                            if (string.IsNullOrEmpty(keyFile.KeyDataAdditional) && !string.IsNullOrEmpty(keyFile.KeyDataAdditionalHash))
                            {
                                // If the user had specified the encryption key when the file was created,
                                // prompt for the password now.
                                using (VerifyPasswordDlg verifyPasswordDlg = new VerifyPasswordDlg(keyFile.KeyDataAdditionalHash))
                                {
                                    getKeyData = verifyPasswordDlg.ShowDialog() == DialogResult.OK;

                                    if (getKeyData)
                                    {
                                        // If the user successfully entered the password, set it in the
                                        // EmergencyRecoveryKeyFile instance.
                                        keyFile.UserEncryptionKey = new ProtectedBinary(true, verifyPasswordDlg.Password);
                                    }
                                }
                            }

                            if (getKeyData)
                            {
                                // Attempt to fetch the unencrypted key data.
                                pbData = keyFile.KeyDataArray;

                                // If an issue occurred, the return value will be null, and keyFile.LastError will
                                // contain the HRESULT of the error.
                                if (pbData == null)
                                    ShowImportError(keyFile.LastError);
                            }
                        }
                    }
                }

                // If everything passes until this point, we need to validate the location of the KeePass
                // database associated with the emergency key recovery file. The database path at the time
                // the file was created is stored in the file itself, but the user may have since moved the
                // database file. It is important for this plugin to associate the protected key store with
                // its corresponding database.
                if (pbData != null)
                    using (OpenFileDialog dlg = new OpenFileDialog
                    {
                        CheckPathExists = true,
                        CheckFileExists = true,
                        FileName = Path.GetFileName(keyFile.DBPath),
                        Filter = Resources.KeePassFileFilter,
                        InitialDirectory = Path.GetDirectoryName(keyFile.DBPath),
                        Multiselect = false,
                        Title = string.Format("Select Database File Associated with {0}",
                            EmergencyRecoveryKeyFileFileType)
                    })
                    {
                        // If the user selects OK, create a protected key store and store it.
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            result = ProtectedKeyStore.CreateAndStoreProtectedKeyStore(dlg.FileName, pbData);
                            if (result)
                                Helper.DisplayMessage(Resources.ProtectedKeyStoreCreatedAfterImport,
                                    KeePassProtectedKeyStoreExt.PluginName,
                                    string.Empty,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Asterisk);
                        }
                    }
            }
            catch (Exception exc)
            {
                // Use the HRESULT to format a message that might be more user-friendly than the message
                // contained in exc.Message.
                ShowImportError(Helper.FormatExceptionMessageFromHResult(exc));
            }

            if (pbData != null)
                MemUtil.ZeroArray(pbData);

            dbPath = result ? keyFile.DBPath : string.Empty;
            exclusive = result && keyFile.Exclusive;

            return result;
        }

        public static byte[] MakeAesEncryptionKeyFromString(byte[] pbEncryptionKeyString)
        {
            byte[] pbEncryptionKey = new byte[AesKeySizeInBytes];

            MemUtil.ZeroArray(pbEncryptionKey);
            Buffer.BlockCopy(pbEncryptionKeyString, 0, pbEncryptionKey, 0, Math.Min(pbEncryptionKeyString.Length, AesKeySizeInBytes));

            return pbEncryptionKey;
        }

        // Method to show a dialog box alerting the user of an emergency key recovery file import error.
        private static void ShowImportError(string additionalInfo) =>
            Helper.DisplayMessage(Resources.EmergencyRecoveryFileImportError,
                KeePassProtectedKeyStoreExt.PluginName,
                additionalInfo);

        // Method to retrieve the KeyData node from the XML document.
        private byte[] GetKeyDataArray()
        {
            // Convert the KeyData node from a hex string to a byte array.
            byte[] pbDataEnc = Helper.StringToByteArray(KeyData);

            // Decrypt the key data. The result will be null if a decryption error occurs.
            byte[] pbData = DecryptKeyData(pbDataEnc);

            // Convert the KeyDataHash node from a hex string to a byte array.
            byte[] pbHash = Helper.StringToByteArray(KeyDataHash);

            // Compute the hash value of the key data.
            byte[] pbHashCheck = pbData != null ? Helper.MD5HashData(pbData) : null;

            // If the key data was not successfully decrypted or the computed hash value is not
            // the same as the KeyDataHash value, set LastError to indicate the data is
            // corrupted.
            LastError = pbHashCheck == null || !Enumerable.SequenceEqual(pbHash, pbHashCheck) ?
                Resources.EmergencyRecoveryKeyFileCorrupt :
                string.Empty;

            return pbData;
        }

        // Method to set the key data in the XML document.
        private void SetKeyDataArray(byte[] pbData)
        {
            // Compute the hash value of the key data.
            byte[] pbHash = Helper.MD5HashData(pbData);

            // Encrypt the key data.
            byte[] pbDataEnc = EncryptKeyData(pbData);

            // Convert the encrypted key data and hash value to hex strings and set the values in
            // the XML document.
            KeyData = Helper.ByteArrayToString(pbDataEnc);
            KeyDataHash = Helper.ByteArrayToString(pbHash);
        }

        // Method to perform an AES encryption on the given byte array.
        //
        // This method does not catch any exceptions that may come up during the encryption. The
        // reason is that this method is called as part of a property setter and as such has no
        // direct way of notifying the caller that an error occurred. For this reason, the
        // expectation is for the caller to catch any exceptions and report them back to the user.
        private byte[] EncryptKeyData(byte[] pbData)
        {
            byte[] pbDataEnc = null;

            using (Aes aes = Aes.Create())
            {
                // Use the user-specified encryption key if specified, else generate a random key.
                byte[] pbEncKey = UserEncryptionKey?.ReadData();

                if (pbEncKey == null)
                {
                    pbEncKey = new byte[AesKeySizeInBytes];
                    new Random().NextBytes(pbEncKey);
                }

                // Set the encryption key and entropy values.
                aes.KeySize = AesKeySizeInBits;
                aes.Key = pbEncKey;
                aes.IV = Entropy;

                // Create an ICryptoTransform instance for the encryption.
                ICryptoTransform cryptoTransform = aes.CreateEncryptor();

                // According to Microsoft's documentation, MemoryStream implements IDisposable but
                // does not have anything to dispose, so wrapping it in a "using" construct is not
                // necessary.
                MemoryStream ms = new MemoryStream();

                // Create a CryptoStream instance and write the unencrypted byte array to it, which
                // will result in the MemoryStream containing the encrypted data.
                //
                // The call to FlushFinalBlock is crucial. Microsoft's example code does not include
                // it, but if it is not included, the last portion of the encrypted data will not be
                // written to the MemoryStream.
                using (CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
                {
                    cs.Write(pbData, 0, pbData.Length);
                    cs.FlushFinalBlock();
                    pbDataEnc = ms.ToArray();
                }

                // If we generated the encryption/decryption key, convert it to a hex string and write
                // it to the XML document in the KeyDataAdditional node. If the user provided the key,
                // generate an MD5 hash of the key, convert it to a hex string and write it to the XML
                // document in the KeyDataAdditionalHash node.
                if (UserEncryptionKey == null)
                    KeyDataAdditional = Helper.ByteArrayToString(pbEncKey);
                else
                {
                    byte[] pbUserEncKey = UserEncryptionKey.ReadData();

                    KeyDataAdditionalHash = Helper.ByteArrayToString(Helper.MD5HashData(pbUserEncKey));
                    MemUtil.ZeroArray(pbUserEncKey);
                }
            }

            return pbDataEnc;
        }

        // Method to perform an AES decryption on the given byte array.
        private byte[] DecryptKeyData(byte[] pbDataEnc)
        {
            // Convert the encryption/decryption key from a hex string to a byte array.
            byte[] pbEncKey = UserEncryptionKey != null ?
                UserEncryptionKey.ReadData() :
                Helper.StringToByteArray(KeyDataAdditional);
            byte[] pbData = null;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    // Set the encryption key and entropy values.
                    aes.KeySize = AesKeySizeInBits;
                    aes.Key = pbEncKey;
                    aes.IV = Entropy;

                    // Create an ICryptoTransform instance for the decryption.
                    ICryptoTransform cryptoTransform = aes.CreateDecryptor();

                    // According to Microsoft's documentation, MemoryStream implements IDisposable but
                    // does not have anything to dispose, so wrapping it in a "using" construct is not
                    // necessary.
                    MemoryStream msEnc = new MemoryStream(pbDataEnc);

                    // Create a CryptoStream instance and write the encrypted byte array to it, which
                    // will result in the MemoryStream containing the decrypted data.
                    using (CryptoStream cs = new CryptoStream(msEnc, cryptoTransform, CryptoStreamMode.Read))
                    {
                        using (MemoryStream msDec = new MemoryStream())
                        {
                            cs.CopyTo(msDec);
                            pbData = msDec.ToArray();
                        }
                    }
                }
            }
            catch
            {
                // If an exception occcurs, just ensure null is returned. The caller will need to
                // check for a null return and relay an appropriate error message back to the user.
                pbData = null;
            }

            // Zero out the encryption key buffer if present (because it might contain the user-entered
            // password). 
            if (pbEncKey != null)
                MemUtil.ZeroArray(pbEncKey);

            return pbData;
        }
    }
}
