using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Code.Validators
{
    public class RequiredSelectedValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var lst = value as IList<SelectListItem>;
            if (lst == null)
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));

            if (!lst.Any(item => item.Selected))
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));

            return null;
        }
    }
}