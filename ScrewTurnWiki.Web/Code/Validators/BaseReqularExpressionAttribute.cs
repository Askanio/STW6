using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace ScrewTurn.Wiki.Web.Code.Validators
{
    public class BaseReqularExpressionAttribute : ValidationAttribute
    {
        protected string Pattern { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Pattern))
                return new ValidationResult("Pattern must be a valid string regex");

            var str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                // We consider that an empty string is valid for this property
                // Decorate with [Required] if this is not the case
                return null;
            }

            var match = Regex.Match(str, Pattern);
            if (!match.Success)
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));

            return null;
        }
    }
}