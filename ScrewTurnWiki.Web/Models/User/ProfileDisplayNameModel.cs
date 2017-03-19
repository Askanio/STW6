using ScrewTurn.Wiki.Web.Code.Validators;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class ProfileDisplayNameModel : BaseModel
    {
        [DisplayUserNameValidation]
        public string DisplayName { get; set; }
    }
}