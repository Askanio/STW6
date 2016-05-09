using System;
using System.Collections.Generic;

namespace ScrewTurn.Wiki.Configuration
{
    /// <summary>
    /// Contains all settings that require an application
    /// </summary>
    public class ApplicationSettings
    {
        private static ApplicationSettings instance;

        /// <summary>
        /// Gets or sets the singleton instance of the <see cref="ApplicationSettings"/> object.
        /// </summary>
        public static ApplicationSettings Instance
        {
            get
            {
                if (instance == null) throw new InvalidOperationException("ApplicationSettings.Instance is null");
                return instance;
            }
            set { instance = value; }
        }

        /// <summary>
        /// Indicates whether the paster password need set
        /// </summary>
        public bool NeedMasterPassword { get; set; }

        /// <summary>
        /// Indicates whether the installation has been completed previously.
        /// </summary>
        public bool Installed { get; set; }

        /// <summary>
        /// "data" directory path
        /// </summary>
        /// <remarks>
        /// Set this item with your "data" directory path, which MUST have write permissions for the ASP.NET
        /// worker process.This path can be relative to the application root, or it can be an absolute path.This parameter is mandatory.
        /// </remarks>
        public string PublicDirectory { get; set; }

        /// <summary>
        /// The fully-qualified name of Global Settings Storage Provider
        /// </summary>
        /// <remarks>
        /// Set this item with the fully-qualified name of Global Settings Storage Provider you want to use,
        /// for example "MyNamespace.MyProvider, MyAssembly". The assembly should be placed inside the bin directory of the application
        /// or the public\Plugins directory.
        /// </remarks>
        public string GlobalSettingsStorageProvider { get; set; }

        /// <summary>
        /// The configuration for the Global Settings Storage Provider
        /// </summary>
        /// <remarks>
        /// Set this item with the configuration for the Global Settings Storage Provider defined above.
        /// The built-in providers does not require any configuration.
        /// </remarks>
        public string GlobalSettingsStorageProviderConfig { get; set; }

        /// <summary>
        /// Wikis
        /// </summary>
        /// <remarks>
        /// Define multiple wikis giving their name and host(s) (multiple hosts are accepted separated by a semicolon).
        /// The "root" wiki, which is MANDATORY, also acts as fallback for all unknown hosts.
        /// </remarks>
        public List<Wiki> Wikis { get; set; }

        #region StorageProviders

        /// <summary>
        /// Settings Storage Provider
        /// </summary>
        public StorageProvider SettingProvider { get; set; }
        /// <summary>
        /// Files Storage Provider
        /// </summary>
        public List<StorageProvider> FilesProviders { get; set; }
        /// <summary>
        /// Users Storage Provider
        /// </summary>
        public List<StorageProvider> UsersProviders { get; set; }
        /// <summary>
        /// Themes Storage Provider
        /// </summary>
        public List<StorageProvider> ThemesProviders { get; set; }
        /// <summary>
        /// Pages Storage Provider
        /// </summary>
        public List<StorageProvider> PagesProviders { get; set; }
        /// <summary>
        /// Index Directory Provider
        /// </summary>
        public StorageProvider IndexDirectoryProvider { get; set; }

        #endregion
    }
}
