using System;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;

namespace ScrewTurn.Wiki.Web.Code.Extensions
{
    public static class CaptchaExtensions
    {
        public static string GenerateCaptcha(this HtmlHelper helper)
        {
            var theme = "white";  //!string.IsNullOrEmpty(captchaSettings.ReCaptchaTheme) ? captchaSettings.ReCaptchaTheme : "white";
            var captchaControl = new Recaptcha.RecaptchaControl
            {
                ID = "recaptcha",
                Theme = theme,
                PublicKey = GlobalSettings.RecaptchaPublicKey,
                PrivateKey = GlobalSettings.RecaptchaPrivateKey
            };

            var htmlWriter = new HtmlTextWriter(new StringWriter());

            captchaControl.RenderControl(htmlWriter);

            return htmlWriter.InnerWriter.ToString();
        }
    }
}