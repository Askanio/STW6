using System;
using System.ComponentModel.DataAnnotations;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Web.Code.Validators
{
    public class CheckExistsEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var email = value as string;

            if (String.IsNullOrEmpty(email))
                return null;

            var currentWiki = Tools.DetectCurrentWiki();
            UserInfo u = Users.FindUserByEmail(currentWiki, email);
            if (u != null)
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            //return new ValidationResult(Localization.Common.Register.CvUsername_ErrorMessage);

            return null;
        }
    }
}