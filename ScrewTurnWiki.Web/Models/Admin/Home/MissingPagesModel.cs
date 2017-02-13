using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScrewTurn.Wiki.Web.Code;

namespace ScrewTurn.Wiki.Web.Models.Admin.Home
{
    public class MissingPagesModel : AdminBaseModel
    {
        public MissingPagesModel()
        {
            WantedPages = new List<WantedPageRow>();
        }

        public List<WantedPageRow> WantedPages { get; set; }
    }
}