using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Code.Attributes;
using ScrewTurn.Wiki.Web.Localization.Common;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models.Common;
using ScrewTurn.Wiki.Web.Models.Wiki;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class AllPagesController : PageController
    {

        public AllPagesController(ApplicationSettings appSettings) : base(appSettings)
        {
        }

        /// <summary>
        /// Get AllPages
        /// </summary>
        /// <param name="page">"AllPages" or "Namespace.AllPages"</param>
        /// <param name="cat"></param>
        /// <param name="sortBy"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        [HttpGet]
        [DetectNamespaceFromPage(PageParamName = "page", Order = 1)]
        [CheckActionForNamespace(Action = CheckActionForNamespaceAttribute.ActionForNamespaces.ReadPages, Order = 2)]
        public ActionResult GetAllPages(string page, string cat, string sortBy, bool reverse = false)
        {
            var model = new AllPagesModel();
            base.PrepareDefaultModel(model, CurrentNamespace, CurrentPageFullName);

            model.SortBy = ParseSortBy(sortBy);
            model.Reverse = reverse;
            model.Category = cat;
            model.Namespace = CurrentNamespace;

            //LoginTools.VerifyReadPermissionsForCurrentNamespace(CurrentWiki);

            if (cat != null)
            {
                if (cat.Equals("-"))
                    model.LblPages = new MvcHtmlString(Messages.UncategorizedPages);
                else
                    model.LblPages = new MvcHtmlString(Messages.PagesOfCategory + " <i>" + cat + "</i>");
            }
            else
            {
                model.LblPages = new MvcHtmlString(AllPages.LblPages_Text);
            }

            model.CategoriesUrl = UrlTools.BuildWikiUrl(CurrentWiki,CurrentNamespace , "Category");
            model.SearchUrl = UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, "Search");

            IList<PageContent> currentPages = GetAllPages(cat);
            model.LinkNames = GetPageLinks(currentPages.Count);

            //int pageSize = Settings.GetListSize(CurrentWiki);
            //int rangeEnd = pageSize - 1;
            //int rangeBegin = 0;
            model.Title = Messages.AllPagesTitle + " - " + Settings.GetWikiTitle(CurrentWiki);
            //  (" + (rangeBegin + 1).ToString() + "-" + (rangeEnd + 1).ToString() + ")

            // Important note
            // This page cannot use a repeater because the page list has particular elements used for grouping pages

            //if (model.LinkNames.Count == 0)
            model.Content = new MvcHtmlString(PrintPages(CurrentNamespace, 0, cat, sortBy, reverse));

            return View("~/Views/Wiki/AllPages.cshtml", model);
        }

        /// <summary>
        /// Prints the pages.
        /// </summary>
        [HttpGet]
        [CheckAndSetNamespace(NamespaceParamName = "nspace", Order = 1)]
        [CheckActionForNamespace(Action = CheckActionForNamespaceAttribute.ActionForNamespaces.ReadPages, Order = 2)
        ]
        public ActionResult GetPages(string nspace, int? page, string cat, string sortBy, bool reverse = false)
        {
            var model = new RegionContentModel();
            model.Content = new MvcHtmlString(PrintPages(nspace, page, cat, sortBy, reverse));
            return PartialView("RegionContent", model);
        }

        /// <summary>
        /// Prints the pages.
        /// </summary>
        public string PrintPages(string nspace, int? page, string category, string sort, bool reverse = false)
        {
            int selectedPage = page ?? 0;
            int pageSize = Settings.GetListSize(CurrentWiki);
            int rangeBegin = (selectedPage * pageSize);
            int rangeEnd = rangeBegin + pageSize - 1;
            
            StringBuilder sb = new StringBuilder(65536);

            IList<PageContent> currentPages = GetAllPages(category);

            if (rangeEnd > currentPages.Count)
                rangeEnd = currentPages.Count - 1;

            // Prepare ExtendedPageInfo array
            ExtendedPageInfo[] tempPageList = new ExtendedPageInfo[rangeEnd - rangeBegin + 1];
            for (int i = 0; i < tempPageList.Length; i++)
            {
                tempPageList[i] = new ExtendedPageInfo(currentPages[rangeBegin + i], GetCreator(currentPages[rangeBegin + i]), currentPages[rangeBegin + i].User);
            }

            // Prepare for sorting
            SortingMethod sortBy = ParseSortBy(sort);

            SortedDictionary<SortingGroup, List<ExtendedPageInfo>> sortedPages = PageSortingTools.Sort(tempPageList, sortBy, reverse);

            sb.Append(@"<table id=""PageListTable"" class=""generic"" cellpadding=""0"" cellspacing=""0"">");
            sb.Append("<thead>");
            sb.Append(@"<tr class=""tableheader"">");

            // Page title
            sb.Append(@"<th><a rel=""nofollow"" href=""");
            UrlTools.BuildWikiUrl(CurrentWiki, nspace, sb, "AllPages?SortBy=Title",
                (!reverse && sortBy == SortingMethod.Title ? "&amp;Reverse=1" : ""),
                (category != null ? "&amp;Cat=" + Tools.UrlEncode(category) : ""),
                "&amp;Page=", selectedPage.ToString());

            sb.Append(@""" title=""");
            sb.Append(Messages.SortByTitle);
            sb.Append(@""">");
            sb.Append(Messages.PageTitle);
            sb.Append((reverse && sortBy.Equals("title") ? " &uarr;" : ""));
            sb.Append((!reverse && sortBy.Equals("title") ? " &darr;" : ""));
            sb.Append("</a></th>");

            // Message count
            sb.Append(@"<th><img src=""/Content/Images/Comment.png"" alt=""Comments"" /></th>");

            // Creation date/time
            sb.Append(@"<th><a rel=""nofollow"" href=""");
            UrlTools.BuildWikiUrl(CurrentWiki, nspace, sb, "AllPages?SortBy=Creation",
                (!reverse && sortBy == SortingMethod.Creation ? "&amp;Reverse=1" : ""),
                (category != null ? "&amp;Cat=" + Tools.UrlEncode(category) : ""),
                "&amp;Page=", selectedPage.ToString());

            sb.Append(@""" title=""");
            sb.Append(Messages.SortByDate);
            sb.Append(@""">");
            sb.Append(Messages.CreatedOn);
            sb.Append((reverse && sortBy.Equals("creation") ? " &uarr;" : ""));
            sb.Append((!reverse && sortBy.Equals("creation") ? " &darr;" : ""));
            sb.Append("</a></th>");

            // Mod. date/time
            sb.Append(@"<th><a rel=""nofollow"" href=""");
            UrlTools.BuildWikiUrl(CurrentWiki, nspace, sb, "AllPages?SortBy=DateTime",
                (!reverse && sortBy == SortingMethod.DateTime ? "&amp;Reverse=1" : ""),
                (category != null ? "&amp;Cat=" + Tools.UrlEncode(category) : ""),
                "&amp;Page=", selectedPage.ToString());

            sb.Append(@""" title=""");
            sb.Append(Messages.SortByDate);
            sb.Append(@""">");
            sb.Append(Messages.ModifiedOn);
            sb.Append((reverse && sortBy.Equals("date") ? " &uarr;" : ""));
            sb.Append((!reverse && sortBy.Equals("date") ? " &darr;" : ""));
            sb.Append("</a></th>");

            // Creator
            sb.Append(@"<th><a rel=""nofollow"" href=""");
            UrlTools.BuildWikiUrl(CurrentWiki, nspace, sb, "AllPages?SortBy=Creator",
                (!reverse && sortBy == SortingMethod.Creator ? "&amp;Reverse=1" : ""),
                (category != null ? "&amp;Cat=" + Tools.UrlEncode(category) : ""),
                "&amp;Page=", selectedPage.ToString());

            sb.Append(@""" title=""");
            sb.Append(Messages.SortByUser);
            sb.Append(@""">");
            sb.Append(Messages.CreatedBy);
            sb.Append((reverse && sortBy.Equals("creator") ? " &uarr;" : ""));
            sb.Append((!reverse && sortBy.Equals("creator") ? " &darr;" : ""));
            sb.Append("</a></th>");

            // Last author
            sb.Append(@"<th><a rel=""nofollow"" href=""");
            UrlTools.BuildWikiUrl(CurrentWiki, nspace, sb, "AllPages?SortBy=User",
                (!reverse && sortBy == SortingMethod.User ? "&amp;Reverse=1" : ""),
                (category != null ? "&amp;Cat=" + Tools.UrlEncode(category) : ""),
                "&amp;Page=", selectedPage.ToString());

            sb.Append(@""" title=""");
            sb.Append(Messages.SortByUser);
            sb.Append(@""">");
            sb.Append(Messages.ModifiedBy);
            sb.Append((reverse && sortBy.Equals("user") ? " &uarr;" : ""));
            sb.Append((!reverse && sortBy.Equals("user") ? " &darr;" : ""));
            sb.Append("</a></th>");

            // Categories
            sb.Append("<th>");
            sb.Append(Messages.Categories);
            sb.Append("</th>");

            sb.Append("</tr>");
            sb.Append("</thead><tbody>");

            foreach (SortingGroup key in sortedPages.Keys)
            {
                List<ExtendedPageInfo> pageList = sortedPages[key];
                for (int i = 0; i < pageList.Count; i++)
                {
                    if (i == 0)
                    {
                        // Add group header
                        sb.Append(@"<tr class=""tablerow"">");
                        if (sortBy == SortingMethod.Title)
                        {
                            sb.AppendFormat("<td colspan=\"7\"><b>{0}</b></td>", key.Label);
                        }
                        else if (sortBy == SortingMethod.Creation)
                        {
                            sb.AppendFormat("<td colspan=\"2\"></td><td colspan=\"5\"><b>{0}</b></td>", key.Label);
                        }
                        else if (sortBy == SortingMethod.DateTime)
                        {
                            sb.AppendFormat("<td colspan=\"3\"></td><td colspan=\"4\"><b>{0}</b></td>", key.Label);
                        }
                        else if (sortBy == SortingMethod.Creator)
                        {
                            sb.AppendFormat("<td colspan=\"4\"></td><td colspan=\"3\"><b>{0}</b></td>", key.Label);
                        }
                        else if (sortBy == SortingMethod.User)
                        {
                            sb.AppendFormat("<td colspan=\"5\"></td><td colspan=\"2\"><b>{0}</b></td>", key.Label);
                        }
                        sb.Append("</tr>");
                    }

                    sb.Append(@"<tr class=""tablerow");
                    if ((i + 1) % 2 == 0) sb.Append("alternate");
                    sb.Append(@""">");

                    // Page title
                    sb.Append(@"<td>");
                    sb.Append(@"<a href=""");
                    UrlTools.BuildWikiUrl(CurrentWiki, nspace, sb, Tools.UrlEncode(pageList[i].PageContent.FullName), GlobalSettings.PageExtension);
                    sb.Append(@""">");
                    sb.Append(pageList[i].Title);
                    sb.Append("</a>");
                    sb.Append("</td>");

                    // Message count
                    sb.Append(@"<td>");
                    int msg = pageList[i].MessageCount;
                    if (msg > 0)
                    {
                        sb.Append(@"<a href=""");
                        UrlTools.BuildWikiUrl(CurrentWiki, nspace, sb, Tools.UrlEncode(pageList[i].PageContent.FullName), GlobalSettings.PageExtension, "?Discuss=1");
                        sb.Append(@""" title=""");
                        sb.Append(Messages.Discuss);
                        sb.Append(@""">");
                        sb.Append(msg.ToString());
                        sb.Append("</a>");
                    }
                    else sb.Append("&nbsp;");
                    sb.Append("</td>");

                    // Creation date/time
                    sb.Append(@"<td>");
                    sb.Append(Preferences.AlignWithTimezone(CurrentWiki, pageList[i].CreationDateTime).ToString(Settings.GetDateTimeFormat(CurrentWiki)) + "&nbsp;");
                    sb.Append("</td>");

                    // Mod. date/time
                    sb.Append(@"<td>");
                    sb.Append(Preferences.AlignWithTimezone(CurrentWiki, pageList[i].PageContent.LastModified).ToString(Settings.GetDateTimeFormat(CurrentWiki)) + "&nbsp;");
                    sb.Append("</td>");

                    // Creator
                    sb.Append(@"<td>");
                    sb.Append(Users.UserLink(CurrentWiki, pageList[i].Creator));
                    sb.Append("</td>");

                    // Last author
                    sb.Append(@"<td>");
                    sb.Append(Users.UserLink(CurrentWiki, pageList[i].LastAuthor));
                    sb.Append("</td>");

                    // Categories
                    CategoryInfo[] cats = Pages.GetCategoriesForPage(pageList[i].PageContent);
                    sb.Append(@"<td>");
                    if (cats.Length == 0)
                    {
                        sb.Append(@"<a href=""");
                        UrlTools.BuildWikiUrl(CurrentWiki, nspace, sb, "AllPages?Cat=-");
                        sb.Append(@""">");
                        sb.Append(Messages.NC);
                        sb.Append("</a>");
                    }
                    else
                    {
                        for (int k = 0; k < cats.Length; k++)
                        {
                            sb.Append(@"<a href=""");
                            UrlTools.BuildWikiUrl(CurrentWiki, nspace, sb, "AllPages?Cat=", Tools.UrlEncode(cats[k].FullName));
                            sb.Append(@""">");
                            sb.Append(NameTools.GetLocalName(cats[k].FullName));
                            sb.Append("</a>");
                            if (k != cats.Length - 1) sb.Append(", ");
                        }
                    }
                    sb.Append("</td>");

                    sb.Append("</tr>");
                }
            }
            sb.Append("</tbody>");
            sb.Append("</table>");

            return sb.ToString();
        }

        /// <summary>
        /// Renders the pages.
        /// </summary>
        private List<string> GetPageLinks(int pageCount)
        {
            int pageSize = Settings.GetListSize(CurrentWiki);
            int pageLinks = 1;
            if (pageSize != 0)
                pageLinks = (int)Math.Ceiling((decimal)pageCount / (decimal)pageSize);

            List<string> result = new List<string>(pageCount);
            for (int i = 0; i < pageLinks - 1; i++)
            {
                result.Add(FormatPageSelectorText(i * pageSize, pageSize));
            }

            var currentCountPage = (pageLinks - 1)*pageSize;
            if (pageCount > currentCountPage)
            {
                result.Add(FormatPageSelectorText(currentCountPage, pageCount - currentCountPage));
            }

            // Don't display anything if there is only one page
            if (result.Count > 1)
                return result;
            else
                return new List<string>();
        }

        public string FormatPageSelectorText(int begin, int size)
        {
            return (begin + 1).ToString() + "-" + (begin + size).ToString();
        }

        private SortingMethod ParseSortBy(string value)
        {
            SortingMethod sortBy = SortingMethod.Title;
            if (value != null)
            {
                try
                {
                    sortBy = (SortingMethod)Enum.Parse(typeof(SortingMethod), value, true);
                }
                catch
                {
                    // Backwards compatibility
                    if (value.ToLowerInvariant() == "date")
                        sortBy = SortingMethod.DateTime;
                }
            }
            return sortBy;
        }

        /// <summary>
        /// Gets all the pages in the namespace.
        /// </summary>
        /// <returns>The pages.</returns>
        private IList<PageContent> GetAllPages(string category)
        {
            IList<PageContent> pages = null;

            // Categories Management
            if (category != null)
            {
                if (category.Equals("-"))
                {
                    pages = Pages.GetUncategorizedPages(CurrentWiki, DetectNamespaceInfo());
                }
                else
                {
                    CategoryInfo cat = Pages.FindCategory(CurrentWiki, category);
                    if (cat != null)
                    {
                        pages = new PageContent[cat.Pages.Length];
                        for (int i = 0; i < cat.Pages.Length; i++)
                        {
                            pages[i] = Pages.FindPage(CurrentWiki, cat.Pages[i]);
                        }
                        Array.Sort(pages as PageContent[], new PageNameComparer());
                    }
                    else
                    {
                        pages = new PageContent[0];
                    }
                }
            }
            else
            {
                pages = Pages.GetPages(CurrentWiki, DetectNamespaceInfo());
            }

            return pages;
        }

        /// <summary>
        /// Gets the creator of a page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>The creator.</returns>
        private string GetCreator(PageContent page)
        {
            List<int> baks = Pages.GetBackups(page);

            PageContent temp = null;
            if (baks.Count > 0)
                temp = Pages.GetBackupContent(page, baks[0]);
            else
                temp = page;

            return temp.User;
        }

    }
}