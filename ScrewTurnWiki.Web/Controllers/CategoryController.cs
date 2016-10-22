using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Code.Attributes;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models.Wiki;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class CategoryController : PageController
    {
        public CategoryController(ApplicationSettings appSettings) : base(appSettings)
        {
        }

        [HttpGet]
        [DetectNamespaceFromPage(PageParamName = "page", Order = 1)]
        [CheckActionForNamespace(Action = CheckActionForNamespaceAttribute.ActionForNamespaces.ReadPages, Order = 2)]
        public ActionResult GetCategory(string page)
        {
            var model = new CategoryModel();
            base.PrepareDefaultModel(model, CurrentNamespace, CurrentPageFullName);

            model.Title = Messages.CategoryTitle + " - " + Settings.GetWikiTitle(CurrentWiki);

            //LoginTools.VerifyReadPermissionsForCurrentNamespace(CurrentWiki);

            model.CatList = new MvcHtmlString(PrintCat());

            return View("~/Views/Wiki/Category.cshtml", model);
        }

        private string PrintCat()
        {
            var namespaceInfo = DetectNamespaceInfo();

            StringBuilder sb = new StringBuilder();
            sb.Append("<ul>");
            sb.Append(@"<li><a href=""");
            UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, sb, "AllPages?Cat=-");
            sb.Append(@""">");
            sb.Append(Messages.UncategorizedPages);
            sb.Append("</a> (");
            sb.Append(Pages.GetUncategorizedPages(CurrentWiki, namespaceInfo).Length.ToString());
            sb.Append(")");
            sb.Append(@" - <small><a href=""");
            UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, sb, "RSS?Category=-");
            sb.Append(@""" title=""");
            sb.Append(Messages.RssForThisCategory);
            sb.Append(@""">RSS</a> - <a href=""");
            UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, sb, "RSS?Discuss=1&amp;Category=-");
            sb.Append(@""" title=""");
            sb.Append(Messages.RssForThisCategoryDiscussion);
            sb.Append(@""">");
            sb.Append(Messages.DiscussionsRss);
            sb.Append("</a>");
            sb.Append("</small>");
            sb.Append("</li></ul><br />");

            sb.Append("<ul>");

            List<CategoryInfo> categories = Pages.GetCategories(CurrentWiki, namespaceInfo);

            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].Pages.Length > 0)
                {
                    sb.Append(@"<li>");
                    sb.Append(@"<a href=""");
                    UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, sb, "AllPages?Cat=", Tools.UrlEncode(categories[i].FullName));
                    sb.Append(@""">");
                    sb.Append(NameTools.GetLocalName(categories[i].FullName));
                    sb.Append("</a> (");
                    sb.Append(categories[i].Pages.Length.ToString());
                    sb.Append(")");
                    sb.Append(@" - <small><a href=""");
                    UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, sb, "RSS?Category=", Tools.UrlEncode(categories[i].FullName));
                    sb.Append(@""" title=""");
                    sb.Append(Messages.RssForThisCategory);
                    sb.Append(@""">RSS</a> - <a href=""");
                    UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, sb, "RSS?Discuss=1&amp;Category=", Tools.UrlEncode(categories[i].FullName));
                    sb.Append(@""" title=""");
                    sb.Append(Messages.RssForThisCategoryDiscussion);
                    sb.Append(@""">");
                    sb.Append(Messages.DiscussionsRss);
                    sb.Append("</a>");
                    sb.Append("</small>");
                    sb.Append("</li>");
                }
                else
                {
                    sb.Append(@"<li><i>");
                    sb.Append(NameTools.GetLocalName(categories[i].FullName));
                    sb.Append("</i></li>");
                }
            }
            sb.Append("</ul>");
            return sb.ToString();
        }
    }
}