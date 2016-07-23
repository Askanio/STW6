#define DEBUG
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
    /// Check allow action for page (Only after <see cref="CheckExistPage"/>)
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = true)]
    public class AllowActionForPage : ActionFilterAttribute
    {

        public ActionForPages Action { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext?.HttpContext == null)
                return;

            var controller = filterContext.Controller as PageController;
            if (controller?.CurrentPage == null)
            {
#if DEBUG
                throw new Exception("AllowActionForPage: Controller = null");
#endif
                return;
            }


            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(controller.CurrentWiki));

            bool allow = authChecker.CheckActionForPage(controller.CurrentPage.FullName, GetAction(Action),
                SessionFacade.GetCurrentUsername(), SessionFacade.GetCurrentGroupNames(controller.CurrentWiki));

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

        private string GetAction(ActionForPages action)
        {
            switch (action)
            {
                case ActionForPages.ReadPage:
                    return Actions.ForPages.ReadPage;
                case ActionForPages.ModifyPage:
                    return Actions.ForPages.ModifyPage;
                case ActionForPages.ManagePage:
                    return Actions.ForPages.ManagePage;
                //case ActionForPages.ReadDiscussion:
                //    break;
                //case ActionForPages.PostDiscussion:
                //    break;
                //case ActionForPages.ManageDiscussion:
                //    break;
                //case ActionForPages.ManageCategories:
                //    break;
                //case ActionForPages.DownloadAttachments:
                //    break;
                //case ActionForPages.UploadAttachments:
                //    break;
                //case ActionForPages.DeleteAttachments:
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        public enum ActionForPages
        {
            ReadPage,
            ModifyPage,
            ManagePage,
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