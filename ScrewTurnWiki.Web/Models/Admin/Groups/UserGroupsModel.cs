using System.Collections.Generic;
using ScrewTurn.Wiki.Web.Code;

namespace ScrewTurn.Wiki.Web.Models.Admin.Groups
{
    public class UserGroupsModel : AdminBaseModel
    {
        public UserGroupsModel()
        {
            Groups = new List<UserGroupRow>();
        }

        public List<UserGroupRow> Groups { get; set; }

        public bool NewGroupButtonEnable { get; set; }
    }
}