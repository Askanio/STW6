using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScrewTurn.Wiki.Web.Code;

namespace ScrewTurn.Wiki.Web.Models.Admin.Home
{
    public class OrphanPagesModel : AdminBaseModel
    {
        public OrphanPagesModel()
        {
            IndexProviders = new List<IndexRow>();
        }

        public int OrphanPagesCount { get; set; }

        public List<IndexRow> IndexProviders { get; set; }
    }
}