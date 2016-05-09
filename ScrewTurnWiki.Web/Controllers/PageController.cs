using ScrewTurn.Wiki.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Models;

namespace ScrewTurn.Wiki.Web.Controllers
{
    /// <summary>
    /// A base controller for all ScrewTurnWiki controller classes.
    /// </summary>
    public class PageController : BaseController
    {
        /// <summary>
        /// The name of the current wiki using the <b>Wiki</b> parameter in the query string.
        /// </summary>
        protected string CurrentWiki { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ApplicationSettings AppSettings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageController"/> class.
        /// </summary>
        /// <param name="appSettings"></param>
        public PageController(ApplicationSettings appSettings)
        {
            AppSettings = appSettings;
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

            CurrentWiki = Tools.DetectCurrentWiki();

            InitializeCulture();
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
            else {
                try
                {
                    if (Settings.GetDefaultLanguage(CurrentWiki).Equals("-"))
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    }
                    else {
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

        #region Default

        [NonAction]
        protected void PrepareDefaultModel(WikiBaseModel model, string currentNamespace, string currentPageFullName)
        {
            FillDefaultHtmlHead(model, currentNamespace);
            FillDefaultHeader(model, currentNamespace, currentPageFullName);
            FillDefaultSidebar(model, currentNamespace, currentPageFullName);
            FillDefaultFooter(model, currentNamespace, currentPageFullName);
            FillDefaultPageHeaderAndFooter(model, currentNamespace, currentPageFullName);
        }

        /// <summary>
        /// Prints the page header and page footer.
        /// </summary>
        public void FillDefaultPageHeaderAndFooter(WikiBaseModel model, string currentNamespace, string currentPageFullName)
        {
            string h = Settings.GetProvider(CurrentWiki).GetMetaDataItem(MetaDataItem.PageHeader, currentNamespace);
            h = @"<div id=""PageInternalHeaderDiv"">" + FormattingPipeline.FormatWithPhase1And2(CurrentWiki, h, false, FormattingContext.PageHeader, currentPageFullName) + "</div>";

            model.PageHeader = MvcHtmlString.Create(FormattingPipeline.FormatWithPhase3(CurrentWiki, h, FormattingContext.PageHeader, currentPageFullName));

            h = Settings.GetProvider(CurrentWiki).GetMetaDataItem(MetaDataItem.PageFooter, currentNamespace);
            h = @"<div id=""PageInternalFooterDiv"">" + FormattingPipeline.FormatWithPhase1And2(CurrentWiki, h, false, FormattingContext.PageFooter, currentPageFullName) + "</div>";

            model.PageFooter = MvcHtmlString.Create(FormattingPipeline.FormatWithPhase3(CurrentWiki, h, FormattingContext.PageFooter, currentPageFullName));
        }

        /// <summary>
        /// Prints the sidebar.
        /// </summary>
        private void FillDefaultSidebar(WikiBaseModel model, string currentNamespace, string currentPageFullName)
        {
            string s = FormattingPipeline.FormatWithPhase1And2(CurrentWiki, Settings.GetProvider(CurrentWiki).GetMetaDataItem(MetaDataItem.Sidebar, currentNamespace),
                false, FormattingContext.Sidebar, currentPageFullName);

            model.Sidebar = MvcHtmlString.Create(FormattingPipeline.FormatWithPhase3(CurrentWiki, s, FormattingContext.Sidebar, currentPageFullName));
        }

        /// <summary>
        /// Prints the HTML head tag.
        /// </summary>
        private void FillDefaultHtmlHead(WikiBaseModel model, string currentNamespace)
        {
            StringBuilder sb = new StringBuilder(100);

            if (Settings.GetRssFeedsMode(CurrentWiki) != RssFeedsMode.Disabled)
            {
                sb.AppendFormat(@"<link rel=""alternate"" title=""{0}"" href=""{1}######______NAMESPACE______######RSS.aspx"" type=""application/rss+xml"" />",
                    Settings.GetWikiTitle(CurrentWiki), Settings.GetMainUrl(CurrentWiki));
                sb.Append("\n");
                sb.AppendFormat(@"<link rel=""alternate"" title=""{0}"" href=""{1}######______NAMESPACE______######RSS.aspx?Discuss=1"" type=""application/rss+xml"" />",
                    Settings.GetWikiTitle(CurrentWiki) + " - Discussions", Settings.GetMainUrl(CurrentWiki));
                sb.Append("\n");
            }

            sb.Append("######______INCLUDES______######");

            sb.AppendLine(Host.Instance.GetAllHtmlHeadContent(CurrentWiki));

            // Use a Control to allow 3rd party plugins to programmatically access the Page header
            string nspace = currentNamespace;
            if (nspace == null) nspace = "";
            else if (nspace.Length > 0) nspace += ".";

            var htmlHead =  MvcHtmlString.Create(
                    sb.ToString()
                        .Replace("######______INCLUDES______######", Tools.GetIncludes(CurrentWiki, currentNamespace))
                        .Replace("######______NAMESPACE______######", nspace));

            model.HtmlHeads.Add(htmlHead);
        }

        /// <summary>
        /// Prints the header.
        /// </summary>
        public void FillDefaultHeader(WikiBaseModel model, string currentNamespace, string currentPageFullName)
        {
            string h = FormattingPipeline.FormatWithPhase1And2(CurrentWiki, Settings.GetProvider(CurrentWiki).GetMetaDataItem(MetaDataItem.Header, currentNamespace),
                false, FormattingContext.Header, currentPageFullName);

            model.Header = MvcHtmlString.Create(FormattingPipeline.FormatWithPhase3(CurrentWiki, h, FormattingContext.Header, currentPageFullName));
        }

        /// <summary>
        /// Prints the footer.
        /// </summary>
        public void FillDefaultFooter(WikiBaseModel model, string currentNamespace, string currentPageFullName)
        {
            string f = FormattingPipeline.FormatWithPhase1And2(CurrentWiki, Settings.GetProvider(CurrentWiki).GetMetaDataItem(MetaDataItem.Footer, currentNamespace),
                false, FormattingContext.Footer, currentPageFullName);

            model.Footer = MvcHtmlString.Create(FormattingPipeline.FormatWithPhase3(CurrentWiki, f, FormattingContext.Footer, currentPageFullName));
        }

        #endregion Default


        //#region Clean

        //[NonAction]
        //protected void PrepareCleanModel(WikiBaseModel model)
        //{
        //    FillCleanHtmlHead(model);
        //    FillCleanHeader(model);
        //    FillCleanFooter(model);
        //}

        ///// <summary>
        ///// Prints the HTML head tag.
        ///// </summary>
        //private void FillCleanHtmlHead(WikiBaseModel model)
        //{
        //    model.HtmlHead =
        //        MvcHtmlString.Create(Tools.GetIncludes(CurrentWiki, Tools.DetectCurrentNamespace()) + "\r\n" +
        //                             Host.Instance.GetAllHtmlHeadContent(CurrentWiki));
        //}

        ///// <summary>
        ///// Prints the header.
        ///// </summary>
        //private void FillCleanHeader(WikiBaseModel model)
        //{
        //    string h = FormattingPipeline.FormatWithPhase1And2(CurrentWiki, "{wikititle}",
        //        false, FormattingContext.Header, null);

        //    model.Header =
        //        MvcHtmlString.Create("<h1>" +
        //                             FormattingPipeline.FormatWithPhase3(CurrentWiki, h, FormattingContext.Header, null) +
        //                             "</h1>");
        //}

        ///// <summary>
        ///// Prints the footer.
        ///// </summary>
        //private void FillCleanFooter(WikiBaseModel model)
        //{
        //    string f = FormattingPipeline.FormatWithPhase1And2(CurrentWiki,
        //        ScrewTurn.Wiki.Settings.GetProvider(CurrentWiki).GetMetaDataItem(MetaDataItem.Footer, CurrentNamespace),
        //        false, FormattingContext.Footer, null);

        //    model.Footer =
        //        MvcHtmlString.Create(FormattingPipeline.FormatWithPhase3(CurrentWiki, f, FormattingContext.Footer, null));
        //}

        //#endregion Clean
    }
}