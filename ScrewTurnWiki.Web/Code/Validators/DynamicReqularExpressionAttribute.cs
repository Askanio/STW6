﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace ScrewTurn.Wiki.Web.Code.Validators
{
    public class DynamicReqularExpressionAttribute : ValidationAttribute
    {
        public string PatternProperty { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //         [AllowHtml]
            //public string NewPasswordValidationExpression { get; set; }
            //Sample:  [DynamicReqularExpression(PatternProperty = "NewPasswordValidationExpression", ErrorMessageResourceName = "InvalidPassword", ErrorMessageResourceType = typeof(Messages))]

            PropertyInfo property = validationContext.ObjectType.GetProperty(PatternProperty);
            if (property == null)
            {
                return new ValidationResult(string.Format("{0} is unknown property", PatternProperty));
            }
            var pattern = property.GetValue(validationContext.ObjectInstance, null) as string; // GlobalSettings.PasswordRegex; //
            if (string.IsNullOrEmpty(pattern))
            {
                return new ValidationResult(string.Format("{0} must be a valid string regex", PatternProperty));
            }

            var str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                // We consider that an empty string is valid for this property
                // Decorate with [Required] if this is not the case
                return null;
            }

            var match = Regex.Match(str, pattern);
            if (!match.Success)
            {
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            }

            return null;
        }
    }
}