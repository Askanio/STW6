using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.Common
{
    public class SearchModel : WikiPageModel
    {
        public SearchModel()
        {
            Categories = new List<SelectListItem>();
        }

        public string Namespace { get; set; }

        public bool IsAllNamespaces { get; set; }

        public bool IsFilesAndAttachments { get; set; }

        public bool AtLeastOneWord { get; set; }

        public bool AllWords { get; set; }

        public bool ExactPhrase { get; set; }

        public bool IsUncategorizedPages { get; set; }

        public string Query { get; set; }

        public IList<SelectListItem> Categories { get; set; }

    }
}