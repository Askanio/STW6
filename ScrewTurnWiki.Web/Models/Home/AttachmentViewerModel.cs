using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.Home
{
    public class AttachmentViewerModel
    {

        public string Name { get; set; }

        public string Size { get; set; }

        [AllowHtml]
        public string Link { get; set; }
    }
}