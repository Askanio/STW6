using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Code.Validators;

namespace ScrewTurn.Wiki.Web.Models.Admin.Home
{
    public class BulkEmailModel : AdminBaseModel
    {
        public BulkEmailModel()
        {
            Groups = new List<SelectListItem>();
        }

        [RequiredSelectedValidation(ErrorMessageResourceName = "CvGroups_ErrorMessage", ErrorMessageResourceType = typeof(Localization.Admin.AdminHome))]
        public IList<SelectListItem> Groups { get; set; }

        [Required(ErrorMessageResourceName = "RfvSubject_ErrorMessage", ErrorMessageResourceType = typeof(Localization.Admin.AdminHome))]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceName = "RfvBody_ErrorMessage", ErrorMessageResourceType = typeof(Localization.Admin.AdminHome))]
        public string Body { get; set; }

        public string Result { get; set; }
    }
}