using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class LogoutModel : LoginBaseModel
    {
        [AllowHtml]
        public string LogoutText { get; set; }

        [AllowHtml]
        public string Description { get; set; }
    }
}