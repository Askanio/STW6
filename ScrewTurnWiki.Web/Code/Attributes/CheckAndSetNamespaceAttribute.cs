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
    /// Check and set namespace to <see cref="PageController"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CheckAndSetNamespaceAttribute : ActionFilterAttribute
    {
        public string NamespaceParamName { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as PageController;
            if (controller == null)
                return;

            if (NamespaceParamName == null)
                return;

            var nspace = "";
            if (NamespaceParamName != null && filterContext.ActionParameters.ContainsKey(NamespaceParamName))
                nspace = filterContext.ActionParameters[NamespaceParamName] as string;

            if (PageHelper.ExistsCurrentNamespace(controller.CurrentWiki, nspace))
            {
                controller.CurrentNamespace = nspace;
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"controller", "Common"},
                        {"action", "PageNotFound"},
                        {"page", nspace}
                    });
            }
        }

    }
}