using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Localization.Common;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class LoginModel : LoginBaseModel
    {
        public bool Remember { get; set; }

        [Required(ErrorMessageResourceName = "UsernameIsRequired", ErrorMessageResourceType = typeof(Login))]
        [AllowHtml]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessageResourceName = "RfvNewPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Login))]
        [AllowHtml]
        public string Password { get; set; }

        public bool DisplayCaptcha { get; set; }

        [AllowHtml]
        public string Description { get; set; }
    }
}