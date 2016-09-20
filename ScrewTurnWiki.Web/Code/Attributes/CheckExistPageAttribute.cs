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
    /// Check exists page and set it to PageController's parametrs
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CheckExistPageAttribute : ActionFilterAttribute
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

            string notFoundPageName;
            if (!CheckPage(pageName, controller, out notFoundPageName))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"controller", "Common"},
                        {"action", "PageNotFound"},
                        {"page", notFoundPageName}
                    });
            }
        }

        /// <summary>
        /// Detect and set CurrentNamespace and CurrentPage
        /// </summary>
        /// <param name="pageName"></param>
        /// <param name="controller"></param>
        /// <param name="notFoundPageName">Name of Page for redirect if return false</param>
        /// <returns>true - ok, false - CurrentNamespace or CurrentPage not exists</returns>
        private bool CheckPage(string pageName, PageController controller, out string notFoundPageName)
        {
            // Try to detect current namespace
            string currentNamespace = PageHelper.DetectNamespace(pageName);

            if (!PageHelper.ExistsCurrentNamespace(controller.CurrentWiki, currentNamespace))
            {
                notFoundPageName = pageName;
                return false;
            }

            var currentPageFullName = GetCurrentPageFullName(controller.CurrentWiki, pageName, currentNamespace);
            
            var currentPage = Pages.FindPage(controller.CurrentWiki, currentPageFullName);

            // Verifies the need for a redirect and performs it.
            if (currentPage == null)
            {
                notFoundPageName = Tools.UrlEncode(currentPageFullName);
                return false;
            }

            controller.CurrentPage = currentPage;
            controller.CurrentPageFullName = currentPageFullName;
            controller.CurrentNamespace = currentNamespace;

            notFoundPageName = null;
            return true;
        }

        private string GetCurrentPageFullName(string currentWiki, string pageName, string currentNamespace)
        {
            // Trim Namespace. from pageName
            if (!string.IsNullOrEmpty(currentNamespace))
                pageName = pageName.Substring(currentNamespace.Length + 1);

            if (string.IsNullOrEmpty(pageName) || pageName == "Default")
                pageName = Settings.GetDefaultPage(currentWiki);

            return string.IsNullOrEmpty(currentNamespace)
                ? pageName
                : $"{currentNamespace}.{pageName}";
        }

    }
}