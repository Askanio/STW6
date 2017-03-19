using System.Collections.Generic;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models
{
    public class WikiBaseModel : BaseModel
    {
        public WikiBaseModel()
        {
            HtmlHeads = new List<MvcHtmlString>();
            Direction = GlobalSettings.Direction;
        }

        public string Direction { get; set; }

        public string Title { get; set; }

        [AllowHtml]
        public List<MvcHtmlString> HtmlHeads { get; set; }
    }
}