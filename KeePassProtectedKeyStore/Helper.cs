﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace KeePassProtectedKeyStore
{
    internal static class Helper
    {
        // Plugin name, to be used for display and other purposes.
        public static string PluginName { get; } = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

        // Default protected key store name.
        public static string DefaultProtectedKeyStoreName => "Default";

        // When creating a new key, specifies whether the user requested to use the default protected
        // key store.
        public static bool CreateNewKeyRequestingDefaultKey { get; set; } = false;

        // When creating a new key, specifies whether a protected key store already exists for the
        // database (either the default key or a database-specific key).
        public static bool CreateNewKeyUsingExistingKey { get; set; } = false;

        // When opening an existing key, specifies whether the default protected key store was used.
        public static bool OpenExistingKeyUsingDefaultKey { get; set; } = false;

        // Method to compute an MD5 hash value for the given byte array.
        public static byte[] MD5HashData(byte[] pbData)
        {
            byte[] pbHash = null;
            using (MD5 md5 = MD5.Create())
            {
                pbHash = md5.ComputeHash(pbData);
            }

            return pbHash;
        }

        // Method to convert a byte array to a hex string.
        public static string ByteArrayToString(byte[] pbData) =>
            BitConverter.ToString(pbData).Replace("-", "");

        // Method to convert a hex string to a byte array.
        public static byte[] StringToByteArray(string str) =>
            Enumerable.Range(0, str.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(str.Substring(x, 2), 16))
                .ToArray();

        // Method to format an exception message based on the exception's HResult. Some exceptions'
        // Message parameter may include information about which users do not need to know, because
        // it does not mean anything to them. Instead, the HResult parameter is used to get a string
        // representation of the error, which will be more generic. The only issue is that this
        // mechanism also returns the HRESULT as part of the string, which again is meaningless to
        // the user. That part of the message string is therefore removed.
        public static string FormatExceptionMessageFromHResult(Exception exc)
        {
            string errorMessage = Marshal.GetExceptionForHR(exc.HResult).Message;
            int idx = errorMessage.IndexOf("(Exception from HRESULT:");

            if (idx >= 0)
                errorMessage = errorMessage.Substring(0, idx);

            return errorMessage;
        }

        // Method to display a dialog box to the user, optionally including up to two pieces of additional
        // information to display.
        public static DialogResult DisplayMessage(string message,
                string additionalInfo1,
                string additionalInfo2 = "",
                MessageBoxButtons buttons = MessageBoxButtons.OK,
                MessageBoxIcon icon = MessageBoxIcon.Error,
                MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1) =>
            MessageBox.Show(string.Format(message, additionalInfo1, additionalInfo2),
                PluginName,
                buttons,
                icon,
                defaultButton);
    }
}
