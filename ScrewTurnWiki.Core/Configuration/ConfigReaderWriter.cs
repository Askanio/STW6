using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Web.Configuration;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Configuration
{
    /// <summary>
    /// Defines a class responsible for reading and writing application settings/configuration.
    /// </summary>
    public class ConfigReaderWriter : IConfigReaderWriter
    {
        private bool _isWebConfig;

        private System.Configuration.Configuration _config;

        private ScrewTurnWikiSection _section;

        private bool _isConfigLoaded;

        /// <summary>
        /// 
        /// </summary>
        public string ConfigFilePath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigReaderWriter"/> class.
        /// </summary>
        public ConfigReaderWriter() : this("")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigReaderWriter"/> class.
        /// </summary>
        internal ConfigReaderWriter(string configFilePath)
        {
            if (!string.IsNullOrEmpty(configFilePath))
            {
                if (!File.Exists(configFilePath))
                    throw new Exception(String.Format("The XML config file {0} could not be found", configFilePath));
            }
            ConfigFilePath = configFilePath;

            Load();
        }

        /// <summary>
        /// Gets the current application settings.
        /// </summary>
        /// <returns>A new <see cref="ApplicationSettings"/> instance</returns>
        public ApplicationSettings GetApplicationSettings()
        {
            //Load();
            var settings = new ApplicationSettings();
            settings.Installed = ConvertToBoolean(_section.StwSettings.Get("Installed").Value);
            settings.PublicDirectory = _section.StwSettings.Get("PublicDirectory").Value;
            settings.GlobalSettingsStorageProvider = _section.StwSettings.Get("GlobalSettingsStorageProvider").Value;
            settings.GlobalSettingsStorageProviderConfig = _section.StwSettings.Get("GlobalSettingsStorageProviderConfig").Value;

            settings.Wikis = MappingElementsToWikis(_section.WikiList);

            settings.SettingProvider = MappingFirstElementToStorageProvider(_section.SettingsProvider);
            settings.FilesProviders = MappingElementsToStorageProviders(_section.FilesProviders);
            settings.UsersProviders = MappingElementsToStorageProviders(_section.UsersProviders);
            settings.ThemesProviders = MappingElementsToStorageProviders(_section.ThemesProviders);
            settings.PagesProviders = MappingElementsToStorageProviders(_section.PagesProviders);
            settings.IndexDirectoryProvider = MappingFirstElementToStorageProvider(_section.IndexDirectoryProviders);

            settings.NeedMasterPassword = NeedMasterPassword(settings);

            return settings;
        }


        private bool NeedMasterPassword(ApplicationSettings applicationSettings)
        {
            if (applicationSettings.Installed)
            {
                try
                {
                    IProviderV60 globalSettingsStorageProvider = ProviderLoader.LoadGlobalSettingsStorageProvider(applicationSettings.GlobalSettingsStorageProvider);
                    var host = new Host();
                    globalSettingsStorageProvider.SetUp(host, applicationSettings.GlobalSettingsStorageProviderConfig);
                    var password = GlobalSettings.GetMasterPassword((IGlobalSettingsStorageProviderV60) globalSettingsStorageProvider);
                    return String.IsNullOrEmpty(password);
                }
                catch (Exception ex)
                {
                    var message = ExceptionHelper.BuildLogError(ex, "");
                    Log.LogEntry(message, EntryType.Error, Log.SystemUsername, null);
                }
            }
            return true;
        }

        /// <summary>
        /// Convert the value to type of boolean
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool ConvertToBoolean(string value)
        {
            bool result = false;
            bool.TryParse(value, out result);
            return result;
        }

        /// <summary>
        /// Mapping all elements to the list of Wiki
        /// </summary>
        /// <returns></returns>
        private List<Wiki> MappingElementsToWikis(SettingElementCollection<WikiElement> elements)
        {
            var result = new List<Wiki>();
            foreach (WikiElement element in elements)
                result.Add(new Wiki(element.Name, element.Host.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>()));
            return result;
        }

        /// <summary>
        /// Mapping first element to the StorageProvider
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        private StorageProvider MappingFirstElementToStorageProvider(SettingElementCollection<ProviderSettingElement> elements)
        {
            if (elements.Count == 0)
                return null;
            return MappingElementToStorageProvider(elements[0]);
        }

        /// <summary>
        /// Mapping all elements to the list of StorageProvider
        /// </summary>
        /// <returns></returns>
        private List<StorageProvider> MappingElementsToStorageProviders(SettingElementCollection<ProviderSettingElement> elements)
        {
            var result = new List<StorageProvider>();
            foreach (ProviderSettingElement element in elements)
                result.Add(MappingElementToStorageProvider(element));
            return result;
        }

        /// <summary>
        /// Mapping element's fields to new StorageProvider
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private StorageProvider MappingElementToStorageProvider(ProviderSettingElement element)
        {
            return new StorageProvider()
            {
                TypeName = element.Name,
                AssemblyName = element.Assembly,
                ConfigurationString = element.Config,
                IsDefault = element.IsDefault
            };
        }

        /// <summary>
        /// Load the configuration
        /// </summary>
        private void Load()
        {
            if (!_isConfigLoaded)
            {
                if (string.IsNullOrEmpty(ConfigFilePath))
                {
                    _config = WebConfigurationManager.OpenWebConfiguration("~");
                    _isWebConfig = true;
                }
                else
                {
                    if (ConfigFilePath.ToLower() == "app.config")
                    {
                        _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    }
                    else
                    {
                        ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                        fileMap.ExeConfigFilename = ConfigFilePath;
                        _config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                    }
                    _isWebConfig = false;
                }
                _isConfigLoaded = true;

                _section = _config.GetSection("screwTurnWiki") as ScrewTurnWikiSection;
                if (_section == null)
                {
                    string errorMessage = "";

                    if (_isWebConfig)
                        errorMessage = "The web.config file does not contain a 'screwTurnWiki' section";
                    else
                        errorMessage = string.Format("The config file{0} does not contain a 'screwTurnWiki' section", ConfigFilePath);

                    throw new InvalidOperationException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Saves the configuration settings. This will save a subset of the <see cref="ApplicationSettings"/> based on 
        /// the values that match those found in the <see cref="ScrewTurnWikiSection"/>
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <exception cref="Exception">An exception occurred while updating the settings.</exception>
        public void Save(ApplicationSettings settings)
        {
            try
            {
                _section.WikiList.Clear();
                foreach (var wiki in settings.Wikis)
                    _section.WikiList.Add(new WikiElement() { Name = wiki.WikiName, Host = String.Join(";", wiki.Hosts) });

                _section.SettingsProvider.Clear();
                _section.SettingsProvider.Add( MappingStorageProviderToElement(settings.SettingProvider));

                _section.FilesProviders.Clear();
                foreach (var filesProvider in settings.FilesProviders)
                    _section.FilesProviders.Add(MappingStorageProviderToElement(filesProvider));

                _section.UsersProviders.Clear();
                foreach (var usersProvider in settings.UsersProviders)
                    _section.UsersProviders.Add(MappingStorageProviderToElement(usersProvider));

                _section.ThemesProviders.Clear();
                foreach (var themesProvider in settings.ThemesProviders)
                    _section.ThemesProviders.Add(MappingStorageProviderToElement(themesProvider));

                _section.PagesProviders.Clear();
                foreach (var pagesProvider in settings.PagesProviders)
                    _section.PagesProviders.Add(MappingStorageProviderToElement(pagesProvider));

                _section.IndexDirectoryProviders.Clear();
                _section.IndexDirectoryProviders.Add(MappingStorageProviderToElement(settings.IndexDirectoryProvider));

                _section.StwSettings.Get("GlobalSettingsStorageProvider").Value = settings.GlobalSettingsStorageProvider;
                _section.StwSettings.Get("GlobalSettingsStorageProviderConfig").Value = settings.GlobalSettingsStorageProviderConfig;
                _section.StwSettings.Get("PublicDirectory").Value = settings.PublicDirectory;
                _section.StwSettings.Get("Installed").Value = settings.Installed.ToString();


                _config.Save(ConfigurationSaveMode.Minimal);
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new Exception("An exception occurred while updating the settings to the web.config", ex);
            }
        }

        /// <summary>
        /// Mapping element's fields to new StorageProvider
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        private ProviderSettingElement MappingStorageProviderToElement(StorageProvider provider)
        {
            return new ProviderSettingElement()
            {
                Name = provider.TypeName,
                Assembly = provider.AssemblyName,
                Config = provider.ConfigurationString,
                IsDefault = provider.IsDefault
            };
        }

        /// <summary>
        /// Resets the state the configuration file/store so the 'installed' property is false.
        /// </summary>
        /// <exception cref="Exception">An exception occurred while resetting web.config install state to false.</exception>
        public void ResetInstalledState()
        {
            try
            {
                _section.StwSettings.Get("Installed").Value = "false";
                _config.Save(ConfigurationSaveMode.Minimal);
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new Exception("An exception occurred while resetting web.config install state to false.", ex);
            }
        }

        /// <summary>
        /// Tests the app.config or web.config file to ensure that it can be written to.
        /// </summary>
        /// <returns>
        /// An empty string if no error occurred; otherwise the error message.
        /// </returns>
        public string TestSaveWebConfig()
        {
            try
            {
                ResetInstalledState();
                return "";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        /// <summary>
        /// Checks exists the public pirectory and attempts to save same file to it
        /// </summary>
        /// <returns>
        /// An empty string if no error occurred; otherwise the error message.
        /// </returns>
        public string TestPublicDirectory(string publicDirectory)
        {
            try
            {
                if (string.IsNullOrEmpty(publicDirectory))
                    return "Public directory cannot be empty or null";

                publicDirectory = publicDirectory.Trim('\\', '/'); // Remove '/' and '\' from head and tail

                var rootDirectory = GlobalSettings.RootDirectory;

                string path = publicDirectory;
                if (!Path.IsPathRooted(publicDirectory))
                {
                    path = Path.Combine(rootDirectory, publicDirectory);
                    if (!path.EndsWith(Path.DirectorySeparatorChar.ToString())) path += Path.DirectorySeparatorChar;
                }

                if (!Directory.Exists(path))
                    return "The public directory do not exists";

                WindowsIdentity user = WindowsIdentity.GetCurrent();
                if (!CanWrite(path, user))
                    return "The public directory do not access to write";

                return "";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool CanWrite(string directory, WindowsIdentity user)
        {
            bool bAllowed = false;
            try
            {
                //Get the directory security
                DirectorySecurity sec = Directory.GetAccessControl(directory, AccessControlSections.Access);
                System.Security.AccessControl.AuthorizationRuleCollection dacls = sec.GetAccessRules(true, true, typeof(SecurityIdentifier));

                //Enumerate each access rule
                foreach (FileSystemAccessRule dacl in dacls)
                {
                    SecurityIdentifier sid = (SecurityIdentifier)dacl.IdentityReference;

                    //If the right is either create files or write access
                    if (((dacl.FileSystemRights & FileSystemRights.CreateFiles) == FileSystemRights.CreateFiles) ||
                        ((dacl.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write))
                    {
                        //If the sid matches the user or a group the user is in
                        if ((sid.IsAccountSid() && user.User == sid) ||
                            (!sid.IsAccountSid() && user.Groups.Contains(sid)))
                        {
                            //If this is a deny right then the user has no access
                            if (dacl.AccessControlType == AccessControlType.Deny)
                                return false;

                            //Allowed, for now
                            bAllowed = true;
                        }
                    }
                }

                return bAllowed;
            }
            catch (SecurityException)
            {
                throw;
            }
        }

    }
}
