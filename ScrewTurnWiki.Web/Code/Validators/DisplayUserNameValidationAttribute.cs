using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Ajax.Utilities;

namespace ScrewTurn.Wiki.Web.Code.Validators
{
    public class DisplayUserNameValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string pattern = GlobalSettings.DisplayNameRegex;

            var str = value as string;

            if (str.IsNullOrWhiteSpace())
                return new ValidationResult(Localization.Common.Profile.RxvDisplayName_ErrorMessage);

            if (!string.IsNullOrEmpty(pattern))
            {
                var match = Regex.Match(str.Trim(), pattern);
                if (!match.Success)
                    return new ValidationResult(Localization.Common.Profile.RxvDisplayName_ErrorMessage);
            }

            // TODO: Check exists dublicate displayName!
            //var currentWiki = Tools.DetectCurrentWiki();
            //var name = Users.FindUserGroup(currentWiki, str);
            //if (name != null)
            //    return new ValidationResult(Localization.Admin.AdminGroups.CvName_ErrorMessage);
            return null;
        }
    }
}