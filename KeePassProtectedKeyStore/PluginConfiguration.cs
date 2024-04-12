using System;
using System.Collections.Generic;
using System.Xml;

namespace KeePassProtectedKeyStore
{
    public class PluginConfiguration
    {
        // All access to this class will be via the Instance member.
        public static PluginConfiguration Instance { get; } = new PluginConfiguration();

        public int LastError { get; set; } = 0;

        private XmlDocument Document { get; } = new XmlDocument();

        private static string RootNodeName => "Configuration";

        private static string VersionNodeName => "Version";

        private static string LastDirectoryNodeName => "LastDirectory";

        private static string AutoLoginOptionsNodeName => "AutoLoginOptions";

        private static string AutoLoginByDefaultNodeName => "AutoLoginByDefault";

        private static string AutoLoginListNodeName => "AutoLoginList";

        private static string AutoLoginNodeName => "AutoLogin";

        private static string AutoLoginEnabledAttributeName => "Enabled";

        private static string CurrentVersion => "1.0";

        // Private constructor. All access to this class will be via the Instance member.
        private PluginConfiguration()
        {
            bool loadSuccessful = false;

            try
            {
                string xml = AppDataStore.GetPluginConfiguration();

                // If xml is empty, that indicates an error occurred loading the configuration file.
                // This will always occur the first time the plugin is used, as the config file was
                // not yet created.
                if (!string.IsNullOrEmpty(xml))
                {
                    Document.LoadXml(xml);
                    loadSuccessful = RootNode != null &&
                        AutoLoginListNode != null &&
                        VersionNode?.InnerText == CurrentVersion;
                }
            }
            catch
            {
                // Ignore any errors loading the xml. The instance will be initialized with default
                // values.
            }

            // If any issues arise loading the xml, remove any xml data that may be there.
            if (!loadSuccessful)
                Document.RemoveAll();

            // Init is non-destructive and will fill in any xml elements that may happen to be missing.
            Init();
        }

        // Method to initialize a default document for the PluginConfiguration instance.
        private void Init()
        {
            if (Document.FirstChild?.NodeType != XmlNodeType.XmlDeclaration)
                Document.AppendChild(Document.CreateXmlDeclaration("1.0", "UTF-8", null));

            if (RootNode == null)
                Document.AppendChild(Document.CreateElement(RootNodeName));

            if (VersionNode == null)
            {
                XmlNode versionNode = Document.CreateElement(VersionNodeName);

                versionNode.InnerText = CurrentVersion;
                RootNode.AppendChild(versionNode);
            }

            if (LastDirectoryNode == null)
                RootNode.AppendChild(Document.CreateElement(LastDirectoryNodeName));

            if (AutoLoginOptionsNode == null)
                RootNode.AppendChild(Document.CreateElement(AutoLoginOptionsNodeName));

            if (AutoLoginByDefaultNode == null)
            {
                XmlNode autoLoginByDefaultNode = Document.CreateElement(AutoLoginByDefaultNodeName);

                autoLoginByDefaultNode.InnerText = true.ToString();
                AutoLoginOptionsNode.AppendChild(autoLoginByDefaultNode);
            }

            if (AutoLoginListNode == null)
                AutoLoginOptionsNode.AppendChild(Document.CreateElement(AutoLoginListNodeName));
        }

        // LastDirectory property. Returns the user's Documents folder if a value has not been set.
        public string LastDirectory
        {
            get => !string.IsNullOrEmpty(LastDirectoryNode?.InnerText) ?
                LastDirectoryNode.InnerText :
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            set
            {
                if (LastDirectoryNode != null)
                {
                    LastDirectoryNode.InnerText = value.ToString();

                    // Because the setter doesn't return a value, the LastError member variable is set
                    // depending on whether the configuration file was written successfully.
                    LastError = AppDataStore.SetPluginConfiguration(Document.OuterXml);
                }
            }
        }

