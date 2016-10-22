using System;
using System.Web.Mvc;
using System.Web.Routing;
using ScrewTurn.Wiki.Web.Controllers;

namespace ScrewTurn.Wiki.Web.Code.Attributes
{
    /// <summary>
    /// Detect namespace and set it to SpecialPageController's parametrs
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DetectNamespaceFromPageAttribute : ActionFilterAttribute
    {
        public string PageParamName { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as PageController;
            if (controller == null)
                return;

            var pageName = "";
            if (PageParamName != null && filterContext.ActionParameters.ContainsKey(PageParamName))
                pageName = filterContext.ActionParameters[PageParamName] as string;


            // Try to detect current namespace
            string currentNamespace = PageHelper.DetectNamespace(pageName);

            if (PageHelper.ExistsCurrentNamespace(controller.CurrentWiki, currentNamespace))
            {
                controller.CurrentNamespace = currentNamespace;
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"controller", "Common"},
                        {"action", "PageNotFound"},
                        {"page", pageName}
                    });
            }
        }

    }
}