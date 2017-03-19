using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Code.Validators;
using ScrewTurn.Wiki.Web.Localization.Common;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class ProfilePasswordModel : BaseModel
    {
        [OldPasswordValidation(ErrorMessageResourceName = "CvOldPassword_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        [Required(ErrorMessageResourceName = "RfvOldPassword_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        [AllowHtml]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceName = "RfvPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        [MinLength(6, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [MaxLength(50, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [PasswordValidation(ErrorMessageResourceName = "RxvPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        [AllowHtml]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceName = "RfvPassword2_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        [MinLength(6, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [MaxLength(50, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        //[PasswordValidation(ErrorMessageResourceName = "RxNewPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessageResourceName = "CvPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        [AllowHtml]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
    }
}