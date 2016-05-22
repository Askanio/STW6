using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ScrewTurn.Wiki.Web.Code.Files;

namespace ScrewTurn.Wiki.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Installer",
                "Install/{action}",
                new { controller = "Install" }
                );

            // http://www.prideparrot.com/blog/archive/2012/7/understanding_routing

            // Attachment files
            AttachmentRouteHandler.RegisterRoute(routes);

            //routes.MapRoute(
            //    "CreateMasterPassword",
            //    "user/CreateMasterPassword",
            //    new {controller = "User", action = "CreateMasterPassword"}
            //    );

            routes.MapRoute(
                "Error",
                "Error/{ns}",
                new {controller = "Common", action = "Error", ns = UrlParameter.Optional }
                );

            routes.MapRoute(
                "PageNotFound",
                "PageNotFound/{page}",
                new {controller = "Common", action = "PageNotFound", page = UrlParameter.Optional}
                );

            routes.MapRoute(
                "PageViewCode",
                "{page}/Code",
                defaults: new {controller = "Wiki", action = "PageViewCode" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET")
                }
                );

            routes.MapRoute(
                "PageDiscuss",
                "{page}/Discuss",
                defaults: new {controller = "Wiki", action = "PageDiscuss" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET")
                }
                );

            routes.MapRoute(
                "Page",
                "{page}",
                defaults: new {controller = "Wiki", action = "Page"},
                constraints: new
                {
                    //id = "[^?<>|:*&%'#\"\\/+].*", // []
                    httpMethod = new HttpMethodConstraint("GET")
                }
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Wiki", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
