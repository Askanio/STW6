using System.Configuration;

namespace ScrewTurn.Wiki.Configuration
{
    /// <summary>
    /// Config file settings - represents a &lt;ScrewTurnWiki&gt; section inside a configuration file.
    /// </summary>
    public class ScrewTurnWikiSection : ConfigurationSection
    {
        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("stwSettings", IsDefaultCollection = false, IsRequired = true)]
        [ConfigurationCollection(typeof(SettingElementCollection<SettingElement>), RemoveItemName = "remove", ClearItemsName = "clear", AddItemName = "add")]
        public SettingElementCollection<SettingElement> StwSettings
        {
            get { return (SettingElementCollection<SettingElement>)base["stwSettings"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("wikiList", IsDefaultCollection = false, IsRequired = true)]
        [ConfigurationCollection(typeof(SettingElementCollection<WikiElement>), RemoveItemName = "remove", ClearItemsName = "clear", AddItemName = "add")]
        public SettingElementCollection<WikiElement> WikiList
        {
            get { return (SettingElementCollection<WikiElement>)base["wikiList"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("settingsProvider", IsDefaultCollection = false, IsRequired = true)]
        [ConfigurationCollection(typeof(SettingElementCollection<ProviderSettingElement>), RemoveItemName = "remove", ClearItemsName = "clear", AddItemName = "add")]
        public SettingElementCollection<ProviderSettingElement> SettingsProvider
        {
            get { return (SettingElementCollection<ProviderSettingElement>)base["settingsProvider"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("filesProviders", IsDefaultCollection = false, IsRequired = true)]
        [ConfigurationCollection(typeof(SettingElementCollection<ProviderSettingElement>), RemoveItemName = "remove", ClearItemsName = "clear", AddItemName = "add")]
        public SettingElementCollection<ProviderSettingElement> FilesProviders
        {
            get { return (SettingElementCollection<ProviderSettingElement>)base["filesProviders"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("usersProviders", IsDefaultCollection = false, IsRequired = true)]
        [ConfigurationCollection(typeof(SettingElementCollection<ProviderSettingElement>), RemoveItemName = "remove", ClearItemsName = "clear", AddItemName = "add")]
        public SettingElementCollection<ProviderSettingElement> UsersProviders
        {
            get { return (SettingElementCollection<ProviderSettingElement>)base["usersProviders"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("themesProviders", IsDefaultCollection = false, IsRequired = true)]
        [ConfigurationCollection(typeof(SettingElementCollection<ProviderSettingElement>), RemoveItemName = "remove", ClearItemsName = "clear", AddItemName = "add")]
        public SettingElementCollection<ProviderSettingElement> ThemesProviders
        {
            get { return (SettingElementCollection<ProviderSettingElement>)base["themesProviders"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("pagesProviders", IsDefaultCollection = false, IsRequired = true)]
        [ConfigurationCollection(typeof(SettingElementCollection<ProviderSettingElement>), RemoveItemName = "remove", ClearItemsName = "clear", AddItemName = "add")]
        public SettingElementCollection<ProviderSettingElement> PagesProviders
        {
            get { return (SettingElementCollection<ProviderSettingElement>)base["pagesProviders"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("indexDirectoryProviders", IsDefaultCollection = false, IsRequired = true)]
        [ConfigurationCollection(typeof(SettingElementCollection<ProviderSettingElement>), RemoveItemName = "remove", ClearItemsName = "clear", AddItemName = "add")]
        public SettingElementCollection<ProviderSettingElement> IndexDirectoryProviders
        {
            get { return (SettingElementCollection<ProviderSettingElement>)base["indexDirectoryProviders"]; }
        }

    }

}
