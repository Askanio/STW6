using System.ComponentModel.DataAnnotations;

namespace ScrewTurn.Wiki.Web.Code.Validators
{
    public class EmailValidationAttribute : BaseReqularExpressionAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Pattern = GlobalSettings.EmailRegex;
            return base.IsValid(value, validationContext);
        }
    }
}