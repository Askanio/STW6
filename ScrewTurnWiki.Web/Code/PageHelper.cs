using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models.Wiki;

namespace ScrewTurn.Wiki.Web.Code
{
    public static class PageHelper
    {
        /// <summary>
        /// Sets the email notification button.
        /// </summary>
        public static EmailNotification SetupEmailNotification(string currentWiki, string currentPageFullName, bool discussMode)
        {
            if (SessionFacade.LoginKey != null && SessionFacade.CurrentUsername != "admin")
            {
                bool pageChanges = false;
                bool discussionMessages = false;

                UserInfo user = SessionFacade.GetCurrentUser(currentWiki);
                if (user != null && user.Provider.UsersDataReadOnly)
                    return null;

                if (user != null)
                {
                    Users.GetEmailNotification(user, currentPageFullName, out pageChanges, out discussionMessages); // currentPage.FullName
                }

                bool active = false;
                if (discussMode)
                {
                    active = discussionMessages;
                }
                else {
                    active = pageChanges;
                }

                var model = new EmailNotification();
                if (active)
                {
                    model.CssClass = "activenotification" + (discussMode ? " discuss" : "");
                    model.ToolTip = Messages.EmailNotificationsAreActive;
                }
                else {
                    model.CssClass = "inactivenotification" + (discussMode ? " discuss" : "");
                    model.ToolTip = Messages.ClickToEnableEmailNotifications;
                }
                model.DiscussMode = discussMode;
                model.PageFullName = currentPageFullName;
                return model;
            }
            return null;
        }

        /// <summary>
        /// Exstract namespace from name of page
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        public static string DetectNamespace(string pageName)
        {
            if (string.IsNullOrEmpty(pageName))
                return "";

            var index = pageName.IndexOf(".", StringComparison.InvariantCulture);
            if (index == -1)
                return "";

            return pageName.Substring(0, index);
        }
    }
}