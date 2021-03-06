﻿using System;
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

            routes.MapMvcAttributeRoutes();

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
                "Login",
                "Login",
                new { controller = "User", action = "Login" }
                );

            //routes.MapRoute(
            //    "ForceLogout",
            //    "ForceLogout",
            //    new {controller = "User", action = "ForceLogout" }
            //    );

            //routes.MapRoute(
            //    "Activate",
            //    "Activate",
            //    new {controller = "User", action = "Activate" }
            //    );

            routes.MapRoute(
                "User",
                "User",
                new { controller = "User", action = "User" }
                );

            routes.MapRoute(
                "UserProfile",
                "User/Profile",
                new { controller = "User", action = "UserProfile" }
                );


            routes.MapRoute(
                "Sitemap",
                "Sitemap",
                new { controller = "Common", action = "Sitemap" }
                );

            routes.MapRoute(
                "Error",
                "Error/{ns}",
                new {controller = "Common", action = "Error", ns = UrlParameter.Optional }
                );

            routes.MapRoute(
                "AccessDenied",
                "AccessDenied",
                new {controller = "Common", action = "AccessDenied"}
                );

            routes.MapRoute(
                "Language",
                "Language",
                new {controller = "User", action = "Language"}
                );

            routes.MapRoute(
                "RandPage",
                "RandPage",
                new {controller = "Common", action = "RandPage" }
                );

            routes.MapRoute(
                "Opensearch",
                "Search/Opensearch",
                new {controller = "Search", action = "GetOpenSearchDescription" }
                );

            routes.MapRoute(
                "PageNotFound",
                "PageNotFound/{page}",
                new {controller = "Common", action = "PageNotFound", page = UrlParameter.Optional}
                );

            //routes.MapRoute(
            //    "AllPages1",
            //    "AllPages/{ns}/{page}",
            //    new {controller = "Common", action = "Index", page = UrlParameter.Optional}
            //    );

            routes.MapRoute(
                "Rss",
                "{rssPage}",
                new {controller = "Rss", action = "Index"},
                new {page = @"^(.*\.|)RSS$"}
                );

            routes.MapRoute(
                "Search",
                "{page}",
                new {controller = "Search", action = "Search" },
                new {page = @"^(.*\.|)Search$" }
                );

            routes.MapRoute(
                "AllPages",
                "{page}",
                new {controller = "AllPages", action = "GetAllPages" },
                new { page = @"^(.*\.|)AllPages$" }
                );

            routes.MapRoute(
                "Category",
                "{page}",
                new {controller = "Category", action = "GetCategory" },
                new {page = @"^(.*\.|)Category$" }
                );

            routes.MapRoute(
                "PageViewCode",
                "{page}/Code",
                defaults: new {controller = "Wiki", action = "PageViewCode" }
                );

            routes.MapRoute(
                "PageDiscuss",
                "{page}/Discuss",
                defaults: new {controller = "Wiki", action = "PageDiscuss" }
                );

            routes.MapRoute(
                "PageHistory",
                "{page}/History",
                defaults: new {controller = "History", action = "GetHistory" }
                );

            routes.MapRoute(
                "PageHistoryRollback",
                "{page}/Rollback",
                defaults: new {controller = "History", action = "Rollback" }
                );

            routes.MapRoute(
                "PageHistoryDiff",
                "{page}/Diff",
                defaults: new {controller = "History", action = "Diff"}
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
