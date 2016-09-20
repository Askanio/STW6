using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Code;

namespace ScrewTurn.Wiki.Web.Models.Wiki
{
    public class AllPagesModel : WikiPageModel
    {
        public AllPagesModel()
        {
            LinkNames = new List<string>();
            Reverse = false;
            SortBy = SortingMethod.Title;
        }

        [AllowHtml]
        public MvcHtmlString LblPages { get; set; }

        public string CategoriesUrl { get; set; }

        public string SearchUrl { get; set; }

        public List<string> LinkNames { get; set; }

        public string Namespace { get; set; }

        public string Category { get; set; }

        public SortingMethod SortBy { get; set; }

        public bool Reverse { get; set; }

        public MvcHtmlString Content { get; set; }

    }
}