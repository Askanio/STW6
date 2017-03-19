using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.Ajax.Utilities;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Web.Code.Validators
{
    public class UserNameValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string pattern = GlobalSettings.UsernameRegex;

            var username = value as string;

            if (username.IsNullOrWhiteSpace())
                return null;//new ValidationResult(Localization.Common.Register.RfvUsername_ErrorMessage);

            username = username.Trim();

            if (!string.IsNullOrEmpty(pattern))
            {
                var match = Regex.Match(username, pattern);
                if (!match.Success)
                    return new ValidationResult(Localization.Common.Register.RxvUserName_ErrorMessage);
            }

            if (username.ToLowerInvariant().Equals("admin") || username.ToLowerInvariant().Equals("guest"))
            {
                return new ValidationResult(Localization.Common.Register.CvUsername_ErrorMessage);
            }
            else
            {
                var currentWiki = Tools.DetectCurrentWiki();
                UserInfo u = Users.FindUser(currentWiki, username);
                if (u != null)
                    return new ValidationResult(Localization.Common.Register.CvUsername_ErrorMessage);
            }

            return null;
        }
    }
}