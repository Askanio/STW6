using System.Collections.Generic;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class UserProfileModel : WikiSABaseModel
    {

        public UserProfileModel()
        {
            PageChanges = new List<SelectListItem>();
            DiscussionMessages = new List<SelectListItem>();
        }

        public string LblTitle { get; set; }

        public string Username { get; set; }

        public string Groups { get; set; }

        public bool UserDataVisible { get; set; }

        public bool AccountVisible { get; set; }

        public bool NoChangesVisible { get; set; }
        
        public IList<SelectListItem> PageChanges { get; set; }

        public IList<SelectListItem> DiscussionMessages { get; set; }

        public string SelectedLanguage { get; set; }

        public string SelectedTimezone { get; set; }

        public List<SelectListItem> Languages { get; set; }

        public List<SelectListItem> Timezones { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

    }
}