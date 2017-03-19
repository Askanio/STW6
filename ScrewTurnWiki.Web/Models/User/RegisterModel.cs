using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Code.Validators;
using ScrewTurn.Wiki.Web.Localization.Common;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class RegisterModel : WikiSABaseModel
    {
        public bool RegisterVisible { get; set; }

        public HtmlString Result { get; set; }

        public bool DisplayCaptcha { get; set; }

        public HtmlString Description { get; set; }

        public HtmlString AccountActivationMode { get; set; }

        [Required(ErrorMessageResourceName = "RfvUsername_ErrorMessage", ErrorMessageResourceType = typeof(Register))]
        [UserNameValidation]
        [AllowHtml]
        public string Username { get; set; }

        [DisplayUserNameValidation]
        [AllowHtml]
        public string DisplayName { get; set; }

        [Required(ErrorMessageResourceName = "RfvEmail1_ErrorMessage", ErrorMessageResourceType = typeof(Register))]
        [CheckExistsEmail(ErrorMessageResourceName = "EmailExists", ErrorMessageResourceType = typeof(Register))]
        [EmailValidation(ErrorMessageResourceName = "RxvEmail1_ErrorMessage", ErrorMessageResourceType = typeof(Register))]
        [DataType(DataType.EmailAddress)]
        public string Email1 { get; set; }

        [Required(ErrorMessageResourceName = "RfvEmail2_ErrorMessage", ErrorMessageResourceType = typeof(Register))]
        [System.ComponentModel.DataAnnotations.Compare("Email1", ErrorMessageResourceName = "CvEmail1_ErrorMessage", ErrorMessageResourceType = typeof(Register))]
        [DataType(DataType.EmailAddress)]
        public string Email2 { get; set; }

        [AllowHtml]
        [Required(ErrorMessageResourceName = "RfvPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Register))]
        [MinLength(6, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [MaxLength(50, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [PasswordValidation(ErrorMessageResourceName = "RxvPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Register))]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [AllowHtml]
        [Required(ErrorMessageResourceName = "RfvPassword2_ErrorMessage", ErrorMessageResourceType = typeof(Register))]
        [MinLength(6, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [MaxLength(50, ErrorMessageResourceName = "PasswordSizeMessage", ErrorMessageResourceType = typeof(Login))]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessageResourceName = "CvPassword1_ErrorMessage", ErrorMessageResourceType = typeof(Register))]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }

    }
}