using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.Wiki
{
    public class DiffModel : WikiPageModel
    {
        [AllowHtml]
        public MvcHtmlString LblBack { get; set; }

        [AllowHtml]
        public MvcHtmlString LblTitle { get; set; }

        [AllowHtml]
        public MvcHtmlString LblDiff { get; set; }
    }
}