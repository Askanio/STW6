using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Models.Home;

namespace ScrewTurn.Wiki.Web.Models
{
    public class WikiPageModel : WikiBaseModel
    {
        public WikiPageModel()
        {
            Contents = new List<MvcHtmlString>();
            PageInfoVisible = false;
            ModificationVisible = false;
            AuthorVisible = false;
            PageCategoriesVisible = false;
            NavigationPathsVisible = false;
            PageDiscussionVisible = false;
            AttachmentViewerVisible = false;
        }

        public bool PageInfoVisible { get; set; }

        public bool ModificationVisible { get; set; }

        public bool AuthorVisible { get; set; }

        public bool PageCategoriesVisible { get; set; }

        public bool NavigationPathsVisible { get; set; }

        public bool PageDiscussionVisible { get; set; }

        public bool AttachmentViewerVisible { get; set; }

        public string PageFullName { get; set; }

        [AllowHtml]
        public MvcHtmlString EmailNotification { get; set; }

        [AllowHtml]
        public MvcHtmlString PageTitle { get; set; }

        [AllowHtml]
        public MvcHtmlString DiscussLink { get; set; }

        [AllowHtml]
        public MvcHtmlString EditLink { get; set; }

        [AllowHtml]
        public MvcHtmlString ViewCodeLink { get; set; }

        [AllowHtml]
        public MvcHtmlString HistoryLink { get; set; }

        [AllowHtml]
        public MvcHtmlString AttachmentsLink { get; set; }

        [AllowHtml]
        public MvcHtmlString AdminToolsLink { get; set; }

        [AllowHtml]
        public MvcHtmlString RollbackPage { get; set; }

        [AllowHtml]
        public MvcHtmlString AdministratePage { get; set; }

        [AllowHtml]
        public MvcHtmlString SetPagePermissions { get; set; }

        [AllowHtml]
        public MvcHtmlString PostMessageLink { get; set; }

        [AllowHtml]
        public MvcHtmlString BackLink { get; set; }

        [AllowHtml]
        public MvcHtmlString DiscussedPage { get; set; }

        [AllowHtml]
        public MvcHtmlString ModifiedDateTime { get; set; }

        [AllowHtml]
        public MvcHtmlString Author { get; set; }

        [AllowHtml]
        public MvcHtmlString PageCategories { get; set; }

        [AllowHtml]
        public MvcHtmlString PrintLink { get; set; }

        [AllowHtml]
        public MvcHtmlString RssLink { get; set; }

        [AllowHtml]
        public MvcHtmlString RedirectionSource { get; set; }

        [AllowHtml]
        public MvcHtmlString NavigationPaths { get; set; }

        [AllowHtml]
        public MvcHtmlString PreviousPage { get; set; }

        [AllowHtml]
        public MvcHtmlString NextPage { get; set; }

        [AllowHtml]
        public MvcHtmlString BreadcrumbsTrail { get; set; }

        [AllowHtml]
        public MvcHtmlString DoubleClickHandler { get; set; }

        [AllowHtml]
        public List<MvcHtmlString> Contents { get; set; }
    }
}