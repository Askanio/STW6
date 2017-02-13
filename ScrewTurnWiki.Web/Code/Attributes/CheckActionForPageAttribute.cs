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
    public class CheckActionForPageAttribute : ActionFilterAttribute
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

            string currentUsername = SessionFacade.GetCurrentUsername();
            string[] currentGroups = SessionFacade.GetCurrentGroupNames(controller.CurrentWiki);

            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(controller.CurrentWiki));

            bool allow = authChecker.CheckActionForPage(controller.CurrentPage.FullName, GetAction(Action),
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

        //private List<String> GetActions()
        //{
        //    var result = new List<String>();
        //    if (Action.HasFlag(ActionForPages.ReadPage))
        //        result.Add(Actions.ForPages.ReadPage);
        //    if (Action.HasFlag(ActionForPages.ModifyPage))
        //        result.Add(Actions.ForPages.ModifyPage);
        //    if (Action.HasFlag(ActionForPages.ManagePage))
        //        result.Add(Actions.ForPages.ManagePage);
        //    if (Action.HasFlag(ActionForPages.ReadDiscussion))
        //        result.Add(Actions.ForPages.ReadDiscussion);
        //    if (Action.HasFlag(ActionForPages.PostDiscussion))
        //        result.Add(Actions.ForPages.PostDiscussion);
        //    if (Action.HasFlag(ActionForPages.ManageDiscussion))
        //        result.Add(Actions.ForPages.ManageDiscussion);
        //    if (Action.HasFlag(ActionForPages.ManageCategories))
        //        result.Add(Actions.ForPages.ManageCategories);
        //    if (Action.HasFlag(ActionForPages.DownloadAttachments))
        //        result.Add(Actions.ForPages.DownloadAttachments);
        //    if (Action.HasFlag(ActionForPages.UploadAttachments))
        //        result.Add(Actions.ForPages.UploadAttachments);
        //    if (Action.HasFlag(ActionForPages.DeleteAttachments))
        //        result.Add(Actions.ForPages.DeleteAttachments);
        //    return result;
        //}

        //[Flags]
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