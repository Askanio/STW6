using System.Collections.Generic;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Code.InfoMessages;

namespace ScrewTurn.Wiki.Web.Models
{
    public class BaseModel
    {
        public BaseModel()
        {
            HtmlHeads = new List<MvcHtmlString>();
            Direction = GlobalSettings.Direction;
        }

        public string Direction { get; set; }

        public string Title { get; set; }

        [AllowHtml]
        public List<MvcHtmlString> HtmlHeads { get; set; }

        public InfoMessage Message { get; set; }
    }
}