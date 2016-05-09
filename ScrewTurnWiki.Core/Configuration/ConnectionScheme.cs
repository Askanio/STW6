using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrewTurn.Wiki.Configuration
{
    /// <summary>
    /// It describes the existing schemes of connections to the database
    /// </summary>
    public class ConnectionScheme
    {
        /// <summary>
        /// SqlServer connection scheme
        /// </summary>
        public static readonly ConnectionScheme SqlServer = new ConnectionScheme("SqlServer", "A SqlServer database.",
            "ScrewTurn.Wiki.Plugins.SqlServer.SqlServerGlobalSettingsStorageProvider, SqlServerProviders",
            "ScrewTurn.Wiki.Plugins.SqlServer.SqlServerSettingsStorageProvider, SqlServerProviders",
            "ScrewTurn.Wiki.Plugins.FSProviders.FilesStorageProvider, FSProviders",
            "ScrewTurn.Wiki.Plugins.SqlServer.SqlServerUsersStorageProvider, SqlServerProviders",
            "ScrewTurn.Wiki.Plugins.FSProviders.ThemesStorageProvider, FSProviders",
            "ScrewTurn.Wiki.Plugins.SqlServer.SqlServerPagesStorageProvider, SqlServerProviders",
            "ScrewTurn.Wiki.Plugins.SqlServer.SqlServerIndexDirectoryProvider, SqlServerProviders");

        /// <summary>
        /// SqlServerCE connection scheme
        /// </summary>
        public static readonly ConnectionScheme SqlServerCE = new ConnectionScheme("SqlServerCE", "A SqlServer CE 4 database.",
            "ScrewTurn.Wiki.Plugins.FSProviders.SqlCEGlobalSettingsStorageProvider, FSProviders",
            "ScrewTurn.Wiki.Plugins.FSProviders.SqlCESettingsStorageProvider, FSProviders",
            "ScrewTurn.Wiki.Plugins.FSProviders.FilesStorageProvider, FSProviders",
            "ScrewTurn.Wiki.Plugins.FSProviders.SqlCEUsersStorageProvider, FSProviders",
            "ScrewTurn.Wiki.Plugins.FSProviders.ThemesStorageProvider, FSProviders",
            "ScrewTurn.Wiki.Plugins.FSProviders.SqlCEPagesStorageProvider, FSProviders",
            "ScrewTurn.Wiki.Plugins.FSProviders.FSIndexDirectoryProvider, FSProviders");

        /// <summary>
        /// Microsoft Azure connection scheme
        /// </summary>
        public static readonly ConnectionScheme Azure = new ConnectionScheme("Microsoft Azure", "A Microsoft Azure storage.",
            "ScrewTurn.Wiki.Plugins.AzureStorage.AzureGlobalSettingsStorageProvider, AzureStorageProviders",
            "ScrewTurn.Wiki.Plugins.AzureStorage.AzureSettingsStorageProvider, AzureStorageProviders",
            "ScrewTurn.Wiki.Plugins.AzureStorage.AzureFilesStorageProvider, AzureStorageProviders",
            "ScrewTurn.Wiki.Plugins.AzureStorage.AzureUsersStorageProvider, AzureStorageProviders",
            "ScrewTurn.Wiki.Plugins.AzureStorage.AzureThemesStorageProvider, AzureStorageProviders",
            "ScrewTurn.Wiki.Plugins.AzureStorage.AzurePagesStorageProvider, AzureStorageProviders",
            "ScrewTurn.Wiki.Plugins.AzureStorage.AzureIndexDirectoryProvider, AzureStorageProviders");

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string GlobalSettingsStorageProvider { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string SettingProvider { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string FilesProvider { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string UsersProvider { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string ThemesProvider { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string PagesProvider { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string IndexDirectoryProvider { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionScheme"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="globalSettingsStorageProvider"></param>
        /// <param name="settingProvider"></param>
        /// <param name="filesProvider"></param>
        /// <param name="usersProvider"></param>
        /// <param name="themesProvider"></param>
        /// <param name="pagesProvider"></param>
        /// <param name="indexDirectoryProvider"></param>
        public ConnectionScheme(string name, string description, string globalSettingsStorageProvider, string settingProvider, string filesProvider, string usersProvider, string themesProvider, string pagesProvider, string indexDirectoryProvider)
        {
            Name = name;
            Description = description;
            GlobalSettingsStorageProvider = globalSettingsStorageProvider;
            SettingProvider = settingProvider;
            FilesProvider = filesProvider;
            UsersProvider = usersProvider;
            ThemesProvider = themesProvider;
            PagesProvider = pagesProvider;
            IndexDirectoryProvider = indexDirectoryProvider;
        }

        static ConnectionScheme()
        {
            AllSchemes = new List<ConnectionScheme>() { SqlServer, SqlServerCE, Azure };
        }

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<ConnectionScheme> AllSchemes { get; internal set; }

    }
}
