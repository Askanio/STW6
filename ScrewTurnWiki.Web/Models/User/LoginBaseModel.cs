using System;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class LoginBaseModel : WikiSABaseModel
    {
        [AllowHtml]
        public string ResultText { get; set; }

        public string ResultCss { get; set; }
    }
}