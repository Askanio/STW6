using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using NUnit.Framework.Constraints;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Web.Code.Providers
{
    public class ProviderHelper
    {

        /// <summary>
        /// Loads the providers list.
        /// </summary>
        /// <param name="currentWiki"></param>
        /// <param name="providerType">The provider type</param>
        /// <param name="usersProviderIntendedUse">The intended use of users storage provider, if applicable</param>
        /// <param name="excludeReadOnly">The value indicating whether to exclude read-only providers</param>
        /// <returns></returns>
        public static List<SelectListItem> GetProviders(string currentWiki, ProviderType providerType, UsersProviderIntendedUse usersProviderIntendedUse, bool excludeReadOnly)
        {
            IProviderV60[] allProviders = null;
            string defaultProvider = null;
            switch (providerType)
            {
                case ProviderType.Users:
                    allProviders = Collectors.CollectorsBox.UsersProviderCollector.GetAllProviders(currentWiki);
                    defaultProvider = GlobalSettings.DefaultUsersProvider;
                    break;
                case ProviderType.Pages:
                    allProviders = Collectors.CollectorsBox.PagesProviderCollector.GetAllProviders(currentWiki);
                    defaultProvider = GlobalSettings.DefaultPagesProvider;
                    break;
                case ProviderType.Themes:
                    allProviders = Collectors.CollectorsBox.ThemesProviderCollector.GetAllProviders(currentWiki);
                    break;
                case ProviderType.Files:
                    allProviders = Collectors.CollectorsBox.FilesProviderCollector.GetAllProviders(currentWiki);
                    defaultProvider = GlobalSettings.DefaultFilesProvider;
                    break;
                default:
                    throw new NotSupportedException();
            }

            var providers = new List<SelectListItem>();

            int count = 0;
            if (providerType == ProviderType.Themes) providers.Add(new SelectListItem() { Text = @"standard", Value = "standard"});
            foreach (IProviderV60 prov in allProviders)
            {
                if (IsProviderIncludedInList(prov, providerType, usersProviderIntendedUse, excludeReadOnly))
                {
                    string typeName = prov.GetType().FullName;
                    providers.Add(new SelectListItem() { Text = prov.Information.Name, Value = typeName});
                    if (typeName == defaultProvider) providers[count].Selected = true;
                    count++;
                }
            }
            return providers;
        }

        /// <summary>
        /// Detectes whether a provider is included in the list.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="providerType"></param>
        /// <param name="usersProviderIntendedUse"></param>
        /// <param name="excludeReadOnly"></param>
        /// <returns><c>true</c> if the provider is included, <c>false</c> otherwise.</returns>
        private static bool IsProviderIncludedInList(IProviderV60 provider, ProviderType providerType, UsersProviderIntendedUse usersProviderIntendedUse, bool excludeReadOnly)
        {
            IStorageProviderV60 storageProvider = provider as IStorageProviderV60;
            IUsersStorageProviderV60 usersProvider = provider as IUsersStorageProviderV60;

            switch (providerType)
            {
                case ProviderType.Users:
                    return IsUsersProviderIncludedInList(usersProvider, usersProviderIntendedUse, excludeReadOnly);
                case ProviderType.Pages:
                    return storageProvider == null || (!storageProvider.ReadOnly || storageProvider.ReadOnly && !excludeReadOnly);
                case ProviderType.Files:
                    return storageProvider == null || (!storageProvider.ReadOnly || storageProvider.ReadOnly && !excludeReadOnly);
                case ProviderType.Themes:
                    return storageProvider == null || (!storageProvider.ReadOnly || storageProvider.ReadOnly && !excludeReadOnly);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Detects whether a users provider is included in the list.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="usersProviderIntendedUse"></param>
        /// <param name="excludeReadOnly"></param>
        /// <returns><c>true</c> if the provider is included, <c>false</c> otherwise.</returns>
        private static bool IsUsersProviderIncludedInList(IUsersStorageProviderV60 provider, UsersProviderIntendedUse usersProviderIntendedUse, bool excludeReadOnly)
        {
            switch (usersProviderIntendedUse)
            {
                case UsersProviderIntendedUse.AccountsManagement:
                    return !provider.UserAccountsReadOnly || (provider.UserAccountsReadOnly && !excludeReadOnly);
                case UsersProviderIntendedUse.GroupsManagement:
                    return !provider.UserGroupsReadOnly || (provider.UserGroupsReadOnly && !excludeReadOnly);
                default:
                    throw new NotSupportedException();
            }
        }

    }
}