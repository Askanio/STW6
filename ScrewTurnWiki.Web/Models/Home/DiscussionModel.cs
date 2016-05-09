using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.Home
{
    public class DiscussionModel
    {
        [AllowHtml]
        public MvcHtmlString Messages { get; set; }
        
    }
}