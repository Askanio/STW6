using System;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;

namespace ScrewTurn.Wiki.Web.Code.Attributes
{
    /// <summary>
    /// Check Authentication
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthenticationRequiredAttribute : FilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            if (SessionFacade.LoginKey == null)
                filterContext.Result = new HttpUnauthorizedResult();
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (filterContext.Result == null || filterContext.Result is HttpUnauthorizedResult)
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    {"controller", "User"},
                    {"action",  "Login"},
                    {"returnUrl", filterContext.HttpContext.Request.RawUrl}
                });
        }
    }
}