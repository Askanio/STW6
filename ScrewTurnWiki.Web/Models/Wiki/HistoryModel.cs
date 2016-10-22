using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Code;

namespace ScrewTurn.Wiki.Web.Models.Wiki
{
    public class HistoryModel : WikiPageModel
    {
        public HistoryModel()
        {
            Rev1List = new List<SelectListItem>();
            Rev2List = new List<SelectListItem>();
            RevisionRows = new List<RevisionRow>();
            BtnCompareEnabled = true;
        }

        [AllowHtml]
        public MvcHtmlString LblTitle { get; set; }

        [AllowHtml]
        public MvcHtmlString LblHistory { get; set; }

        public List<SelectListItem> Rev1List { get; set; }

        public List<SelectListItem> Rev2List { get; set; }

        public List<RevisionRow> RevisionRows { get; set; }

        public bool BtnCompareEnabled { get; set; }
    }
}