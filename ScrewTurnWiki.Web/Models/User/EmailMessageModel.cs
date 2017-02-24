using System;
using System.ComponentModel.DataAnnotations;
using ScrewTurn.Wiki.Web.Code.InfoMessages;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class EmailMessageModel
    {
        [Required(ErrorMessageResourceName = "RfvSubject_ErrorMessage", ErrorMessageResourceType = typeof(Localization.Common.User))]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceName = "RfvBody_ErrorMessage", ErrorMessageResourceType = typeof(Localization.Common.User))]
        public string Body { get; set; }

        public string UserName { get; set; }

        public InfoMessage Message { get; set; }
    }
}