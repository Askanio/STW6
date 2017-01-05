using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Code.Attributes;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models.Admin;

namespace ScrewTurn.Wiki.Web.Controllers.Admin
{
    [AuthenticationRequired]
    public class BaseController : Controllers.BaseController
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public BaseController(ApplicationSettings settings) : base(settings)
        {
        }

        [NonAction]
        protected void PrepareModel(AdminBaseModel model, AdminMenu? selectedMenu = null)
        {
            model.Title = Messages.AdminTitle + " - " + Settings.GetWikiTitle(CurrentWiki);

            string currentUser = SessionFacade.GetCurrentUsername();
            string[] currentGroups = SessionFacade.GetCurrentGroupNames(CurrentWiki);

            // Categories (can manage categories in at least one NS)
            model.CategoriesAvailable = CanManageCategories(currentUser, currentGroups);


            var canManageConfiguration = CanManageConfiguration(currentUser, currentGroups);

            // Configuration (can manage config)
            model.ConfigAvailable = canManageConfiguration;

            // Home (can manage config)
            model.HomeAvailable = canManageConfiguration;


            // Content (can manage config)
            model.ContentAvailable = CanManageMetaFiles(currentUser, currentGroups);

            // Groups (can manage groups)
            model.GroupsAvailable = CanManageGroups(currentUser, currentGroups);

            // Namespaces (can manage namespaces)
            model.NamespacesAvailable = CanManageNamespaces(currentUser, currentGroups);

            // Nav. Paths (can manage pages in at least one NS)
            model.NavPathsAvailable = CanManagePages(currentUser, currentGroups);

            // Pages
            // Always displayed because checking every page can take too much time
            model.PagesAvailable = true;

            // Providers (can manage providers)
            model.PluginsAvailable = CanManageProviders(currentUser, currentGroups);

            // Snippets (can manage snippets)
            model.SnippetsAvailable = CanManageSnippetsAndTemplates(currentUser, currentGroups);

            // Accounts (can manage user accounts)
            model.UsersAvailable = CanManageUsers(currentUser, currentGroups);

            // TODO:
            model.ThemeAvailable = true;

            var canManageGlobalConfiguration = CanManageGlobalConfiguration(currentUser, currentGroups);

            // Log (can manage config)
            model.LogAvailable = canManageGlobalConfiguration;

            // Global Home (can manage global config)
            model.GlobalHomeAvailable = canManageGlobalConfiguration;

            // Global Configuration (can manage global config)
            model.GlobalConfigAvailable = canManageGlobalConfiguration;

            // Providers Management (can manage global config)
            model.ProvidersManagementAvailable = canManageGlobalConfiguration;

            // Import export (can manage global config)
            model.ImportExportAvailable = canManageGlobalConfiguration;


            foreach (var value in Enum.GetValues(typeof(AdminMenu)))
                model.MenuCssClass.Add((AdminMenu)value, "tab");

            if (selectedMenu != null && model.MenuCssClass.ContainsKey((AdminMenu)selectedMenu))
                model.MenuCssClass[(AdminMenu)selectedMenu] = "tabselected";
            // http://dbmast.ru/vertikalnoe-menyu-v-stile-akkordeon-css-i-jquery
            switch (selectedMenu)
            {
                case AdminMenu.MissingPages:
                case AdminMenu.OrphanPages:
                case AdminMenu.BulkEmail:
                    model.HomeDisplayStyle = "display: list-item;";
                    break;
                case AdminMenu.UserGroups:
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selectedMenu), selectedMenu, null);
            }
        }

        /// <summary>
        /// Determines whether a user can manage categories in at least one namespace.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the user can manage categories in at least one namespace, <c>false</c> otherwise.</returns>
        private bool CanManageCategories(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            if (authChecker.CheckActionForNamespace(null, Actions.ForNamespaces.ManageCategories, username, groups))
                return true;

            foreach (NamespaceInfo ns in Pages.GetNamespaces(CurrentWiki))
                if (authChecker.CheckActionForNamespace(ns, Actions.ForNamespaces.ManageCategories, username, groups))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines whether a user can manage the configuration.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the user can manage the configuration, <c>false</c> otherwise.</returns>
        private bool CanManageConfiguration(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            bool canManageConfiguration = authChecker.CheckActionForGlobals(Actions.ForGlobals.ManageConfiguration, username, groups);
            return canManageConfiguration;
        }

        /// <summary>
        /// Determines whether a user can manage the Meta-Files (Content).
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the user can manage the Meta-Files (Content), <c>false</c> otherwise.</returns>
        private bool CanManageMetaFiles(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            bool canManageMetaFiles = authChecker.CheckActionForGlobals(Actions.ForGlobals.ManageMetaFiles, username, groups);
            return canManageMetaFiles;
        }

        /// <summary>
        /// Determines whether a user can manage user groups.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the user can manage groups, <c>false</c> otherwise.</returns>
        private bool CanManageGroups(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            bool canManageGroups = authChecker.CheckActionForGlobals(Actions.ForGlobals.ManageGroups, username, groups);
            return canManageGroups;
        }

        /// <summary>
        /// Determines whether a user can manage namespaces.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the user can manage namespace, <c>false</c> otherwise.</returns>
        private bool CanManageNamespaces(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            bool canManageNamespaces = authChecker.CheckActionForGlobals(Actions.ForGlobals.ManageNamespaces, username, groups);
            return canManageNamespaces;
        }

        /// <summary>
        /// Determines whether a user can manage pages in at least one namespace.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the the user can manage pages in at least one namespace, <c>false</c> otherwise.</returns>
        private bool CanManagePages(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            if (authChecker.CheckActionForNamespace(null, Actions.ForNamespaces.ManagePages, username, groups))
                return true;

            foreach (NamespaceInfo ns in Pages.GetNamespaces(CurrentWiki))
                if (authChecker.CheckActionForNamespace(ns, Actions.ForNamespaces.ManagePages, username, groups))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines whether a user can manage providers.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the user can manage providers, <c>false</c> otherwise.</returns>
        private bool CanManageProviders(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            bool canManageProviders = authChecker.CheckActionForGlobals(Actions.ForGlobals.ManageProviders, username, groups);
            return canManageProviders;
        }

        /// <summary>
        /// Determines whether a user can manage snippets and templates.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the user can manage snippets and templates, <c>false</c> otherwise.</returns>
        private bool CanManageSnippetsAndTemplates(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            bool canManageSnippets = authChecker.CheckActionForGlobals(Actions.ForGlobals.ManageSnippetsAndTemplates, username, groups);
            return canManageSnippets;
        }

        /// <summary>
        /// Determines whether a user can manager user accounts.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the user can manage user accounts, <c>false</c> otherwise.</returns>
        private bool CanManageUsers(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            bool canManageUsers = authChecker.CheckActionForGlobals(Actions.ForGlobals.ManageAccounts, username, groups);
            return canManageUsers;
        }

        /// <summary>
        /// Determines whether a user can manage the global configuration.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the user can manage the global configuration, <c>false</c> otherwise.</returns>
        private bool CanManageGlobalConfiguration(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            bool canManageConfiguration = authChecker.CheckActionForGlobals(Actions.ForGlobals.ManageGlobalConfiguration, username, groups);
            return canManageConfiguration;
        }

        /// <summary>
        /// Determines whether a user can manage permissions.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="groups">The groups.</param>
        /// <returns><c>true</c> if the user can manage permissions, <c>false</c> otherwise.</returns>
        public bool CanManagePermissions(string username, string[] groups)
        {
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            bool canManagePermissions = authChecker.CheckActionForGlobals(Actions.ForGlobals.ManagePermissions, username, groups);
            return canManagePermissions;
        }

    }
}