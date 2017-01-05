using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Ajax.Utilities;

namespace ScrewTurn.Wiki.Web.Code.Validators
{
    public class NewUserGroupNameValidationAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PropertyInfo property = validationContext.ObjectType.GetProperty("GroupNameEnabled");
            if (property == null)
            {
                return new ValidationResult("'GroupNameEnabled' is unknown property");
            }
            var groupNameEnabled = (bool)property.GetValue(validationContext.ObjectInstance, null);

            if (groupNameEnabled)
            {
                string pattern = GlobalSettings.UsernameRegex;

                var str = value as string;

                if (str.IsNullOrWhiteSpace())
                    return new ValidationResult(Localization.Admin.AdminGroups.RfvName_ErrorMessage);

                if (!string.IsNullOrEmpty(pattern))
                {
                    var match = Regex.Match(str.Trim(), pattern);
                    if (!match.Success)
                        return new ValidationResult(Localization.Admin.AdminGroups.RevName_ErrorMessage);
                }

                var currentWiki = Tools.DetectCurrentWiki();
                var name = Users.FindUserGroup(currentWiki, str);
                if (name != null)
                    return new ValidationResult(Localization.Admin.AdminGroups.CvName_ErrorMessage);
            }
            return null;
        }
    }
}