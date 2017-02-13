using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using System.Xml;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models;
using ScrewTurn.Wiki.Web.Models.Common;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class CommonController : PageController
    {
        public CommonController(ApplicationSettings appSettings) : base(appSettings)
        {
        }

        [HttpGet]
        public ActionResult Error(string ns)
        {
            var model = new WikiSABaseModel();

            model.Title = Messages.ErrorTitle + " - " + Settings.GetWikiTitle(CurrentWiki);

            var currentNamespace = string.IsNullOrEmpty(ns) ? "" : ns;

            PrepareSAModel(model, currentNamespace);


            // Workaround for ASP.NET vulnerability
            // http://weblogs.asp.net/scottgu/archive/2010/09/18/important-asp-net-security-vulnerability.aspx
            byte[] delay = new byte[1];
            RandomNumberGenerator prng = new RNGCryptoServiceProvider();

            prng.GetBytes(delay);
            Thread.Sleep((int)delay[0]);

            IDisposable disposable = prng as IDisposable;
            if (disposable != null) { disposable.Dispose(); }


            return View("Error", model);
        }

        [HttpGet]
        public ActionResult AccessDenied()
        {
            var model = new AccessDeniedModel();

            model.Title = Messages.AccessDeniedTitle + " - " + Settings.GetWikiTitle(CurrentWiki);

            PrepareSAModel(model, CurrentNamespace);

            string n = Settings.GetProvider(CurrentWiki).GetMetaDataItem(MetaDataItem.AccessDeniedNotice, null);
            if (!string.IsNullOrEmpty(n))
            {
                n = FormattingPipeline.FormatWithPhase1And2(CurrentWiki, n, false, FormattingContext.Other, null);
            }
            if (!string.IsNullOrEmpty(n))
                model.Description =
                    new MvcHtmlString(FormattingPipeline.FormatWithPhase3(CurrentWiki, n, FormattingContext.Other, null));

            return View("AccessDenied", model);
        }

        [HttpGet]
        public ActionResult RandPage()
        {
            List<PageContent> pages = Pages.GetPages(CurrentWiki, Tools.DetectCurrentNamespaceInfo());
            Random r = new Random();
            //UrlTools.Redirect(pages[r.Next(0, pages.Count)].FullName + GlobalSettings.PageExtension);

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("controller", "Wiki");
            routeValueDictionary.Add("action", "Page");
            routeValueDictionary.Add("page", pages[r.Next(0, pages.Count)].FullName);
            return new RedirectToRouteResult(routeValueDictionary);
        }

        #region PageNotFound

        [HttpGet]
        public ActionResult PageNotFound(string page)
        {
            var model = new PageNotFoundModel();
            model.Title = Messages.PageNotFoundTitle + " - " + Settings.GetWikiTitle(CurrentWiki);

            var currentNamespace = PageHelper.DetectNamespace(page);

            base.PrepareDefaultModel(model, currentNamespace, page);

            if (page != null)
            {
                model.Description = Localization.Common.PageNotFound.LblDescription_Text.Replace("##PAGENAME##", page);
            }
            else
            {
                return RedirectToAction("Page", "Wiki", new { page = $"{CurrentWiki}.Default"});
                //UrlTools.Redirect(UrlTools.BuildUrl(CurrentWiki, "Default"));
            }

            model.SearchResults = new MvcHtmlString(PrintSearchResults(currentNamespace, page));


            return View("PageNotFound", model);
        }



        /// <summary>
        /// Prints the results of the automatic search.
        /// </summary>
        private string PrintSearchResults(string currentNamespace, string page)
        {
            StringBuilder sb = new StringBuilder(1000);

            PageContent[] results = SearchClass.SearchSimilarPages(CurrentWiki, page, currentNamespace);
            if (results.Length > 0)
            {
                sb.Append("<p>");
                sb.Append(Messages.WereYouLookingFor);
                sb.Append("</p>");
                sb.Append("<ul>");
                for (int i = 0; i < results.Length; i++)
                {
                    sb.Append(@"<li><a href=""");
                    UrlTools.BuildUrl(CurrentWiki, sb, Tools.UrlEncode(results[i].FullName), GlobalSettings.PageExtension);
                    sb.Append(@""">");
                    sb.Append(FormattingPipeline.PrepareTitle(CurrentWiki, results[i].Title, false, FormattingContext.PageContent, results[i].FullName));
                    sb.Append("</a></li>");
                }
                sb.Append("</ul>");
            }
            else
            {
                sb.Append("<p>");
                sb.Append(Messages.NoSimilarPages);
                sb.Append("</p>");
            }
            sb.Append(@"<br /><p>");
            sb.Append(Messages.YouCanAlso);
            sb.Append(@" <a href=""");
            UrlTools.BuildUrl(CurrentWiki, sb, "Search?Query=", Tools.UrlEncode(page)); // TODO:
            sb.Append(@""">");
            sb.Append(Messages.PerformASearch);
            sb.Append("</a> ");
            sb.Append(Messages.Or);
            sb.Append(@" <a href=""");
            UrlTools.BuildUrl(CurrentWiki, sb, "Edit.aspx?Page=", Tools.UrlEncode(page)); // TODO:
            sb.Append(@"""><b>");
            sb.Append(Messages.CreateThePage);
            sb.Append("</b></a> (");
            sb.Append(Messages.CouldRequireLogin);
            sb.Append(").</p>");

            return sb.ToString();
        }

        #endregion

        #region Sitemap

        [HttpGet]
        public ActionResult Sitemap()
        {
            Response.ClearContent();
            Response.ContentType = "text/xml;charset=UTF-8";
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;

            string mainUrl = Settings.GetMainUrl(CurrentWiki);
            string rootDefault = Settings.GetDefaultPage(CurrentWiki).ToLowerInvariant();

            using (XmlWriter writer = XmlWriter.Create(Response.OutputStream))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi", "schemaLocation", null, "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/09/sitemap.xsd");

                string user = SessionFacade.GetCurrentUsername();
                string[] groups = SessionFacade.GetCurrentGroupNames(CurrentWiki);


                AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));

                foreach (PageContent page in Pages.GetPages(CurrentWiki, null))
                {
                    if (authChecker.CheckActionForPage(page.FullName, Actions.ForPages.ReadPage, user, groups))
                        WritePage(mainUrl, page.FullName, page.FullName.ToLowerInvariant() == rootDefault, writer);
                }
                foreach (NamespaceInfo nspace in Pages.GetNamespaces(CurrentWiki))
                {
                    string nspaceDefault = nspace.DefaultPageFullName.ToLowerInvariant();

                    foreach (PageContent page in Pages.GetPages(CurrentWiki, nspace))
                    {
                        if (authChecker.CheckActionForPage(page.FullName, Actions.ForPages.ReadPage, user, groups))
                            WritePage(mainUrl, page.FullName, page.FullName.ToLowerInvariant() == nspaceDefault, writer);
                    }
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            return View("~/Views/Common/Sitemap.cshtml");
        }

        /// <summary>
        /// Writes a page to the output XML writer.
        /// </summary>
        /// <param name="mainUrl">The main wiki URL.</param>
        /// <param name="pageFullName">The page full name.</param>
        /// <param name="isDefault">A value indicating whether the page is the default of its namespace.</param>
        /// <param name="writer">The writer.</param>
        private void WritePage(string mainUrl, string pageFullName, bool isDefault, XmlWriter writer)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", mainUrl + Tools.UrlEncode(pageFullName) + GlobalSettings.PageExtension);
            writer.WriteElementString("priority", isDefault ? "0.75" : "0.5");
            writer.WriteElementString("changefreq", "daily");
            writer.WriteEndElement();
        }

        #endregion

    }
}