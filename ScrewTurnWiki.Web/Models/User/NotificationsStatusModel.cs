using System.Collections.Generic;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class NotificationsStatusModel : BaseModel
    {
        public NotificationsStatusModel()
        {
            PageChanges = new List<SelectListItem>();
            DiscussionMessages = new List<SelectListItem>();
        }

        public IList<SelectListItem> PageChanges { get; set; }

        public IList<SelectListItem> DiscussionMessages { get; set; }
    }
}