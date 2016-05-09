using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Code.Extensions
{
    /// <summary>
    /// ScrewTurnWiki specific extensions methods for the <see cref="UrlHelper"/> class.
    /// </summary>
    public static class UrlHelperExtensions
    {
        private static string _assemblyVersion;

        /// <summary>
        /// Provides a CSS link tag for the CSS file provided. If the relative path does not begin with ~ then
        /// the Content/Css folder is assumed.
        /// </summary>
        public static MvcHtmlString CssLink(this UrlHelper helper, string relativePath)
        {
            string path = relativePath;

            if (!path.StartsWith("~"))
                path = "~/Content/CSS/" + relativePath;

            path = helper.Content(path);
            string html = string.Format("<link href=\"{0}?version={1}\" rel=\"stylesheet\" type=\"text/css\" />", path, GetAssemblyVersion());

            return MvcHtmlString.Create(html);
        }

        /// <summary>
        /// Provides a Javascript script tag for the Javascript file provided. If the relative path does not begin with ~ then
        /// the Content/Scripts folder is assumed.
        /// </summary>
        public static MvcHtmlString ScriptLink(this UrlHelper helper, string relativePath)
        {
            string path = relativePath;

            if (!path.StartsWith("~"))
                path = "~/Scripts/" + relativePath;

            path = helper.Content(path);
            string html = string.Format("<script type=\"text/javascript\" language=\"javascript\" src=\"{0}?version={1}\"></script>", path, GetAssemblyVersion());

            return MvcHtmlString.Create(html);
        }

        /// <summary>
        /// Provides a Javascript script tag for the installer Javascript file provided, using ~/Scripts/stw/installer as the base path.
        /// </summary>
        public static MvcHtmlString InstallerScriptLink(this UrlHelper helper, string filename)
        {
            string path = helper.Content("~/Scripts/stw/installer/" + filename);
            string html = string.Format("<script type=\"text/javascript\" language=\"javascript\" src=\"{0}?version={1}\"></script>", path, GetAssemblyVersion());

            return MvcHtmlString.Create(html);
        }

        /// <summary>
        /// Provides a Javascript script tag for the installer Javascript file provided, using ~/Scripts/stw/installer as the base path.
        /// </summary>
        public static MvcHtmlString StwScriptLink(this UrlHelper helper, string filename)
        {
            string path = helper.Content("~/Scripts/stw/common/" + filename);
            string html = string.Format("<script type=\"text/javascript\" language=\"javascript\" src=\"{0}?version={1}\"></script>", path, GetAssemblyVersion());

            return MvcHtmlString.Create(html);
        }

        ///// <summary>
        ///// Provides a CSS tag for the Bootstrap framework.
        ///// </summary>
        //public static MvcHtmlString BootstrapCSS(this UrlHelper helper)
        //{
        //    string path = helper.Content("~/Content/bootstrap/css/bootstrap.min.css");
        //    string html = string.Format("<link href=\"{0}?version={1}\" rel=\"stylesheet\" type=\"text/css\" />", path, GetAssemblyVersion());

        //    return MvcHtmlString.Create(html);
        //}

        ///// <summary>
        ///// Provides a Javascript script tag for the Bootstrap framework.
        ///// </summary>
        //public static MvcHtmlString BootstrapJS(this UrlHelper helper)
        //{
        //    string path = helper.Content("~/Content/bootstrap/js/bootstrap.min.js");
        //    string html = string.Format("<script type=\"text/javascript\" language=\"javascript\" src=\"{0}?version={1}\"></script>", path, GetAssemblyVersion());

        //    return MvcHtmlString.Create(html);
        //}

        private static string GetAssemblyVersion()
        {
            if (_assemblyVersion == null)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                _assemblyVersion = fvi.FileVersion;
            }
            return _assemblyVersion;
        }

    }
}