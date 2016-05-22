using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Models;
using ScrewTurn.Wiki.Web.Models.Wiki;

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

        //public ActionResult Index()
        //{
        //    return View();
        //}

        //[HttpGet]
        //public ActionResult CreateMasterPassword()
        //{
        //    var model = new WikiBaseModel();
        //    base.PrepareCleanModel(model);
        //    return View(model);
        //}
    }
}