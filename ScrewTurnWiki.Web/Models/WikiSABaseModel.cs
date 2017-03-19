using System;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models
{
    public class WikiSABaseModel : WikiModel
    {

        public String LnkMainPageUrl { get; set; }

        public MvcHtmlString LnkPreviousPageUrl { get; set; }
    }
}