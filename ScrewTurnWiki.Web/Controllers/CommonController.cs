using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
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

        #region PageNotFound

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
    }
}