using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Code.Validators;
using ScrewTurn.Wiki.Web.Localization.Common;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class PasswordRecoveryModel : LoginBaseModel
    {

        public bool HidePasswordFiealds { get; set; }
        
        [Required(ErrorMessageResourceName = "RfvNewPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Login))]
        [MinLength(6, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [MaxLength(50, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [PasswordValidation(ErrorMessageResourceName = "RxNewPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Login))]
        [AllowHtml]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceName = "RfvNewPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Login))]
        [MinLength(6, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [MaxLength(50, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [PasswordValidation(ErrorMessageResourceName = "RxNewPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Login))]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessageResourceName = "PasswordsNotEqual", ErrorMessageResourceType = typeof(Login))]
        [AllowHtml]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
    }
}