        // AutoLoginMap property. The getter returns a map of the auto-login entries, with
        // the keys being the database names and the values being the status of whether
        // auto-login is enabled for that database. The setter copies the map to the
        // Document and saves the configuration file.
        public Dictionary<string, bool> AutoLoginMap
        {
            get
            {
                Dictionary<string, bool> result = new Dictionary<string, bool>();

                foreach (XmlNode autoLoginNode in AutoLoginListNode?.ChildNodes)
                    result[autoLoginNode.InnerText] = Convert.ToBoolean(autoLoginNode.Attributes?[AutoLoginEnabledAttributeName]?.Value ?? false.ToString());

                return result;
            }
            set
            {
                XmlNode autoLoginListNode = AutoLoginListNode;

                autoLoginListNode?.RemoveAll();
                foreach (string dbName in value.Keys)
                {
                    XmlNode autoLoginNode = Document.CreateElement(AutoLoginNodeName);
                    XmlAttribute autoLoginEnabledAttribute = Document.CreateAttribute(AutoLoginEnabledAttributeName);

                    autoLoginEnabledAttribute.InnerText = value[dbName].ToString();
                    autoLoginNode.InnerText = dbName;
                    autoLoginNode.Attributes.Append(autoLoginEnabledAttribute);
                    autoLoginListNode?.AppendChild(autoLoginNode);
                }

                // Because the setter doesn't return a value, the LastError member variable is set
                // depending on whether the configuration file was written successfully.
                LastError = AppDataStore.SetPluginConfiguration(Document.OuterXml);
            }
        }

        // AutoLoginByDefault property.
        public bool AutoLoginByDefault
        {
            get => Convert.ToBoolean(AutoLoginByDefaultNode?.InnerText ?? true.ToString());
            set
            {
                if (AutoLoginByDefaultNode != null)
                {
                    AutoLoginByDefaultNode.InnerText = value.ToString();

                    // Because the setter doesn't return a value, the LastError member variable is set
                    // depending on whether the configuration file was written successfully.
                    LastError = AppDataStore.SetPluginConfiguration(Document.OuterXml);
                }
            }
        }

        // Method to determine whether the database specified by dbPath exists in the Document. If
        // found, it also returns the status of whetner auto-login is enabled for that database.
        public bool IsAutoLoginSet(string dbPath, out bool autoLoginEnabled)
        {
            XmlNode autoLoginNode = null;

            if (!string.IsNullOrEmpty(dbPath))
            {
                string dbPathLower = dbPath.ToLower();
                XmlNodeList autoLoginNodes = AutoLoginListNode?.ChildNodes;

                // Loop through the AutoLoginList nodes for the database. The list is searched manually
                // rather than using methods such as XmlNode.SelectSingleNode, because there does not
                // appear to be any way of performing a case-insensitive match against the InnerText
                // of the AutoLogin nodes.
                for (int i = 0; i < autoLoginNodes?.Count && autoLoginNode == null; i++)
                    if (autoLoginNodes[i].InnerText.ToLower() == dbPathLower)
                        autoLoginNode = autoLoginNodes[i];
            }

            autoLoginEnabled = Convert.ToBoolean(autoLoginNode?.Attributes?[AutoLoginEnabledAttributeName]?.Value ?? false.ToString());
            return autoLoginNode != null;
        }

        // Method to add an auto-login for the specified database to the configuration file.
        public static bool AddAutoLogin(string dbPath)
        {
            PluginConfiguration pluginConfiguration = Instance;
            bool result = false;

            if (!pluginConfiguration.IsAutoLoginSet(dbPath, out _))
            {
                Dictionary<string, bool> autoLoginMap = pluginConfiguration.AutoLoginMap;

                // If an entry doesn't already exist, add it to the Dictionary. The value indicating
                // whether auto-login is enabled is determined by the user's preference whether to
                // enable it by default.
                autoLoginMap[dbPath] = pluginConfiguration.AutoLoginByDefault;

                // Set the PluginConfiguration's AutoLoginMap with the updated Dictionary. This assignment
                // will cause the configuration file to be written to the disk.
                pluginConfiguration.AutoLoginMap = autoLoginMap;

                // If the assignment to AutoLoginMap failed (for example, an I/O error writing the file),
                // the PluginConfiguration class' LastError will contain the HRESULT of the caught
                // exception.
                result = pluginConfiguration.LastError == 0;
            }

            return result;
        }

        // Method to return the document's root node.
        private XmlNode RootNode =>
            Document?.SelectSingleNode(RootNodeName);

        // Method to return the document's Version node.
        private XmlNode VersionNode =>
            RootNode?.SelectSingleNode(VersionNodeName);

        // Method to return the document's LastDirectory node.
        private XmlNode LastDirectoryNode =>
            RootNode?.SelectSingleNode(LastDirectoryNodeName);

        // Method to return the document's AutoLoginOptions node.
        private XmlNode AutoLoginOptionsNode =>
            RootNode?.SelectSingleNode(AutoLoginOptionsNodeName);

        // Method to return the document's AutoLoginByDefault node.
        private XmlNode AutoLoginByDefaultNode =>
            AutoLoginOptionsNode?.SelectSingleNode(AutoLoginByDefaultNodeName);

        // Method to return the document's AutoLoginList node.
        private XmlNode AutoLoginListNode =>
            AutoLoginOptionsNode?.SelectSingleNode(AutoLoginListNodeName);
    }
}
