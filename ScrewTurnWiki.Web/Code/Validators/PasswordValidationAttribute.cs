using System.ComponentModel.DataAnnotations;

namespace ScrewTurn.Wiki.Web.Code.Validators
{
    public class PasswordValidationAttribute : BaseReqularExpressionAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Pattern = GlobalSettings.PasswordRegex;
            return base.IsValid(value, validationContext);
        }
    }
}