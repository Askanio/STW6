using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.UI;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Code.Attributes;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models.User;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class UserController : PageController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public UserController(ApplicationSettings settings) : base(settings)
        {
        }

        #region EmailNotification

        [HttpGet]
        public ActionResult EmailNotification(string page, bool discussMode)
        {
            SetEmailNotification(page, discussMode);
            var model = PageHelper.SetupEmailNotification(CurrentWiki, page, discussMode);
            return PartialView("EmailNotification", model);
        }

        private void SetEmailNotification(string page, bool discuss)
        {
            bool pageChanges = false;
            bool discussionMessages = false;

            var currentPage = Pages.FindPage(CurrentWiki, page);

            UserInfo user = SessionFacade.GetCurrentUser(CurrentWiki);
            if (user != null)
            {
                Users.GetEmailNotification(user, currentPage.FullName, out pageChanges, out discussionMessages);
            }

            if (discuss)
            {
                Users.SetEmailNotification(CurrentWiki, user, currentPage.FullName, pageChanges, !discussionMessages);
            }
            else {
                Users.SetEmailNotification(CurrentWiki, user, currentPage.FullName, !pageChanges, discussionMessages);
            }
        }

        #endregion

        #region Login / logout

        [HttpGet]
        public ActionResult ForceLogout(string redirect)
        {
            SessionFacade.IsLoggingOut = true;
            Logout();
            if (redirect != null) Response.Redirect(redirect);
            return new ContentResult();
        }

        [HttpGet]
        public ActionResult Activate(string code ,string username)
        {
            var model = new LoginModel();
            PrepareBaseModel(model);

            // TODO: change Activate in Register!!!
            if (username != null)
            {
                AccountActivation(model, code, username);
            }
            else
            {
                model.ResultCss = "resulterror";
                model.ResultText = Messages.AccountNotFound;
            }
            return View("Login", model);
        }

        private void AccountActivation(LoginModel model, string code, string username)
        {
            UserInfo user = Users.FindUser(CurrentWiki, username);
            if (user != null && Tools.ComputeSecurityHash(user.Username, user.Email, user.DateTime).Equals(code))
            {
                Log.LogEntry("Account activation requested for " + user.Username, EntryType.General, Log.SystemUsername, CurrentWiki);
                if (user.Active)
                {
                    model.ResultCss = "resultok";
                    model.ResultText = Messages.AccountAlreadyActive;
                    return;
                }
                if (user.DateTime.AddHours(24).CompareTo(DateTime.Now) < 0)
                {
                    // Too late
                    model.ResultCss = "resulterror";
                    model.ResultText = Messages.AccountNotFound;
                    // Delete user (is this correct?)
                    Users.RemoveUser(CurrentWiki, user);
                    return;
                }
                // Activate User
                Users.SetActivationStatus(user, true);
                model.ResultCss = "resultok";
                model.ResultText = Messages.AccountActivated;
                return;
            }
            model.ResultCss = "resulterror";
            model.ResultText = Messages.AccountNotActivated;
            return;
        }

        [HttpGet]
        public ActionResult Login(string returnUrl) // string redirect
        {
            var model = new LoginModel();
            PrepareBaseModel(model);
            model.Description = GetLoginNotice();

            // In case of provider supporting autologin, a user might not be able to logout
            // without applying a "filter" because the provider might keep logging her in.
            // When she clicks Logout and redirects to Login.aspx?Logout=1 a flag is set,
            // avoiding autologin for the current session - see LoginTools class
            string logout = (string)TempData["Logout"];
            if (logout != null && logout == "1")
                SessionFacade.IsLoggingOut = true; // Request["Logout"]

            if (SessionFacade.LoginKey != null)
            {
                var logoutModel = new LogoutModel();
                PrepareBaseModel(logoutModel);
                logoutModel.Description = GetLoginNotice();
                logoutModel.LogoutText = "<b>" + SessionFacade.CurrentUsername + "</b>, " + Localization.Common.Login.LblLogout_Text;
                return View("Logout", logoutModel);
            }

            model.DisplayCaptcha = GlobalSettings.IsRecaptchaEnabled && GlobalSettings.ShowCaptchaOnLoginPage;
            this.ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [CaptchaValidator]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl, bool isCaptchaValid)
        {
            //validate CAPTCHA
            if (GlobalSettings.IsRecaptchaEnabled && GlobalSettings.ShowCaptchaOnLoginPage && !isCaptchaValid)
            {
                ModelState.AddModelError("", Messages.WrongCaptcha);
            }

            if (ModelState.IsValid)
            {
                model.ResultText = null;
                UserInfo user = Users.TryLogin(CurrentWiki, model.Username, model.Password);
                if (user != null)
                {
                    string loginKey = Users.ComputeLoginKey(user.Username, user.Email, user.DateTime);
                    if (model.Remember)
                    {
                        LoginTools.SetLoginCookie(user.Username, loginKey, DateTime.Now.AddYears(1));
                    }
                    LoginTools.SetupSession(CurrentWiki, user);
                    Log.LogEntry("User " + user.Username + " logged in", EntryType.General, Log.SystemUsername,
                        CurrentWiki);

                    var formattedRedirect = LoginTools.GetFormatedRedirect(returnUrl, CurrentWiki, true);
                    return Redirect(formattedRedirect);
                }
                else
                {
                    model.ResultCss = "resulterror";
                    model.ResultText = Messages.WrongUsernamePassword;
                }
            }

            //if (model.ResultText != null)
            //    ModelState.AddModelError("", model.ResultText);

            model.Password = "";
            PrepareBaseModel(model);
            model.Description = GetLoginNotice();
            model.DisplayCaptcha = GlobalSettings.IsRecaptchaEnabled && GlobalSettings.ShowCaptchaOnLoginPage;
            this.ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpGet]
        public ActionResult PasswordReset()
        {
            var model = new PasswordResetModel();
            PrepareBaseModel(model);
            model.DisplayCaptcha = GlobalSettings.IsRecaptchaEnabled && GlobalSettings.ShowCaptchaOnPasswordResetPage;
            return View(model);
        }

        [HttpPost]
        [CaptchaValidator]
        [ValidateAntiForgeryToken]
        public ActionResult PasswordReset(PasswordResetModel model, bool isCaptchaValid)
        {
            //validate CAPTCHA
            if (GlobalSettings.IsRecaptchaEnabled && GlobalSettings.ShowCaptchaOnPasswordResetPage && !isCaptchaValid)
            {
                ModelState.AddModelError("", Messages.WrongCaptcha);
            }

            if (ModelState.IsValid)
            {
                // Find the user
                if (model.UsernameReset != null)
                    model.UsernameReset = model.UsernameReset.Trim();
                if (model.EmailReset != null)
                    model.EmailReset = model.EmailReset.Trim();

                UserInfo user = null;
                if (!string.IsNullOrEmpty(model.UsernameReset))
                {
                    user = Users.FindUser(CurrentWiki, model.UsernameReset);
                }
                else if (!string.IsNullOrEmpty(model.EmailReset))
                {
                    user = Users.FindUserByEmail(CurrentWiki, model.EmailReset);
                }

                if (user != null)
                {
                    Log.LogEntry("Password reset message sent for " + user.Username, EntryType.General, Log.SystemUsername, CurrentWiki);

                    Users.SendPasswordResetMessage(CurrentWiki, user.Username, user.Email, user.DateTime);

                    model.ResultCss = "resultok";
                    model.ResultText = Messages.AMessageWasSentCheckInbox;
                    model.UsernameReset = "";
                    model.EmailReset = "";
                }
            }
            PrepareBaseModel(model);
            return View(model);
        }

        [HttpGet]
        public ActionResult PasswordRecovery(string resetCode, string username)
        {
            if (resetCode == null || username == null || LoadUserForPasswordReset(username, resetCode) == null)
            {
                return RedirectToAction("Login");
            }

            var model = new PasswordRecoveryModel();
            PrepareBaseModel(model);
            this.ViewBag.Username = username;
            this.ViewBag.Resetcode = resetCode;
            model.HidePasswordFiealds = false;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PasswordRecovery(PasswordRecoveryModel model, string resetCode, string username)
        {
            if (ModelState.IsValid)
            {
                UserInfo user = LoadUserForPasswordReset(username, resetCode);
                if (user != null)
                {
                    Users.ChangePassword(user, model.NewPassword);

                    PrepareBaseModel(model);
                    model.HidePasswordFiealds = true;
                    model.ResultCss = "resultok";
                    model.ResultText = Messages.NewPasswordSavedPleaseLogin;
                    return View(model);
                }
            }
            PrepareBaseModel(model);
            model.NewPassword = null;
            model.ConfirmNewPassword = null;
            this.ViewBag.Username = username;
            this.ViewBag.Resetcode = resetCode;
            model.HidePasswordFiealds = false;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout(LoginModel model, string returnUrl)
        {
            Logout();
            TempData["Logout"] = "1";
            return RedirectToAction("Login");
        }

        private void PrepareBaseModel(LoginBaseModel model)
        {
            PrepareSAModel(model, CurrentNamespace);
            model.Title = Messages.LoginTitle + " - " + Settings.GetWikiTitle(CurrentWiki);
        }

        /// <summary>
        /// Performs the logout.
        /// </summary>
        private void Logout()
        {
            Users.NotifyLogout(CurrentWiki, SessionFacade.CurrentUsername);
            LoginTools.SetLoginCookie("", "", DateTime.Now.AddYears(-1));
            Log.LogEntry("User " + SessionFacade.CurrentUsername + " logged out", EntryType.General, Log.SystemUsername, CurrentWiki);
            SessionFacade.Clear();
        }

        /// <summary>
        /// Loads the user for the password reset procedure.
        /// </summary>
        /// <returns>The user, or <c>null</c>.</returns>
        private UserInfo LoadUserForPasswordReset(string username, string resetCode)
        {
            UserInfo user = Users.FindUser(CurrentWiki, username);
            if (user != null && resetCode == Tools.ComputeSecurityHash(user.Username, user.Email, user.DateTime))
            {
                return user;
            }
            else return null;
        }

        /// <summary>
        /// Get the login notice.
        /// </summary>
        private string GetLoginNotice()
        {
            string n = Settings.GetProvider(CurrentWiki).GetMetaDataItem(MetaDataItem.LoginNotice, null);
            if (!string.IsNullOrEmpty(n))
            {
                n = FormattingPipeline.FormatWithPhase1And2(CurrentWiki, n, false, FormattingContext.Other, null);
            }
            if (!string.IsNullOrEmpty(n))
                return FormattingPipeline.FormatWithPhase3(CurrentWiki, n, FormattingContext.Other, null);

            return null;
        }

        #endregion

    }
}