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
    /// Check allow action for namespace (Only after <see cref="CheckExistPage"/> or <see cref="DetectNamespaceFromPageAttribute"/>)
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = true)]
    public class CheckAllowNamespaceActionAttribute : ActionFilterAttribute
    {
        public ActionForNamespaces Action { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext?.HttpContext == null)
                return;

            var controller = filterContext.Controller as BaseController;
            if (controller == null)
            {
#if DEBUG
                throw new Exception("AllowNamespaceAction: Controller = null");
#endif
                return;
            }
            
            string currentUsername = SessionFacade.GetCurrentUsername();
            string[] currentGroups = SessionFacade.GetCurrentGroupNames(controller.CurrentWiki);

            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(controller.CurrentWiki));

            bool allow = authChecker.CheckActionForNamespace(controller.DetectNamespaceInfo(), GetAction(Action),
                currentUsername, currentGroups);

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


        private string GetAction(ActionForNamespaces action)
        {
            switch (Action)
            {
                case ActionForNamespaces.ReadPages:
                    return Actions.ForNamespaces.ReadPages;
                //case ActionForNamespaces.CreatePages:
                //    break;
                //case ActionForNamespaces.ModifyPages:
                //    break;
                //case ActionForNamespaces.DeletePages:
                //    break;
                //case ActionForNamespaces.ManagePages:
                //    break;
                //case ActionForNamespaces.ReadDiscussion:
                //    break;
                //case ActionForNamespaces.PostDiscussion:
                //    break;
                //case ActionForNamespaces.ManageDiscussion:
                //    break;
                //case ActionForNamespaces.ManageCategories:
                //    break;
                //case ActionForNamespaces.DownloadAttachments:
                //    break;
                //case ActionForNamespaces.UploadAttachments:
                //    break;
                //case ActionForNamespaces.DeleteAttachments:
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        public enum ActionForNamespaces
        {
            ReadPages,
            CreatePages,
            ModifyPages,
            DeletePages,
            ManagePages,
            ReadDiscussion,
            PostDiscussion,
            ManageDiscussion,
            ManageCategories,
            DownloadAttachments,
            UploadAttachments,
            DeleteAttachments
        }
    }
}