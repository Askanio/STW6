using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models
{
    [Serializable]
    public class WikiBaseModel
    {
        public WikiBaseModel()
        {
            HtmlHeads = new List<MvcHtmlString>();
            Direction = GlobalSettings.Direction;
        }

        public string Direction { get; set; }

        public string Title { get; set; }

        [AllowHtml]
        public List<MvcHtmlString> HtmlHeads { get; set; }

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