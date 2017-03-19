using System.ComponentModel.DataAnnotations;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class EmailMessageModel : BaseModel
    {
        [Required(ErrorMessageResourceName = "RfvSubject_ErrorMessage", ErrorMessageResourceType = typeof(Localization.Common.User))]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceName = "RfvBody_ErrorMessage", ErrorMessageResourceType = typeof(Localization.Common.User))]
        public string Body { get; set; }

        public string UserName { get; set; }
    }
}