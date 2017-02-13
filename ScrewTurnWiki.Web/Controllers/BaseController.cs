using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// The name of the current wiki using the <b>Wiki</b> parameter in the query string.
        /// </summary>
        public string CurrentWiki
        {
            get
            {
                if (_currentWiki == null)
                    _currentWiki = Tools.DetectCurrentWiki();
                return _currentWiki;
            }
        }
        private string _currentWiki = null;

        /// <summary>
        /// 
        /// </summary>
        public ApplicationSettings AppSettings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        /// <param name="appSettings"></param>
        protected BaseController(ApplicationSettings appSettings)
        {
            AppSettings = appSettings;
        }

        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        public virtual string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = this.ControllerContext.RouteData.GetRequiredString("action");

            this.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = System.Web.Mvc.ViewEngines.Engines.FindPartialView(this.ControllerContext, viewName);
                var viewContext = new ViewContext(this.ControllerContext, viewResult.View, this.ViewData, this.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Called before the action method is invoked.
        /// </summary>
        /// <param name="filterContext">Information about the current request and action.</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Redirect if ScrewTurnWiki isn't installed or an upgrade is needed.
            if (!AppSettings.Installed)
            {
                if (!(filterContext.Controller is InstallController))
                    filterContext.Result = new RedirectResult(this.Url.Action("Index", "Install"));
                return;
            }
            if (AppSettings.Installed && AppSettings.NeedMasterPassword)
            {
                if (!(filterContext.Controller is InstallController))
                    filterContext.Result = new RedirectResult(this.Url.Action("Step4", "Install"));
                return;
            }
            //else if (ApplicationSettings.UpgradeRequired)
            //{
            //    if (!(filterContext.Controller is UpgradeController))
            //        filterContext.Result = new RedirectResult(this.Url.Action("Index", "Upgrade"));

            //    return;
            //}

            InitializeCulture();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            // Log the route data values
            List<string> routeData = new List<string>();
            foreach (string key in filterContext.RouteData.Values.Keys)
                routeData.Add(string.Format("'{0}' : '{1}'", key, filterContext.RouteData.Values[key]));

            string routeInfo = string.Join(", ", routeData);
            var message = $"MVC error caught. Route data: [{routeInfo}] - {filterContext.Exception.Message}\n{filterContext.Exception.ToString()}";
            Log.LogEntry(message, PluginFramework.EntryType.Error, Log.SystemUsername, null);

            base.OnException(filterContext);
        }

        private void InitializeCulture()
        {
            // First, look for hard-stored user preferences
            // If they are not available, look at the cookie

            string culture = Preferences.LoadLanguageFromUserData(CurrentWiki);
            if (culture == null) culture = Preferences.LoadLanguageFromCookie();

            if (culture != null)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            }
            else
            {
                try
                {
                    if (Settings.GetDefaultLanguage(CurrentWiki).Equals("-"))
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    }
                    else
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.GetDefaultLanguage(CurrentWiki));
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.GetDefaultLanguage(CurrentWiki));
                    }
                }
                catch
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                }
            }
        }
    }
}