using System.Collections.Generic;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Code.Validators;

namespace ScrewTurn.Wiki.Web.Models.Admin.Groups
{
    public class EditUserGroupModel : AdminBaseModel
    {
        public EditUserGroupModel()
        {
            ActionsSelectorVisible = false;
            DescriptionEnabled = true;
        }

        [NewUserGroupNameValidation]
        [AllowHtml]
        public string GroupName { get; set; }

        public bool GroupNameEnabled { get; set; }

        public string Description { get; set; }

        public bool ActionsSelectorVisible { get; set; }

        public string[] ActionsGrant { get; set; }

        public string[] ActionsDeny { get; set; }

        public string SelectedProvider { get; set; }

        public List<SelectListItem> Providers { get; set; }

        public bool ButtonSaveVisible { get; set; }

        public bool ButtonDeleteVisible { get; set; }

        public bool DescriptionEnabled { get; set; }

        public bool CreateUserGroup { get; set; }

        public string CurrentGroupName { get; set; }
    }
}