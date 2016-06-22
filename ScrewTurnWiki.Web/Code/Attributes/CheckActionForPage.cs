using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ScrewTurn.Wiki.Web.Controllers;

namespace ScrewTurn.Wiki.Web.Code.Attributes
{
    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = true)]
    public class CheckActionForPage : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as PageController;
            if (controller == null || controller.CurrentPage == null)
                return;

            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(controller.CurrentWiki));

            bool allow = authChecker.CheckActionForPage(controller.CurrentPage.FullName, Actions.ForPages.ReadPage,
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
    }
}