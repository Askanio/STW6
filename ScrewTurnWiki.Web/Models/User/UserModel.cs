using System.Web;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class UserModel : WikiSABaseModel
    {
        public string LblTitle { get; set; }

        public HtmlString Gravatar { get; set; }

        public bool NoActivityVisible { get; set; }

        public HtmlString RecentActivity { get; set; }

        public bool PanelMessageVisible { get; set; }

        public string UserName { get; set; }
    }
}