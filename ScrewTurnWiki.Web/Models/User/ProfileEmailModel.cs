using System.ComponentModel.DataAnnotations;
using ScrewTurn.Wiki.Web.Code.Validators;
using ScrewTurn.Wiki.Web.Localization.Common;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class ProfileEmailModel : BaseModel
    {
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessageResourceName = "RfvEmail1_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        [EmailValidation(ErrorMessageResourceName = "RxvEmail1_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        public string Email1 { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessageResourceName = "RfvEmail2_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        [Compare("Email1", ErrorMessageResourceName = "CvEmail1_ErrorMessage", ErrorMessageResourceType = typeof(Profile))]
        public string Email2 { get; set; }
    }
}