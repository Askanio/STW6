using ScrewTurn.Wiki.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models;

namespace ScrewTurn.Wiki.Web.Controllers
{
    /// <summary>
    /// A base controller for all ScrewTurnWiki controller classes.
    /// </summary>
    public class PageController : BaseController
    {
        /// <summary>
        /// The name of the current namespace using the <b>NS</b> parameter in the query string.
        /// </summary>
        public string CurrentNamespace { get; set; }

        /// <summary>
        /// The correct <see cref="T:PageInfo" /> object associated to the current page using the <b>Page</b> and <b>NS</b> parameters in the query string.
        /// </summary>
        public string CurrentPageFullName { get; set; }

        /// <summary>
        /// Page in the given wiki
        /// </summary>
        public PageContent CurrentPage { get; set; }

        /// <summary>
        /// Detects the correct <see cref="T:NamespaceInfo" /> object associated to the current namespace.
        /// </summary>
        /// <returns>The correct <see cref="T:NamespaceInfo" /> object, or <c>null</c>.</returns>
        public NamespaceInfo DetectNamespaceInfo()
        {
            NamespaceInfo nsinfo = CurrentNamespace != null ? Pages.FindNamespace(CurrentWiki, CurrentNamespace) : null;
            return nsinfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public PageController(ApplicationSettings settings) : base(settings)
        {
        }

        #region SA

        [NonAction]
        protected void PrepareSAModel(WikiSABaseModel model, string currentNamespace)
        {
            string nspace = currentNamespace;
            if (string.IsNullOrEmpty(nspace)) nspace = "";
            else nspace += ".";
            model.LnkMainPageUrl = "/" + nspace + "Default";

                string referrer = Request.UrlReferrer != null ? Request.UrlReferrer.FixHost().ToString() : "";
                if (!string.IsNullOrEmpty(referrer))
                    model.LnkPreviousPageUrl = new MvcHtmlString(referrer);

            FillSAHtmlHead(model);
            FillDefaultHeader(model, currentNamespace, null);
            FillDefaultFooter(model, currentNamespace, null);
        }

        /// <summary>
        /// Prints the HTML head tag.
        /// </summary>
        private void FillSAHtmlHead(WikiBaseModel model)
        {
            var htmlHead =
                MvcHtmlString.Create(Tools.GetIncludes(CurrentWiki, Tools.DetectCurrentNamespace()) + "\r\n" +
                                     Host.Instance.GetAllHtmlHeadContent(CurrentWiki));
            model.HtmlHeads.Add(htmlHead);
        }

        #endregion

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
        private void FillDefaultPageHeaderAndFooter(WikiBaseModel model, string currentNamespace, string currentPageFullName)
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

            var htmlHead = MvcHtmlString.Create(
                    sb.ToString()
                        .Replace("######______INCLUDES______######", Tools.GetIncludes(CurrentWiki, currentNamespace))
                        .Replace("######______NAMESPACE______######", nspace));

            model.HtmlHeads.Add(htmlHead);
        }

        /// <summary>
        /// Prints the header.
        /// </summary>
        private void FillDefaultHeader(WikiBaseModel model, string currentNamespace, string currentPageFullName)
        {
            string h = FormattingPipeline.FormatWithPhase1And2(CurrentWiki, Settings.GetProvider(CurrentWiki).GetMetaDataItem(MetaDataItem.Header, currentNamespace),
                false, FormattingContext.Header, currentPageFullName);

            model.Header = MvcHtmlString.Create(FormattingPipeline.FormatWithPhase3(CurrentWiki, h, FormattingContext.Header, currentPageFullName));
        }

        /// <summary>
        /// Prints the footer.
        /// </summary>
        private void FillDefaultFooter(WikiBaseModel model, string currentNamespace, string currentPageFullName)
        {
            string f = FormattingPipeline.FormatWithPhase1And2(CurrentWiki, Settings.GetProvider(CurrentWiki).GetMetaDataItem(MetaDataItem.Footer, currentNamespace),
                false, FormattingContext.Footer, currentPageFullName);

            model.Footer = MvcHtmlString.Create(FormattingPipeline.FormatWithPhase3(CurrentWiki, f, FormattingContext.Footer, currentPageFullName));
        }

        #endregion Default

        //#region Detect and set CurrentNamespace and CurrentPage

        ///// <summary>
        ///// Detect and set CurrentNamespace and CurrentPage
        ///// </summary>
        ///// <param name="pageName"></param>
        ///// <param name="notFoundPageName">Name of Page for redirect if return false</param>
        ///// <returns>true - ok, false - CurrentNamespace or CurrentPage not exists</returns>
        //[NonAction]
        //protected bool InitializeCurrentPage(string pageName, out string notFoundPageName)
        //{
        //    // Try to detect current namespace
        //    CurrentNamespace = PageHelper.DetectNamespace(pageName);

        //    if (!ExistsCurrentNamespace())
        //    {
        //        notFoundPageName = pageName;
        //        return false;
        //    }

        //    SetCurrentPage(pageName);

        //    // Verifies the need for a redirect and performs it.
        //    if (CurrentPage == null)
        //    {
        //        notFoundPageName = Tools.UrlEncode(CurrentPageFullName);
        //        return false;
        //    }

        //    notFoundPageName = null;
        //    return true;
        //}

        //private bool ExistsCurrentNamespace()
        //{
        //    if (!string.IsNullOrEmpty(CurrentNamespace))
        //    {
        //        // Verify that namespace exists
        //        return Pages.FindNamespace(Tools.DetectCurrentWiki(), CurrentNamespace) != null;
        //    }
        //    return true; // default "root"
        //}


        //private void SetCurrentPage(string pageName)
        //{
        //    CurrentPageFullName = GetCurrentPageFullName(pageName);

        //    CurrentPage = Pages.FindPage(CurrentWiki, CurrentPageFullName);
        //}

        //private string GetCurrentPageFullName(string pageName)
        //{
        //    // Trim Namespace. from pageName
        //    if (!string.IsNullOrEmpty(CurrentNamespace))
        //        pageName = pageName.Substring(CurrentNamespace.Length + 1);

        //    if (string.IsNullOrEmpty(pageName) || pageName == "Default")
        //        pageName = Settings.GetDefaultPage(CurrentWiki);

        //    return string.IsNullOrEmpty(CurrentNamespace)
        //        ? pageName
        //        : $"{CurrentNamespace}.{pageName}";
        //}

        //#endregion

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