using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.Wiki
{
    public class DiscussionModel
    {
        [AllowHtml]
        public MvcHtmlString Messages { get; set; }
        
    }
}