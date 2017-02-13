using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ScrewTurn.Wiki.Web.Controllers;

namespace ScrewTurn.Wiki.Web.Code.Attributes
{
    /// <summary>
    /// Check allow action for globals
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class CheckActionForGlobalsAttribute : ActionFilterAttribute
    {
        public ActionForGlobals Action { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext?.HttpContext == null)
                return;

            var controller = filterContext.Controller as BaseController;
            if (controller == null)
            {
#if DEBUG
                throw new Exception("AllowActionForGlobals: Controller = null");
#endif
                return;
            }

            string currentUsername = SessionFacade.GetCurrentUsername();
            string[] currentGroups = SessionFacade.GetCurrentGroupNames(controller.CurrentWiki);

            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(controller.CurrentWiki));

            bool allow = authChecker.CheckActionForGlobals(GetAction(Action), currentUsername, currentGroups);

            if (!allow)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"controller", "Common"},
                        {"action", "AccessDenied"}
                    });
            }
        }

        private string GetAction(ActionForGlobals action)
        {
            switch (action)
            {
                case ActionForGlobals.ManageAccounts:
                    return Actions.ForGlobals.ManageAccounts;
                case ActionForGlobals.ManageGroups:
                    return Actions.ForGlobals.ManageGroups;
                case ActionForGlobals.ManagePagesAndCategories:
                    return Actions.ForGlobals.ManagePagesAndCategories;
                case ActionForGlobals.ManageDiscussions:
                    return Actions.ForGlobals.ManageDiscussions;
                case ActionForGlobals.ManageNamespaces:
                    return Actions.ForGlobals.ManageNamespaces;
                case ActionForGlobals.ManageConfiguration:
                    return Actions.ForGlobals.ManageConfiguration;
                case ActionForGlobals.ManageProviders:
                    return Actions.ForGlobals.ManageProviders;
                case ActionForGlobals.ManageFiles:
                    return Actions.ForGlobals.ManageFiles;
                case ActionForGlobals.ManageSnippetsAndTemplates:
                    return Actions.ForGlobals.ManageSnippetsAndTemplates;
                case ActionForGlobals.ManageNavigationPaths:
                    return Actions.ForGlobals.ManageNavigationPaths;
                case ActionForGlobals.ManageMetaFiles:
                    return Actions.ForGlobals.ManageMetaFiles;
                case ActionForGlobals.ManagePermissions:
                    return Actions.ForGlobals.ManagePermissions;
                case ActionForGlobals.ManageGlobalConfiguration:
                    return Actions.ForGlobals.ManageGlobalConfiguration;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        public enum ActionForGlobals
        {
            ManageAccounts,
            ManageGroups,
            ManagePagesAndCategories,
            ManageDiscussions,
            ManageNamespaces,
            ManageConfiguration,
            ManageProviders,
            ManageFiles,
            ManageSnippetsAndTemplates,
            ManageNavigationPaths,
            ManageMetaFiles,
            ManagePermissions,
            ManageGlobalConfiguration,
        }
    }
}