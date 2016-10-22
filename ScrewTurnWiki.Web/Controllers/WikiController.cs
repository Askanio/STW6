using ScrewTurn.Wiki.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Code.Attributes;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models;
using ScrewTurn.Wiki.Web.Models.Wiki;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class WikiController : PageController
    {
      
        private bool DiscussMode { get; set; }

        private bool ViewCodeMode { get; set; }

        /// <summary>
        /// A value indicating whether the current user can post messages.
        /// </summary>
        private bool CanPostMessages { get; set; }

        /// <summary>
        /// A value indicating whether the current user can manage the discussion.
        /// </summary>
        private bool CanManageDiscussion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WikiController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public WikiController(ApplicationSettings settings) : base(settings)
        {
            DiscussMode = false;
            ViewCodeMode = false;
        }

        [HttpGet]
        [CheckExistPage]
        public ActionResult Index()
        {
            DiscussMode = Request["Discuss"] != null;
            ViewCodeMode = Request["Code"] != null && !DiscussMode;
            if (!Settings.GetEnableViewPageCodeFeature(CurrentWiki)) ViewCodeMode = false;

            return GeneratePage();           
        }

        [HttpGet]
        [CheckExistPage(PageParamName = "page")]
        public ActionResult Page(string page, string from)
        {
            return GeneratePage();
        }

        [HttpGet]
        [CheckExistPage(PageParamName = "page")]
        public ActionResult PageDiscuss(string page)
        {
            DiscussMode = true;
            return GeneratePage();
        }

        [HttpGet]
        [CheckExistPage(PageParamName = "page")]
        public ActionResult PageViewCode(string page)
        {
            ViewCodeMode = true;
            return GeneratePage();
        }

        private ActionResult GeneratePage()
        {
            //string notFoundPageName;
            //if (!InitializeCurrentPage(pageName, out notFoundPageName))
            //    return RedirectToAction("PageNotFound", "Common", new { page = notFoundPageName });

            //// Try to detect current namespace
            //CurrentNamespace = DetectCurrentNamespace(pageName);

            //if (!ExistsCurrentNamespace())
            //    return RedirectToAction("PageNotFound", "Common", new { page = pageName });

            //SetCurrentPage(pageName);

            //// Verifies the need for a redirect and performs it.
            //if (CurrentPage == null)
            //    return RedirectToAction("PageNotFound", "Common", new { page = Tools.UrlEncode(CurrentPageFullName) });

            if (Request["Edit"] == "1")
            {
                UrlTools.Redirect(UrlTools.BuildUrl(CurrentWiki, "Edit.aspx?Page=", Tools.UrlEncode(CurrentPage.FullName))); // TODO:
            }
            //if (Request["History"] == "1")
            //{
            //    UrlTools.Redirect(UrlTools.BuildUrl(CurrentWiki, "History.aspx?Page=", Tools.UrlEncode(CurrentPage.FullName))); // TODO:
            //}

            var model = new WikiPageModel();
            base.PrepareDefaultModel(model, CurrentNamespace, CurrentPageFullName);

            FillModel(model);

            return View("WikiPage", model);
        }

        //[HttpGet]
        //public JsonResult EmailNotification(string page, bool discuss)
        //{
        //    // <input type="image" name="ctl00$CphMaster$btnEmailNotification" id="ctl00_CphMaster_btnEmailNotification" title="Если желаете получать уведомления с этой страницы, поставьте отметку" class="inactivenotification" AutoUpdateAfterCallBack="True" UpdateAfterCallBack="True" src="Images/Blank.png" style="border-width:0px;" />
        //    // https://habrahabr.ru/post/180011/
        //    // http://ru.stackoverflow.com/questions/432591/%D0%9A%D0%B0%D0%BA-%D0%BE%D0%B1%D0%BD%D0%BE%D0%B2%D0%B8%D1%82%D1%8C-%D1%87%D0%B0%D1%81%D1%82%D1%8C-%D0%B4%D0%B0%D0%BD%D0%BD%D1%8B%D1%85-%D0%BD%D0%B0-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B5-%D0%B1%D0%B5%D0%B7-%D0%BE%D0%B1%D0%BD%D0%BE%D0%B2%D0%BB%D0%B5%D0%BD%D0%B8%D1%8F-%D0%B2%D1%81%D0%B5%D0%B9-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D1%8B
        //    // https://social.msdn.microsoft.com/Forums/ru-RU/f3c75180-0cbe-4108-a272-a6e8dd97c95e/aspnet-mvc-?forum=aspnetru
        //    // http://www.codeproject.com/Tips/857119/Update-a-div-content-using-Ajax-ActionLink

        //    bool pageChanges = false;
        //    bool discussionMessages = false;

        //    CurrentPage = Pages.FindPage(CurrentWiki, CurrentPageFullName); // ??????? Проверить

        //    UserInfo user = SessionFacade.GetCurrentUser(CurrentWiki);
        //    if (user != null)
        //    {
        //        Users.GetEmailNotification(user, CurrentPage.FullName, out pageChanges, out discussionMessages);
        //    }

        //    if (discuss)
        //    {
        //        Users.SetEmailNotification(CurrentWiki, user, CurrentPage.FullName, pageChanges, !discussionMessages);
        //    }
        //    else {
        //        Users.SetEmailNotification(CurrentWiki, user, CurrentPage.FullName, !pageChanges, discussionMessages);
        //    }

        //    var model = new EmailNotification();

        //    DiscussMode = discuss;
        //    var model = SetupEmailNotification();

        //    return Json(model, JsonRequestBehavior.AllowGet);
        //}

        ///// <summary>
        ///// Sets the email notification button.
        ///// </summary>
        //public EmailNotification SetupEmailNotification()
        //{
        //    if (SessionFacade.LoginKey != null && SessionFacade.CurrentUsername != "admin")
        //    {
        //        bool pageChanges = false;
        //        bool discussionMessages = false;

        //        UserInfo user = SessionFacade.GetCurrentUser(CurrentWiki);
        //        if (user != null && user.Provider.UsersDataReadOnly)
        //            return null;

        //        if (user != null)
        //        {
        //            Users.GetEmailNotification(user, CurrentPage.FullName, out pageChanges, out discussionMessages);
        //        }

        //        bool active = false;
        //        if (DiscussMode)
        //        {
        //            active = discussionMessages;
        //        }
        //        else {
        //            active = pageChanges;
        //        }

        //        var model = new EmailNotification();
        //        if (active)
        //        {
        //            model.CssClass = "activenotification" + (DiscussMode ? " discuss" : "");
        //            model.ToolTip = Messages.EmailNotificationsAreActive;
        //        }
        //        else {
        //            model.CssClass = "inactivenotification" + (DiscussMode ? " discuss" : "");
        //            model.ToolTip = Messages.ClickToEnableEmailNotifications;
        //        }
        //        //model.DiscussMode = DiscussMode;
        //        return model;
        //    }
        //    return null;
        //}

        private void FillModel(WikiPageModel model)
        {

            // The following actions are verified:
            // - View content (redirect to AccessDenied)
            // - Edit or Edit with Approval (for button display)
            // - Any Administrative activity (Rollback/Admin/Perms) (for button display)
            // - Download attachments (for button display - download permissions are also checked in GetFile)
            // - View discussion (for button display in content mode)
            // - Post discussion (for button display in discuss mode)

            string currentUsername = SessionFacade.GetCurrentUsername();
            string[] currentGroups = SessionFacade.GetCurrentGroupNames(CurrentWiki);

            //TODO: To Attribute
            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));

            bool canView = authChecker.CheckActionForPage(CurrentPage.FullName, Actions.ForPages.ReadPage, currentUsername, currentGroups);
            bool canEdit = false;
            bool canEditWithApproval = false;
            Pages.CanEditPage(CurrentWiki, CurrentPage.FullName, currentUsername, currentGroups, out canEdit, out canEditWithApproval);
            if (canEditWithApproval && canEdit)
                canEditWithApproval = false;
            bool canDownloadAttachments = authChecker.CheckActionForPage(CurrentPage.FullName, Actions.ForPages.DownloadAttachments, currentUsername, currentGroups);
            bool canSetPerms = authChecker.CheckActionForGlobals(Actions.ForGlobals.ManagePermissions, currentUsername, currentGroups);
            bool canAdmin = authChecker.CheckActionForPage(CurrentPage.FullName, Actions.ForPages.ManagePage, currentUsername, currentGroups);
            bool canViewDiscussion = authChecker.CheckActionForPage(CurrentPage.FullName, Actions.ForPages.ReadDiscussion, currentUsername, currentGroups);
            CanPostMessages = authChecker.CheckActionForPage(CurrentPage.FullName, Actions.ForPages.PostDiscussion, currentUsername, currentGroups);
            CanManageDiscussion = authChecker.CheckActionForPage(CurrentPage.FullName, Actions.ForPages.ManageDiscussion, currentUsername, currentGroups);

            if (!canView)
            {
                if (SessionFacade.LoginKey == null) UrlTools.Redirect("Login?Redirect=" + Tools.UrlEncode(Tools.GetCurrentUrlFixed()));
                else UrlTools.Redirect(UrlTools.BuildUrl(CurrentWiki, "AccessDenied"));
            }

            model.PageFullName = CurrentPage.FullName;
            model.PageFullNameEncode = Tools.UrlEncode(CurrentPage.FullName);
            model.AttachmentViewerVisible = canDownloadAttachments;
            
            model.PageInfoVisible = Settings.GetEnablePageInfoDiv(CurrentWiki);

            SetupTitles(model);

            SetupToolbarLinks(model, canEdit || canEditWithApproval, canViewDiscussion, CanPostMessages, canDownloadAttachments, canAdmin, canAdmin, canSetPerms);

            SetupLabels(model);
            SetupPrintAndRssLinks(model);
            SetupMetaInformation(model);
            VerifyAndPerformPageRedirection(model);
            SetupRedirectionSource(model);
            SetupNavigationPaths(model);
            SetupAdjacentPages(model);

            SessionFacade.Breadcrumbs(CurrentWiki).AddPage(CurrentPage.FullName);
            SetupBreadcrumbsTrail(model);

            SetupDoubleClickHandler(model);

            var enModel = PageHelper.SetupEmailNotification(CurrentWiki, CurrentPageFullName, DiscussMode);
            if (enModel != null)
            {
                var emailNotification = base.RenderPartialViewToString("EmailNotification", enModel);
                model.EmailNotification = new MvcHtmlString(emailNotification);
            }
            //model.EmailNotificationVisible = enModel != null;

            SetupPageContent(model);

            if (CurrentPage != null)
            {
                var canonical =
                    new MvcHtmlString(Tools.GetCanonicalUrlTag(Request.Url.ToString(), CurrentPage.FullName,
                        Pages.FindNamespace(CurrentWiki, NameTools.GetNamespace(CurrentPage.FullName))));

                model.HtmlHeads.Add(canonical);
            }
        }

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}

        /// <summary>
        /// Sets the titles used in the page.
        /// </summary>
        private void SetupTitles(WikiPageModel model)
        {
            string title = FormattingPipeline.PrepareTitle(CurrentWiki, CurrentPage.Title, false, FormattingContext.PageContent, CurrentPage.FullName);
            model.Title = title + " - " + Settings.GetWikiTitle(CurrentWiki);
            model.PageTitle = new MvcHtmlString(title);
        }

        /// <summary>
        /// Sets the content and visibility of all toolbar links.
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="canEdit">A value indicating whether the current user can edit the page.</param>
        /// <param name="canViewDiscussion">A value indicating whether the current user can view the page discussion.</param>
        /// <param name="canPostMessages">A value indicating whether the current user can post messages in the page discussion.</param>
        /// <param name="canDownloadAttachments">A value indicating whether the current user can download attachments.</param>
        /// <param name="canRollback">A value indicating whether the current user can rollback the page.</param>
        /// <param name="canAdmin">A value indicating whether the current user can perform at least one administration task.</param>
        /// <param name="canSetPerms">A value indicating whether the current user can set page permissions.</param>
        private void SetupToolbarLinks(WikiPageModel model, bool canEdit, bool canViewDiscussion, bool canPostMessages,
            bool canDownloadAttachments, bool canRollback, bool canAdmin, bool canSetPerms)
        {
            if (!DiscussMode && !ViewCodeMode && canViewDiscussion)
            {
                model.DiscussLink = new MvcHtmlString(
                    string.Format(@"<a id=""DiscussLink"" title=""{0}"" href=""{3}/Discuss"">{1} ({2})</a>",
                        Messages.Discuss, Messages.Discuss, Pages.GetMessageCount(CurrentPage), CurrentPageFullName));
                //model.DiscussLink = new MvcHtmlString(
                //    string.Format(@"<a id=""DiscussLink"" title=""{0}"" href=""{3}?Discuss=1"">{1} ({2})</a>",
                //        Messages.Discuss, Messages.Discuss, Pages.GetMessageCount(CurrentPage),
                //        UrlTools.BuildUrl(CurrentWiki, NameTools.GetLocalName(CurrentPage.FullName),
                //            GlobalSettings.PageExtension)));
            }

            if (Settings.GetEnablePageToolbar(CurrentWiki) && !DiscussMode && !ViewCodeMode && canEdit)
            {
                model.EditLink = new MvcHtmlString(
                    string.Format(@"<a id=""EditLink"" title=""{0}"" href=""{1}"">{2}</a>",
                        Messages.EditThisPage,
                        UrlTools.BuildUrl(CurrentWiki, "Edit.aspx?Page=", Tools.UrlEncode(CurrentPage.FullName)),
                        Messages.Edit));
            }

            if (Settings.GetEnablePageToolbar(CurrentWiki) && Settings.GetEnableViewPageCodeFeature(CurrentWiki))
            {
                if (!DiscussMode && !ViewCodeMode && !canEdit)
                {
                    model.ViewCodeLink =
                        new MvcHtmlString(
                            string.Format(@"<a id=""ViewCodeLink"" title=""{0}"" href=""{2}/Code"">{1}</a>",
                                Messages.ViewPageCode, Messages.ViewPageCode, CurrentPageFullName));
                    //model.ViewCodeLink =
                    //    new MvcHtmlString(
                    //        string.Format(@"<a id=""ViewCodeLink"" title=""{0}"" href=""{2}?Code=1"">{1}</a>",
                    //            Messages.ViewPageCode, Messages.ViewPageCode,
                    //            UrlTools.BuildUrl(CurrentWiki, NameTools.GetLocalName(CurrentPage.FullName),
                    //                GlobalSettings.PageExtension)));
                }
            }

            if (Settings.GetEnablePageToolbar(CurrentWiki) && !DiscussMode && !ViewCodeMode)
            {
                model.HistoryLink =
                    new MvcHtmlString(string.Format(@"<a id=""HistoryLink"" title=""{0}"" href=""{1}"">{2}</a>",
                        Messages.ViewPageHistory,
                        UrlTools.BuildUrl(CurrentWiki, Tools.UrlEncode(CurrentPage.FullName), "/History"),
                        Messages.History));
            }

            int attachmentCount = GetAttachmentCount();
            if (canDownloadAttachments && !DiscussMode && !ViewCodeMode && attachmentCount > 0)
            {
                model.AttachmentsLink =
                    new MvcHtmlString(
                        string.Format(
                            @"<a id=""PageAttachmentsLink"" title=""{0}"" href=""#"">{1}</a>",
                            Messages.Attachments, Messages.Attachments)); // onclick=""javascript:return __ToggleAttachmentsMenu(event.clientX, event.clientY);""
            }

            int bakCount = GetBackupCount();
            var adminToolsLinkVisible = Settings.GetEnablePageToolbar(CurrentWiki) && !DiscussMode && !ViewCodeMode &&
                ((canRollback && bakCount > 0) || canAdmin || canSetPerms);
            if (adminToolsLinkVisible)
            {
                model.AdminToolsLink =
                    new MvcHtmlString(
                        string.Format(
                            @"<a id=""AdminToolsLink"" title=""{0}"" href=""#"">{1}</a>",
                            Messages.AdminTools, Messages.Admin)); // onclick=""javascript:return __ToggleAdminToolsMenu(event.clientX, event.clientY);""

                if (canRollback && bakCount > 0)
                {
                    model.RollbackPage =
                        new MvcHtmlString(
                            string.Format(
                                @"<a id=""AdminPagesLink"" href=""AdminPages.aspx?Rollback={0}"" title=""{1}"">{2}</a>",
                                Tools.UrlEncode(CurrentPage.FullName),
                                Messages.RollbackThisPage, Messages.Rollback)); //  onclick=""javascript:return __RequestConfirm();""
                }

                if (canAdmin)
                {
                    model.AdministratePage =
                        new MvcHtmlString(string.Format(@"<a href=""AdminPages.aspx?Admin={0}"" title=""{1}"">{2}</a>",
                            Tools.UrlEncode(CurrentPage.FullName),
                            Messages.AdministrateThisPage, Messages.Administrate));
                }

                if (canSetPerms)
                {
                    model.SetPagePermissions =
                        new MvcHtmlString(string.Format(@"<a href=""AdminPages.aspx?Perms={0}"" title=""{1}"">{2}</a>",
                            Tools.UrlEncode(CurrentPage.FullName),
                            Messages.SetPermissionsForThisPage, Messages.Permissions));
                }
            }

            if (DiscussMode && !ViewCodeMode && canPostMessages)
            {
                model.PostMessageLink =
                    new MvcHtmlString(string.Format(@"<a id=""PostReplyLink"" title=""{0}"" href=""{1}"">{2}</a>",
                        Messages.PostMessage,
                        UrlTools.BuildUrl(CurrentWiki, "Post.aspx?Page=", Tools.UrlEncode(CurrentPage.FullName)),
                        Messages.PostMessage));
            }

            if (DiscussMode || ViewCodeMode)
            {
                model.BackLink =
                    new MvcHtmlString(
                        $@"<a id=""BackLink"" title=""{Messages.Back}"" href=""/{Tools.UrlEncode(CurrentPage.FullName)
                            }"">{Messages.Back}</a>");
            }
        }

        /// <summary>
        /// Gets the number of backups for the current page.
        /// </summary>
        /// <returns>The number of backups.</returns>
        private int GetBackupCount()
        {
            return Pages.GetBackups(CurrentPage).Count;
        }

        /// <summary>
        /// Gets the number of attachments for the current page.
        /// </summary>
        /// <returns>The number of attachments.</returns>
        private int GetAttachmentCount()
        {
            int count = 0;
            foreach (IFilesStorageProviderV60 prov in Collectors.CollectorsBox.FilesProviderCollector.GetAllProviders(CurrentWiki))
            {
                count += prov.ListPageAttachments(CurrentPage.FullName).Length;
            }
            return count;
        }

        /// <summary>
        /// Sets the content and visibility of all labels used in the page.
        /// </summary>
        private void SetupLabels(WikiPageModel model)
        {
            if (DiscussMode)
            {
                model.ModificationVisible = false;
                model.AuthorVisible = false;
                model.PageCategoriesVisible = false;
                model.NavigationPathsVisible = false;
                model.PageDiscussionVisible = true;
                model.DiscussedPage =
                    new MvcHtmlString("<b>" +
                                      FormattingPipeline.PrepareTitle(CurrentWiki, CurrentPage.Title, false,
                                          FormattingContext.PageContent, CurrentPage.FullName) + "</b>");
            }
            else {
                model.ModificationVisible = true;
                model.AuthorVisible = true;
                model.PageCategoriesVisible = true;
                model.NavigationPathsVisible = true;
                model.PageDiscussionVisible = false;

                model.ModifiedDateTime = new MvcHtmlString(
                    Preferences.AlignWithTimezone(CurrentWiki, CurrentPage.LastModified)
                        .ToString(Settings.GetDateTimeFormat(CurrentWiki)));
                model.Author = new MvcHtmlString(Users.UserLink(CurrentWiki, CurrentPage.User));
                model.PageCategories = new MvcHtmlString(GetFormattedPageCategories());
            }
        }

        /// <summary>
        /// Gets the categories for the current page, already formatted for display.
        /// </summary>
        /// <returns>The categories, formatted for display.</returns>
        private string GetFormattedPageCategories()
        {
            CategoryInfo[] categories = Pages.GetCategoriesForPage(CurrentPage);
            if (categories.Length == 0)
            {
                return string.Format(@"<i><a href=""{0}"" title=""{1}"">{2}</a></i>",
                    GetCategoryLink("-"),
                    Messages.Uncategorized, Messages.Uncategorized);
            }
            else {
                StringBuilder sb = new StringBuilder(categories.Length * 10);
                for (int i = 0; i < categories.Length; i++)
                {
                    sb.AppendFormat(@"<a href=""{0}"" title=""{1}"">{2}</a>",
                        GetCategoryLink(categories[i].FullName),
                        NameTools.GetLocalName(categories[i].FullName),
                        NameTools.GetLocalName(categories[i].FullName));
                    if (i != categories.Length - 1) sb.Append(", ");
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the link to a category.
        /// </summary>
        /// <param name="category">The full name of the category.</param>
        /// <returns>The link URL.</returns>
        private string GetCategoryLink(string category)
        {
            return UrlTools.BuildUrl(CurrentWiki, "AllPages?Cat=", Tools.UrlEncode(category));
        }

        /// <summary>
        /// Sets the Print and RSS links.
        /// </summary>
        private void SetupPrintAndRssLinks(WikiPageModel model)
        {
            if (!ViewCodeMode)
            {
                model.PrintLink =
                    new MvcHtmlString(
                        string.Format(@"<a id=""PrintLink"" href=""{0}"" title=""{1}"" target=""_blank"">{2}</a>",
                            UrlTools.BuildUrl(CurrentWiki, "Print.aspx?Page=", Tools.UrlEncode(CurrentPage.FullName),
                                DiscussMode ? "&amp;Discuss=1" : ""),
                            Messages.PrinterFriendlyVersion, Messages.Print));

                if (Settings.GetRssFeedsMode(CurrentWiki) != RssFeedsMode.Disabled)
                {
                    model.RssLink =
                        new MvcHtmlString(
                            string.Format(@"<a id=""RssLink"" href=""{0}"" title=""{1}"" target=""_blank""{2}>RSS</a>",
                                UrlTools.BuildUrl(CurrentWiki, "RSS.aspx?Page=", Tools.UrlEncode(CurrentPage.FullName),
                                    DiscussMode ? "&amp;Discuss=1" : ""),
                                DiscussMode ? Messages.RssForThisDiscussion : Messages.RssForThisPage,
                                DiscussMode ? " class=\"discuss\"" : ""));
                }
            }
        }

        /// <summary>
        /// Sets the content of the META description and keywords for the current page.
        /// </summary>
        private void SetupMetaInformation(WikiPageModel model)
        {
            // Set keywords and description
            if (CurrentPage.Keywords != null && CurrentPage.Keywords.Length > 0)
            {
                var lit =
                    new MvcHtmlString(string.Format("<meta name=\"keywords\" content=\"{0}\" />",
                        PrintKeywords(CurrentPage.Keywords)));
                model.HtmlHeads.Add(lit);

            }
            if (!string.IsNullOrEmpty(CurrentPage.Description))
            {
                var lit =
                    new MvcHtmlString(string.Format("<meta name=\"description\" content=\"{0}\" />",
                        CurrentPage.Description));
                model.HtmlHeads.Add(lit);
            }
        }

        /// <summary>
        /// Prints the keywords in a CSV list.
        /// </summary>
        /// <param name="keywords">The keywords.</param>
        /// <returns>The list.</returns>
        private string PrintKeywords(string[] keywords)
        {
            StringBuilder sb = new StringBuilder(50);
            for (int i = 0; i < keywords.Length; i++)
            {
                sb.Append(keywords[i]);
                if (i != keywords.Length - 1) sb.Append(", ");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Verifies the need for a page redirection, and performs it when appropriate.
        /// </summary>
        private void VerifyAndPerformPageRedirection(WikiPageModel model)
        {
            if (CurrentPage == null) return;

            PageContent dest = Redirections.GetDestination(CurrentPage.FullName);

            if (dest != null)
            {
                if (Request["NoRedirect"] != "1")
                {
                    UrlTools.Redirect(dest.FullName + GlobalSettings.PageExtension + "?From=" + CurrentPage.FullName, false);
                }
                else {
                    // Write redirection hint
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"<div id=""RedirectionDiv"">");
                    sb.Append(Messages.ThisPageRedirectsTo);
                    sb.Append(": ");
                    sb.Append(@"<a href=""");
                    UrlTools.BuildUrl(CurrentWiki, sb, "++", Tools.UrlEncode(dest.FullName), GlobalSettings.PageExtension, "?From=", Tools.UrlEncode(CurrentPage.FullName));
                    sb.Append(@""">");
                    sb.Append(FormattingPipeline.PrepareTitle(CurrentWiki, dest.Title, false, FormattingContext.PageContent, CurrentPage.FullName));
                    sb.Append("</a></div>");
                    model.Contents.Add(new MvcHtmlString(sb.ToString()));
                }
            }
        }

        /// <summary>
        /// Sets the redirection source page link, if appropriate.
        /// </summary>
        private void SetupRedirectionSource(WikiPageModel model)
        {
            if (Request["From"] != null)
            {

                PageContent source = Pages.FindPage(CurrentWiki, Request["From"]);

                if (source != null)
                {
                    StringBuilder sb = new StringBuilder(300);
                    sb.Append(@"<div id=""RedirectionInfoDiv"">");
                    sb.Append(Messages.RedirectedFrom);
                    sb.Append(": ");
                    sb.Append(@"<a href=""");
                    sb.Append(UrlTools.BuildUrl(CurrentWiki, "++", Tools.UrlEncode(source.FullName), GlobalSettings.PageExtension, "?NoRedirect=1"));
                    sb.Append(@""">");
                    sb.Append(FormattingPipeline.PrepareTitle(CurrentWiki, source.Title, false, FormattingContext.PageContent, CurrentPage.FullName));
                    sb.Append("</a></div>");

                    model.RedirectionSource = new MvcHtmlString(sb.ToString());
                }
            }
        }

        /// <summary>
        /// Sets the navigation paths label.
        /// </summary>
        private void SetupNavigationPaths(WikiPageModel model)
        {
            string[] paths = NavigationPaths.PathsPerPage(CurrentWiki, CurrentPage.FullName);

            string currentPath = Request["NavPath"];
            if (!string.IsNullOrEmpty(currentPath)) currentPath = currentPath.ToLowerInvariant();

            if (!DiscussMode && !ViewCodeMode && paths.Length > 0)
            {
                StringBuilder sb = new StringBuilder(500);
                sb.Append(Messages.Paths);
                sb.Append(": ");
                for (int i = 0; i < paths.Length; i++)
                {
                    NavigationPath path = NavigationPaths.Find(CurrentWiki, paths[i]);
                    if (path != null)
                    {
                        if (currentPath != null && paths[i].ToLowerInvariant().Equals(currentPath)) sb.Append("<b>");

                        sb.Append(@"<a href=""");
                        sb.Append(UrlTools.BuildUrl(CurrentWiki, "Default.aspx?Page=", Tools.UrlEncode(CurrentPage.FullName), "&amp;NavPath=", Tools.UrlEncode(paths[i])));
                        sb.Append(@""" title=""");
                        sb.Append(NameTools.GetLocalName(path.FullName));
                        sb.Append(@""">");
                        sb.Append(NameTools.GetLocalName(path.FullName));
                        sb.Append("</a>");

                        if (currentPath != null && paths[i].ToLowerInvariant().Equals(currentPath)) sb.Append("</b>");
                        if (i != paths.Length - 1) sb.Append(", ");
                    }
                }

                model.NavigationPaths = new MvcHtmlString(sb.ToString());
            }
        }

        /// <summary>
        /// Prepares the previous and next pages link for navigation paths.
        /// </summary>
        private void SetupAdjacentPages(WikiPageModel model)
        {
            StringBuilder prev = new StringBuilder(50), next = new StringBuilder(50);

            if (Request["NavPath"] != null)
            {
                NavigationPath path = NavigationPaths.Find(CurrentWiki, Request["NavPath"]);

                if (path != null)
                {
                    int idx = Array.IndexOf(path.Pages, CurrentPage.FullName);
                    if (idx != -1)
                    {
                        if (idx > 0)
                        {
                            PageContent prevPage = Pages.FindPage(CurrentWiki, path.Pages[idx - 1]);
                            prev.Append(@"<a href=""");
                            UrlTools.BuildUrl(CurrentWiki, prev, "Default.aspx?Page=", Tools.UrlEncode(prevPage.FullName),
                                "&amp;NavPath=", Tools.UrlEncode(path.FullName));

                            prev.Append(@""" title=""");
                            prev.Append(Messages.PrevPage);
                            prev.Append(": ");
                            prev.Append(FormattingPipeline.PrepareTitle(CurrentWiki, prevPage.Title, false, FormattingContext.PageContent, CurrentPage.FullName));
                            prev.Append(@"""><b>&laquo;</b></a> ");
                        }
                        if (idx < path.Pages.Length - 1)
                        {
                            PageContent nextPage = Pages.FindPage(CurrentWiki, path.Pages[idx + 1]);
                            next.Append(@" <a href=""");
                            UrlTools.BuildUrl(CurrentWiki, next, "Default.aspx?Page=", Tools.UrlEncode(nextPage.FullName),
                                "&amp;NavPath=", Tools.UrlEncode(path.FullName));

                            next.Append(@""" title=""");
                            next.Append(Messages.NextPage);
                            next.Append(": ");
                            next.Append(FormattingPipeline.PrepareTitle(CurrentWiki, nextPage.Title, false, FormattingContext.PageContent, CurrentPage.FullName));
                            next.Append(@"""><b>&raquo;</b></a>");
                        }
                    }
                }
            }

            if (prev.Length > 0)
            {
                model.PreviousPage = new MvcHtmlString(prev.ToString());
            }

            if (next.Length > 0)
            {
                model.NextPage = new MvcHtmlString(next.ToString());
            }
        }

        /// <summary>
        /// Sets the breadcrumbs trail, if appropriate.
        /// </summary>
        private void SetupBreadcrumbsTrail(WikiPageModel model)
        {
            if (Settings.GetDisableBreadcrumbsTrail(CurrentWiki) || DiscussMode || ViewCodeMode)
                return;

            StringBuilder sb = new StringBuilder(1000);

            sb.Append(@"<div id=""BreadcrumbsDiv"">");

            string[] pageTrailTemp = SessionFacade.Breadcrumbs(CurrentWiki).GetAllPages();
            List<PageContent> pageTrail = new List<PageContent>(pageTrailTemp.Length);
            // Build a list of pages the are currently available
            foreach (string pageFullName in pageTrailTemp)
            {
                PageContent p = Pages.FindPage(CurrentWiki, pageFullName);
                if (p != null) pageTrail.Add(p);
            }
            int min = 3;
            if (pageTrail.Count < 3) min = pageTrail.Count;

            sb.Append(@"<div id=""BreadcrumbsDivMin"">");
            if (pageTrail.Count > 3)
            {
                // Write hyperLink
                sb.Append(@"<a id =""ShowAllTrailLink"" href=""#"" title="""); // onclick = ""javascript: return __ShowAllTrail(); ""
                sb.Append(Messages.ViewBreadcrumbsTrail);
                sb.Append(@""">(");
                sb.Append(pageTrail.Count.ToString());
                sb.Append(")</a> ");
            }

            for (int i = pageTrail.Count - min; i < pageTrail.Count; i++)
            {
                AppendBreadcrumb(sb, pageTrail[i], "s");
            }
            sb.Append("</div>");

            sb.Append(@"<div id=""BreadcrumbsDivAll"" style=""display: none;"">");
            // Write hyperLink
            sb.Append(@"<a id=""HideTrailLink"" href=""#"" title="""); // onclick=""javascript:return __HideTrail();""
            sb.Append(Messages.HideBreadcrumbsTrail);
            sb.Append(@""">[X]</a> ");
            for (int i = 0; i < pageTrail.Count; i++)
            {
                AppendBreadcrumb(sb, pageTrail[i], "f");
            }
            sb.Append("</div>");

            sb.Append("</div>");

            model.BreadcrumbsTrail = new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Appends a breadbrumb trail element.
        /// </summary>
        /// <param name="sb">The destination <see cref="T:StringBuilder" />.</param>
        /// <param name="page">The page to append.</param>
        /// <param name="dpPrefix">The drop-down menu ID prefix.</param>
        private void AppendBreadcrumb(StringBuilder sb, PageContent page, string dpPrefix)
        {
            PageNameComparer comp = new PageNameComparer();

            // If the page does not exists return.
            if (page == null) return;

            string id = AppendBreadcrumbDropDown(sb, page.FullName, dpPrefix);

            string nspace = NameTools.GetNamespace(page.FullName);

            sb.Append("&raquo; ");
            //if (comp.Compare(page, CurrentPage) == 0) sb.Append("<b>");
            sb.AppendFormat(@"<a class=""breadcrumbdropdown {5}"" href=""{0}"" title=""{1}""{2}{3}{4}>{1}</a>",
                Tools.UrlEncode(page.FullName) + GlobalSettings.PageExtension,
                FormattingPipeline.PrepareTitle(CurrentWiki, page.Title, false, FormattingContext.PageContent, CurrentPage.FullName) + (string.IsNullOrEmpty(nspace) ? "" : (" (" + NameTools.GetNamespace(page.FullName) + ")")),
                "",//(id != null ? @" onmouseover=""javascript:return __ShowDropDown(event, '" + id + @"', this);""" : ""),
                (id != null ? @" id=""lnk" + id + @"""" : ""),
                "",//(id != null ? @" onmouseout=""javascript:return __HideDropDown('" + id + @"');""" : ""),
                comp.Compare(page, CurrentPage) == 0 ? "fontbold" : "");
            //if (comp.Compare(page, CurrentPage) == 0) sb.Append("</b>");
            sb.Append(" ");
        }

        /// <summary>
        /// Appends the drop-down menu DIV with outgoing links for a page.
        /// </summary>
        /// <param name="sb">The destination <see cref="T:StringBuilder" />.</param>
        /// <param name="pageFullName">The page full name.</param>
        /// <param name="dbPrefix">The drop-down menu DIV ID prefix.</param>
        /// <returns>The DIV ID, or <c>null</c> if no target pages were found.</returns>
        private string AppendBreadcrumbDropDown(StringBuilder sb, string pageFullName, string dbPrefix)
        {
            // Build outgoing links list
            // Generate list DIV
            // Return DIV's ID

            string[] outgoingLinks = Pages.GetPageOutgoingLinks(CurrentWiki, pageFullName);
            if (outgoingLinks == null || outgoingLinks.Length == 0) return null;

            string id = dbPrefix + Guid.NewGuid().ToString();

            StringBuilder buffer = new StringBuilder(300);

            //buffer.AppendFormat(@"<div id=""{0}"" style=""display: none;"" class=""pageoutgoinglinksmenu"" onmouseover=""javascript:return __CancelHideTimer();"" onmouseout=""javascript:return __HideDropDown('{0}');"">", id);
            buffer.AppendFormat(@"<div id=""{0}"" style=""display: none;"" class=""pageoutgoinglinksmenu"">", id);
            int count = 0;
            foreach (string link in outgoingLinks)
            {
                PageContent target = Pages.FindPage(CurrentWiki, link);
                if (target != null)
                {
                    count++;
                    string title = FormattingPipeline.PrepareTitle(CurrentWiki, target.Title, false, FormattingContext.PageContent, CurrentPage.FullName);

                    buffer.AppendFormat(@"<a href=""{0}{1}"" title=""{2}"">{2}</a>", link, GlobalSettings.PageExtension, title, title);
                }
                if (count >= 20) break;
            }
            buffer.Append("</div>");

            sb.Insert(0, buffer.ToString());

            if (count > 0) return id;
            return null;
        }

        /// <summary>
        /// Sets the JavaScript double-click editing handler.
        /// </summary>
        private void SetupDoubleClickHandler(WikiPageModel model)
        {
            if (Settings.GetEnableDoubleClickEditing(CurrentWiki) && !DiscussMode && !ViewCodeMode)
            {
                StringBuilder sb = new StringBuilder(200);
                sb.Append(@"<script type=""text/javascript"">" + "\n");
                sb.Append("<!--\n");
                sb.Append("document.ondblclick = function() {\n");
                sb.Append("document.location = '");
                sb.Append(UrlTools.BuildUrl(CurrentWiki, "Edit.aspx?Page=", Tools.UrlEncode(CurrentPage.FullName)));
                sb.Append("';\n");
                sb.Append("}\n");
                sb.Append("// -->\n");
                sb.Append("</script>");

                model.DoubleClickHandler = new MvcHtmlString(sb.ToString());
            }
        }

        /// <summary>
        /// Sets the actual page content, based on the current view mode (normal, discussion, view code).
        /// </summary>
        /// <param name="model"></param>
        private void SetupPageContent(WikiPageModel model)
        {
            if (!DiscussMode && !ViewCodeMode)
            {
                string content = null;
                if (CurrentPage != null)
                {
                    // Force formatting so that the destination can be detected
                    content = FormattedContent.GetFormattedPageContent(CurrentWiki, CurrentPage);
                }
                if (content != null)
                    model.Contents.Add(new MvcHtmlString(content));
            }
            else if (!DiscussMode && ViewCodeMode)
            {
                if (Settings.GetEnableViewPageCodeFeature(CurrentWiki))
                {
                    StringBuilder sb = new StringBuilder(CurrentPage.Content.Length + 100);
                    sb.Append(@"<textarea style=""width: 98%; height: 500px;"" readonly=""true"">");
                    sb.Append(Server.HtmlEncode(CurrentPage.Content));
                    sb.Append("</textarea>");
                    sb.Append("<br /><br />");
                    sb.Append(Messages.MetaKeywords);
                    sb.Append(": <b>");
                    sb.Append(PrintKeywords(CurrentPage.Keywords));
                    sb.Append("</b><br />");
                    sb.Append(Messages.MetaDescription);
                    sb.Append(": <b>");
                    sb.Append(CurrentPage.Description);
                    sb.Append("</b><br />");
                    sb.Append(Messages.ChangeComment);
                    sb.Append(": <b>");
                    sb.Append(CurrentPage.Comment);
                    sb.Append("</b>");
                    model.Contents.Add(new MvcHtmlString(sb.ToString()));
                }
            }
            else if (DiscussMode && !ViewCodeMode)
            {
                var discussion = GetDiscussion();
                model.Contents.Add(new MvcHtmlString(discussion));
            }
        }

        #region Discussion

        private string GetDiscussion()
        {
            var discussionModel = new DiscussionModel();
            RenderMessages(discussionModel);
            return this.RenderPartialViewToString("Discussion", discussionModel);
        }

        /// <summary>
        /// Renders the messages.
        /// </summary>
        private void RenderMessages(DiscussionModel model)
        {
            if (CurrentPage == null) return;

            Message[] messages = Pages.GetPageMessages(CurrentPage);

            if (messages.Length == 0)
            {
                model.Messages = new MvcHtmlString("<i>" + Messages.NoMessages + "</i>");
            }
            else {
                model.Messages = new MvcHtmlString(PrintDiscussion());
            }
        }

        /// <summary>
        /// Prints the discussion tree.
        /// </summary>
        /// <returns>The formatted page discussion.</returns>
        private string PrintDiscussion()
        {
            List<Message> messages = new List<Message>(Pages.GetPageMessages(CurrentPage));
            if (messages.Count == 0)
            {
                return "<i>" + Messages.NoMessages + "</i>";
            }
            else {
                StringBuilder sb = new StringBuilder(10000);
                PrintSubtree(messages, null, sb);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Prints a subtree of Messages depth-first.
        /// </summary>
        /// <param name="messages">The Messages.</param>
        /// <param name="parent">The parent message, or <c>null</c>.</param>
        /// <param name="sb">The output <see cref="T:StringBuilder" />.</param>
        private void PrintSubtree(IEnumerable<Message> messages, Message parent, StringBuilder sb)
        {
            foreach (Message msg in messages)
            {
                sb.Append(@"<div");
                sb.Append(parent != null ? @" class=""messagecontainer""" : @" class=""rootmessagecontainer""");
                sb.Append(">");
                PrintMessage(msg, parent, sb);
                PrintSubtree(msg.Replies, msg, sb);
                sb.Append("</div>");
            }
        }

        /// <summary>
        /// Prints a message.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="parent">The parent message, or <c>null</c>.</param>
        /// <param name="sb">The output <see cref="T:StringBuilder" />.</param>
        private void PrintMessage(Message message, Message parent, StringBuilder sb)
        {
            string currentWiki = Tools.DetectCurrentWiki();

            // Print header
            sb.Append(@"<div class=""messageheader"">");
            //sb.AppendFormat(@"<a id=""MSG_{0}""></a>", message.ID);

            if (!CurrentPage.Provider.ReadOnly)
            {
                // Print reply/edit/delete buttons only if provider is not read-only
                sb.Append(@"<div class=""reply"">");

                if (CanPostMessages)
                {
                    sb.Append(@"<a class=""reply"" href=""");
                    sb.Append(UrlTools.BuildUrl(currentWiki, "Post.aspx?Page=", Tools.UrlEncode(CurrentPage.FullName), "&amp;Parent=", message.ID.ToString()));

                    sb.Append(@""">");
                    sb.Append(Messages.Reply);
                    sb.Append("</a>");
                }

                // If current user is the author of the message or is an admin, print the edit hyperLink
                // A message can be edited only if the user is authenticated - anonymous users cannot edit their messages
                if (SessionFacade.LoginKey != null && ((message.Username == SessionFacade.CurrentUsername && CanPostMessages) || CanManageDiscussion))
                {
                    sb.Append(@" <a class=""edit"" href=""");
                    sb.Append(UrlTools.BuildUrl(currentWiki, "Post.aspx?Page=", Tools.UrlEncode(CurrentPage.FullName), "&amp;Edit=", message.ID.ToString()));

                    sb.Append(@""">");
                    sb.Append(Messages.Edit);
                    sb.Append("</a>");
                }

                // If the current user is an admin, print the delete hyperLink
                if (SessionFacade.LoginKey != null && CanManageDiscussion)
                {
                    sb.Append(@" <a class=""delete"" href=""");
                    sb.Append(UrlTools.BuildUrl(currentWiki, "Operation.aspx?Operation=DeleteMessage&amp;Message=", message.ID.ToString(),
                        "&amp;Page=", Tools.UrlEncode(CurrentPage.FullName)));

                    sb.Append(@""">");
                    sb.Append(Messages.Delete);
                    sb.Append("</a>");
                }
                sb.Append("</div>");
            }

            sb.Append(@"<div>");
            sb.AppendFormat(@"<a id=""{0}"" href=""#{0}"" title=""Permalink"">&#0182;</a> ", Tools.GetMessageIdForAnchor(message.DateTime));

            // Print subject
            if (message.Subject.Length > 0)
            {
                sb.Append(@"<span class=""messagesubject"">");
                sb.Append(FormattingPipeline.PrepareTitle(currentWiki, message.Subject, false, FormattingContext.MessageBody, CurrentPage.FullName));
                sb.Append("</span>");
            }

            // Print message date/time
            sb.Append(@"<span class=""messagedatetime"">");
            sb.Append(Preferences.AlignWithTimezone(currentWiki, message.DateTime).ToString(Settings.GetDateTimeFormat(currentWiki)));
            sb.Append(" ");
            sb.Append(Messages.By);
            sb.Append(" ");
            sb.Append(Users.UserLink(currentWiki, message.Username));
            sb.Append("</span>");

            sb.Append("</div>");

            sb.Append("</div>");

            // Print body
            sb.Append(@"<div class=""messagebody"">");
            sb.Append(FormattingPipeline.FormatWithPhase3(currentWiki, FormattingPipeline.FormatWithPhase1And2(currentWiki, message.Body, false, FormattingContext.MessageBody, CurrentPage.FullName),
                FormattingContext.MessageBody, CurrentPage.FullName));
            sb.Append("</div>");
        }

        #endregion

    }
}