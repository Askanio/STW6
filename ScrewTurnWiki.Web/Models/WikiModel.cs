using System;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models
{
    public class WikiModel : WikiBaseModel
    {
        [AllowHtml]
        public MvcHtmlString Header { get; set; }

        [AllowHtml]
        public MvcHtmlString Footer { get; set; }

        [AllowHtml]
        public MvcHtmlString Sidebar { get; set; }

        [AllowHtml]
        public MvcHtmlString PageFooter { get; set; }

        [AllowHtml]
        public MvcHtmlString PageHeader { get; set; }
    }
}