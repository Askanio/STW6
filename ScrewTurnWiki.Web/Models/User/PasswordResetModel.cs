using System;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class PasswordResetModel : LoginBaseModel
    {
        public string UsernameReset { get; set; }

        public string EmailReset { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}