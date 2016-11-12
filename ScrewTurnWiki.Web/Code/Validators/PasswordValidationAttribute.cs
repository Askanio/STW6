using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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