using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models
{
    public class WikiBaseModel : BaseModel
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