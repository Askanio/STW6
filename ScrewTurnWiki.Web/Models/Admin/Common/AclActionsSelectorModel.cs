using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.Admin.Common
{
    public class AclActionsSelectorModel
    {

        public AclActionsSelectorModel()
        {
            ActionsGrant = new List<SelectListItem>();
            ActionsDeny = new List<SelectListItem>();
        }

        public IList<SelectListItem> ActionsGrant { get; set; }

        public IList<SelectListItem> ActionsDeny { get; set; }
    }
}