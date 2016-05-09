using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Routing;
using ScrewTurn.Wiki.Configuration;

namespace ScrewTurn.Wiki.Web.Code.Files
{
    /// <summary>
    /// A route handler for the files. This route handler doesn't swallow MVC routes.
    /// </summary>
    public class AttachmentRouteHandler : IRouteHandler
    {
        /// <summary>
        /// Registers the attachments path route, using the settings given in the application settings.
        /// </summary>
        /// <param name="routes">The routes.</param>
        /// <exception cref="ConfigurationException">
        /// The configuration is missing an attachments route path.
        /// or
        /// The attachmentsRoutePath in the config is set to 'files' which is not an allowed route path.
        /// </exception>
        public static void RegisterRoute(RouteCollection routes)
        {
            Route route = new Route("getfile", new AttachmentRouteHandler());
            route.Constraints = new RouteValueDictionary();
            route.Constraints.Add("MvcContraint", new IgnoreMvcConstraint());

            routes.Add(route);
        }

        /// <summary>
        /// Provides the object that processes the request.
        /// </summary>
        /// <param name="requestContext">An object that encapsulates information about the request.</param>
        /// <returns>
        /// An object that processes the request.
        /// </returns>
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new FileHandler();
        }

        /// <summary>
        /// A route constraint for the attachments route, that ignores any controller or action route requests so only the
        /// /getfile routes get through.
        /// </summary>
        public class IgnoreMvcConstraint : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                if (routeDirection == RouteDirection.UrlGeneration)
                    return false;
                if (values.ContainsKey("controller") || values.ContainsKey("action"))
                    return false;

                if (route.Url.StartsWith("getfile"))
                    return true;
                else
                    return false;
            }
        }
    }
}