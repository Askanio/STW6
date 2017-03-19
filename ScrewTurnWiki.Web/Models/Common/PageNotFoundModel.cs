using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.Common
{
    public class PageNotFoundModel : WikiModel
    {
        public string Description { get; set; }

        public MvcHtmlString SearchResults { get; set; }
    }
}