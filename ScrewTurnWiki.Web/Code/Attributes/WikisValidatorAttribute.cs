using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScrewTurn.Wiki.Web.Code.Attributes
{
    public class WikisValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var wikis = value as Dictionary<string, string>;
            if (wikis != null)
            {
                if (!wikis.ContainsKey("root"))
                    return new ValidationResult("There is no default wiki (root)");

                foreach (var wiki in wikis)
                {
                    if (wiki.Key.Length > 100)
                        return new ValidationResult(String.Format("Name of wiki '{0}' more 100 symbols", wiki.Key));
                    if (wiki.Key != "root" && (wiki.Value == null || wiki.Value.Trim().Length == 0))
                        return new ValidationResult(validationContext.DisplayName + " is required hosts");
                }

                //TODO: if (Regex.IsMatch(wiki.Key, @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", RegexOptions.IgnoreCase))
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("" + validationContext.DisplayName + " is required");
            }
        }
    }
}