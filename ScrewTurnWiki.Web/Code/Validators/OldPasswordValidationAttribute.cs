using System.ComponentModel.DataAnnotations;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Web.Code.Validators
{
    public class OldPasswordValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                // We consider that an empty string is valid for this property
                // Decorate with [Required] if this is not the case
                return null;
            }

            var currentWiki = Tools.DetectCurrentWiki();
            UserInfo user = SessionFacade.GetCurrentUser(currentWiki);
            var isValid = user.Provider.TestAccount(user, str);
            if (!isValid)
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));

            return null;
        }
    }
}