using KeePass;
using KeePassProtectedKeyStore.Properties;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace KeePassProtectedKeyStore
{
    internal abstract class EncryptionEngine
    {
        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Additional entropy to increase the complexity of the encryption.
        protected static byte[] Entropy { get; } = new byte[]
        {
            0xF7, 0x72, 0x93, 0x27, 0x62, 0xAF, 0x4F, 0x2B,
            0x87, 0x32, 0xE8, 0x0B, 0x92, 0x33, 0x0A, 0x06
        };

        // Static method to return an appropriate EncryptionEngine instance based on whether the user is using
        // Windows Hello encryption. The older version of C# used by this plugin requires the "as EncryptionEngine"
        // cast, or else it generates a CS8957 compile-time error.
        public static EncryptionEngine NewInstance =>
            PluginConfiguration.Instance.UseWindowsHelloEncryption ?
            new EncryptionEngineUsingWindowsHello() as EncryptionEngine :
            new EncryptionEngineUsingDataProtectionAPI();

        // Method to encrypt the specified byte data and save it in a file. If DoEncrypt throws an exception (e.g.,
        // encryption failed, OS does not support this method, etc.), AppDataStore.SetProtectedKeyStore will not be
        // called.
        public bool Encrypt(string dbPath, byte[] pbData)
        {
            bool result = false;

            try
            {
                // Encrypt the data. The encrypted key is valid only for the currently logged-in user and will not
                // be valid if the user's computer is rebuilt or if the user purchases a new computer. In such
                // cases, the user should have previously created an emergency key recovery file and can import it
                // if this key is no longer valid.
                byte[] pbProtectedKey = DoEncrypt(pbData);

                // Attempt to save the protected key store in a file.
                result = AppDataStore.SetProtectedKeyStore(dbPath, pbData, pbProtectedKey, ProtectedKeyStoreSubFolder, this);
            }
            catch (Exception exc)
            {
                // In these cases, exc.Message will be sufficient to display to the user.
                Helper.DisplayMessage(Resources.ProtectedKeyStoreCreateError, exc.Message);
            }

            return result;
        }

        // Method to decrypt a protected key store for the specified database. AppDataStore.GetProtectedKeyStore
        // will display a popup and return null if it cannot find a key file. If DoDecrypt throws an exception, the
        // caught exception will display a popup, and this method will return null. KeePass assumes we will notify
        // the user of any errors.
        public byte[] Decrypt(string dbPath)
        {
            byte[] pbProtectedKey = AppDataStore.GetProtectedKeyStore(dbPath, ProtectedKeyStoreSubFolder);
            byte[] pbData = null;

            try
            {
                if (pbProtectedKey != null)
                    pbData = DoDecrypt(pbProtectedKey);
            }
            catch (Exception exc)
            {
                Helper.DisplayMessage(Resources.ProtectedKeyStoreDecryptError, exc.Message);
            }

            return pbData;
        }

        // Method to return the protected key store filenames for the given ProtectedKeyStoreFileType.
        public string[] ProtectedKeyStoreFileNames =>
            AppDataStore.GetProtectedKeyStores(ProtectedKeyStoreSubFolder);

        // Method to delete the protected key store files for the given ProtectedKeyStoreFileType.
        public void DeleteProtectedKeyStoreFiles(string[] protectedKeyStoreFiles)
        {
            foreach (string protectedKeyStoreFile in protectedKeyStoreFiles)
                AppDataStore.DeleteProtectedKeyStore(protectedKeyStoreFile, ProtectedKeyStoreSubFolder);
        }

        // AppDataStore subfolder for the protected key store files.
        protected virtual string ProtectedKeyStoreSubFolder => string.Empty;

        // Not implemented in the base class; must be overridden in a derived class.
        protected abstract byte[] DoEncrypt(byte[] pbData);

        // Not implemented in the base class; must be overridden in a derived class.
        protected abstract byte[] DoDecrypt(byte[] pbProtectedKey);
    }

    // Encryption engine using Data Protection API (DPAPI).
    internal class EncryptionEngineUsingDataProtectionAPI : EncryptionEngine
    {
        // Overridden method to encrypt the byte array.
        protected override byte[] DoEncrypt(byte[] pbData) =>
            pbData != null ? ProtectedData.Protect(pbData, Entropy, DataProtectionScope.CurrentUser) : null;

        // Overridden method to decrypt the byte array.
        protected override byte[] DoDecrypt(byte[] pbProtectedKey) =>
            pbProtectedKey != null ? ProtectedData.Unprotect(pbProtectedKey, Entropy, DataProtectionScope.CurrentUser) : null;
    }

    // Encryption engine using Windows Hello.
    internal class EncryptionEngineUsingWindowsHello : EncryptionEngine
    {
        // CryptographicKey instance. Needs to be set only once during the lifetime of this class instance.
        private CryptographicKey CryptoKey { get; set; } = null;

        // Key to be signed, for creating a symmetric key.
        private static byte[] SignKey { get; } =
        {
            0x3E, 0xF5, 0xCB, 0x02, 0xB7, 0x40, 0x47, 0xE2,
            0xB5, 0xD6, 0x3D, 0x77, 0x0D, 0x70, 0x0E, 0x9E
        };

        // Class name for the Windows Hello dialog.
        private static string WindowsHelloDialogClassName => "Credential Dialog Xaml Host";

        // Sleep interval to poll for existence of Windows Hello dialog.
        private static int WindowsHelloDialogSleepInterval => 100;

        // Number of retries to poll for existence of Windows Hello dialog.
        private static int WindowsHelloDialogRetries => 30000 / WindowsHelloDialogSleepInterval;

        // Overridden AppDataStore subfolder for protected key store files.
        protected override string ProtectedKeyStoreSubFolder => "WindowsHello";

        // Overridden method to encrypt the byte array.
        protected override byte[] DoEncrypt(byte[] pbData) =>
            EncryptOrDecrypt(pbData, true);

        // Overridden method to decrypt the byte array.
        protected override byte[] DoDecrypt(byte[] pbProtectedKey) =>
            EncryptOrDecrypt(pbProtectedKey, false);

        // Method to encrypt or decrypt the byte array.
        private byte[] EncryptOrDecrypt(byte[] pbData, bool encrypt)
        {
            byte[] result = null;

            // By calling the InitCryptographicKey method from a worker thread and waiting for the thread to complete,
            // we are shielding the asynchronous nature of the method from all of the code paths leading to this point.
            Task.Run(async () => { await InitCryptographicKey(); }).Wait();

            if (CryptoKey != null && pbData != null)
            {
                IBuffer bufferIn = CryptographicBuffer.CreateFromByteArray(pbData);
                IBuffer bufferEntropy = CryptographicBuffer.CreateFromByteArray(Entropy);
                IBuffer bufferOut = encrypt ?
                    CryptographicEngine.Encrypt(CryptoKey, bufferIn, bufferEntropy) :
                    CryptographicEngine.Decrypt(CryptoKey, bufferIn, bufferEntropy);

                result = bufferOut.ToArray();
            }

            return result;
        }

        // Method to initialize the CryptoKey member variable.
        private async Task InitCryptographicKey()
        {
            // Although this code will not be called (i.e., this class will not be instantiated) if Windows Hello
            // is not supported, we will still double-check anyway as a sanity measure.
            if (CryptoKey == null && await KeyCredentialManager.IsSupportedAsync())
            {
                // Attempt to open the key credential if it already exists. If the open is unsuccessful, attempt
                // to create it.
                KeyCredentialRetrievalResult retrievalResult = await KeyCredentialManager.OpenAsync(Helper.PluginName);

                if (retrievalResult.Status != KeyCredentialStatus.Success)
                    retrievalResult = await KeyCredentialManager.RequestCreateAsync(Helper.PluginName, KeyCredentialCreationOption.ReplaceExisting);
                if (retrievalResult.Status == KeyCredentialStatus.Success)
                {
                    // Kickoff worker thread to circumvent Windows Hello anomalies. Pass in a manual reset event
                    // so the thread will keeep executing until it gets a signal to exit.
                    ManualResetEventSlim exitThreadEvent = new ManualResetEventSlim(false);

                    _ = Task.Run(() => CircumventWindowsHelloAnomalies(exitThreadEvent));

                    // Attempt to sign the sign key using the user's credentials. This is where the user is prompted
                    // with the Windows Hello verification screen.
                    IBuffer dataToSign = CryptographicBuffer.CreateFromByteArray(SignKey);
                    KeyCredentialOperationResult operationResult = await retrievalResult.Credential.RequestSignAsync(dataToSign);

                    // Set the event to cause the worker thread to exit.
                    exitThreadEvent.Set();

                    if (operationResult.Status == KeyCredentialStatus.Success)
                    {
                        // Create a symmetric key using the AES-CBC-PKCS7 algorithm.
                        SymmetricKeyAlgorithmProvider provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);

                        CryptoKey = provider.CreateSymmetricKey(operationResult.Result);
                    }
                }
            }
        }

        // This method is named as such because there are two anomalies associated with calling Windows Hello via
        // KeyCredential.RequestSignAsync. DISCLAIMER: The methods used to address these anomalies are workarounds
        // and cannot be guaranteed to work 100% of the time.
        //
        // As a side note, the Windows Hello verification dialog is launched from a separate process and is not
        // associated with the KeePass process itself. These anomalies would probably not exist if it was a child
        // window of KeePass. However, due to the high level of security associated with Windows Hello, it is
        // understandable that the requesting appliction would have no direct control over the dialog.
        //
        // The following anomalies are addressed:
        // - When the Windows Hello verification dialog is displayed, although its associated process is considered
        //   by Windows to be the foreground process, most of the time it does not receive the keyboard input.
        //   * This anomaly is addressed by finding the HWND of the dialog and forcing it to be the foreground window.
        //     The code was inspired by a combination of the WinHelloUnlock (https://github.com/Angelelz/WinHelloUnlock)
        //     and KeePassWinHello (https://github.com/sirAndros/KeePassWinHello) source code. It has a dependency on
        //     Windows Hello's class name and will no longer work if Microsoft decides to change it.
        // - When the Windows Hello verification dialog is dismissed, KeePass becomes repositioned to the bottom of
        //   the Z-order. It ends up getting covered up by any other windows on the desktop and must be brought back
        //   to the foreground manually by the user.
        //   * This anomaly is addressed by launching a "hidden" modeless dialog box while the Windows Hello dialog
        //     is being displayed, and closing it when the dialog is dismissed. While troubleshooting the anomaly, it
        //     was noted that if a MessageBox was used to display diagnostic information, KeePass would become the
        //     foreground task after Windows Hello was dismissed. Launching a modeless dialog box positioned outside
        //     the desktop coordinates (effectively making it "hidden") appears to have the same effect. After trying
        //     many different ways to bring KeePass back to the foreground, this was the only one that worked at all.
        //     It may not work consitiently (or not work at all) on versions of Windows other than the one on which it
        //     was tested.
        private void CircumventWindowsHelloAnomalies(ManualResetEventSlim exitThreadEvent)
        {
            // Keep looping until the invoking thread signals to exit the thread. The Windows Hello dialog will go from a
            // non-existent state to an existent state more than once if the user completes the verification incorrectly.
            while (!exitThreadEvent.Wait(0))
            {
                IntPtr windowsHelloDialogHandle = IntPtr.Zero;
                HiddenDlg dlg = new HiddenDlg();

                // Keep retrying to find the HWND of the Windows Hello dialog. If it is not found within the given number
                // of retries, it is unlikely the dialog will ever be found within this iteration of the code.
                for (int i = 1; i <= WindowsHelloDialogRetries && windowsHelloDialogHandle == IntPtr.Zero && !exitThreadEvent.Wait(0); i++)
                {
                    Thread.Sleep(WindowsHelloDialogSleepInterval);
                    windowsHelloDialogHandle = FindWindow(WindowsHelloDialogClassName, null);
                }

                // Launch the modeless hidden dialog.
                dlg.Show(Program.MainForm);

                // Keep making Windows Hello the foreground window as long as it exists.
                while (windowsHelloDialogHandle != IntPtr.Zero)
                {
                    SetForegroundWindow(windowsHelloDialogHandle);
                    Thread.Sleep(WindowsHelloDialogSleepInterval);
                    windowsHelloDialogHandle = FindWindow(WindowsHelloDialogClassName, null);
                }

                // Close the modeless dialog, which "should" make KeePass the foreground process.
                Thread.Sleep(WindowsHelloDialogSleepInterval);
                dlg.Close();
            }
        }
    }
}
