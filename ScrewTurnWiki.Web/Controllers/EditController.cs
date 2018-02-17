using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code.Attributes;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models.Edit;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class EditController : PageController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WikiController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public EditController(ApplicationSettings settings) : base(settings)
        {
        }


        [HttpGet]
        public ActionResult Edit(string page, int? section)
        {
            PageContent currentPage = null;
            int currentSection = -1;
            CurrentNamespace = Tools.DetectCurrentNamespace();
            bool isDraft = false;

            var model = new EditModel();
            PrepareSAModel(model, CurrentNamespace);

            model.Title = Messages.EditTitle + " - " + Settings.GetWikiTitle(CurrentWiki);

            model.EditNotice = new HtmlString(Formatter.FormatPhase3(CurrentWiki, Formatter.Format(CurrentWiki, Settings.GetProvider(CurrentWiki).GetMetaDataItem(
                MetaDataItem.EditNotice, CurrentNamespace), false, FormattingContext.Other, null), FormattingContext.Other, null));

            //// Prepare page unload warning
            //string ua = Request.UserAgent;
            //if (!string.IsNullOrEmpty(ua))
            //{
                //ua = ua.ToLowerInvariant();
                //StringBuilder sbua = new StringBuilder(50);
                //sbua.Append(@"<script type=""text/javascript"">");
                //sbua.Append("\r\n<!--\r\n");
                //if (ua.Contains("gecko"))
                //{
                //    // Mozilla
                //    sbua.Append("addEventListener('beforeunload', __UnloadPage, true);");
                //}
                //else
                //{
                //    // IE
                //    sbua.Append("window.attachEvent('onbeforeunload', __UnloadPage);");
                //}
                //sbua.Append("\r\n// -->\r\n");
                //sbua.Append("</script>");
                //lblUnloadPage.Text = sbua.ToString();
            //}

            //if (!Page.IsPostBack)
            //{
                PopulateCategories(new CategoryInfo[0], currentPage);

                if (Settings.GetAutoGeneratePageNames(CurrentWiki))
                {
                    model.PageNameVisible = false;
                    model.ManualNameVisible = true;
                }
            //}

            // Load requested page, if any
            if (page != null)
            {
                //string name = null;
                //if (Request["Page"] != null)
                //{
                //    name = Request["Page"];
                //}
                //else
                //{
                //    name = model.Name.ToString();
                //}

                currentPage = Pages.FindPage(CurrentWiki, page);

                // If page already exists, load the content and disable page name,
                // otherwise pre-fill page name
                if (currentPage != null)
                {
                    keepAlive.CurrentPage = currentPage.FullName;

                    // Look for a draft
                    PageContent draftContent = Pages.GetDraft(currentPage);

                    if (draftContent == null)
                        draftContent = currentPage;
                    else
                        isDraft = true;

                    // Set current page for editor and attachment manager
                    editor.CurrentPage = currentPage;
                    attachmentManager.CurrentPage = currentPage;

                    if (section != null) currentSection = (int)section; //if (!int.TryParse(Request["Section"], out currentSection)) currentSection = -1;

                    //// Fill data, if not posted back
                    //if (!Page.IsPostBack)
                    //{
                    // Set keywords, description
                    model.Keywords =  SetKeywords(draftContent.Keywords);
                        model.Description = draftContent.Description;

                    model.Name = new HtmlString(NameTools.GetLocalName(currentPage.FullName));
                        model.NameEnabled = false;
                        model.PageNameVisible = false;
                        model.ManualNameVisible = false;

                        PopulateCategories(Pages.GetCategoriesForPage(currentPage), currentPage);

                        model.PageTitle = new HtmlString(draftContent.Title);

                        // Manage section, if appropriate (disable if draft)
                        if (!isDraft && currentSection != -1)
                        {
                            int startIndex, len;
                            string dummy = "";
                            ExtractSection(draftContent.Content, currentSection, out startIndex, out len, out dummy);
                            editor.SetContent(draftContent.Content.Substring(startIndex, len), Settings.GetUseVisualEditorAsDefault(CurrentWiki));
                        }
                        else
                        {
                            // Select default editor view (WikiMarkup or Visual) and populate content
                            editor.SetContent(draftContent.Content, Settings.GetUseVisualEditorAsDefault(CurrentWiki));
                        }
                    //}
                }
                else
                {
                    //// Pre-fill name, if not posted back
                    //if (!Page.IsPostBack)
                    //{
                        // Set both name and title, as the NAME was provided from the query-string and must be preserved
                        model.PageNameVisible = true;
                        model.ManualNameVisible = false;
                    model.Name = new HtmlString(NameTools.GetLocalName(page));
                        model.PageTitle = model.Name;
                        editor.SetContent(LoadTemplateIfAppropriate(model), Settings.GetUseVisualEditorAsDefault(CurrentWiki));
                    //}
                }
            }
            else
            {
                //if (!Page.IsPostBack)
                //{
                    model.MinorChangeVisible = false;
                model.SaveAsDraftVisible = false;

                    editor.SetContent(LoadTemplateIfAppropriate(model), Settings.GetUseVisualEditorAsDefault(CurrentWiki));
                //}
            }

            // Here is centralized all permissions-checking code
            var permissions = DetectPermissions(currentPage);

            // Verify the following permissions:
            // - if new page, check for page creation perms
            // - else, check for editing perms
            //    - full edit or edit with approval
            // - categories management
            // - attachment manager
            // - CAPTCHA if enabled and user is anonymous
            // ---> recheck every time an action is performed

            if (currentPage == null)
            {
                // Check permissions for creating new pages
                if (!permissions.CanCreateNewPages)
                {
                    if (SessionFacade.LoginKey == null) UrlTools.Redirect("Login?Redirect=" + Tools.UrlEncode(Tools.GetCurrentUrlFixed()));
                    else UrlTools.Redirect("AccessDenied"); // TODO:
                }
            }
            else
            {
                // Check permissions for editing current page
                if (!permissions.CanEdit && !permissions.CanEditWithApproval)
                {
                    if (SessionFacade.LoginKey == null) UrlTools.Redirect("Login?Redirect=" + Tools.UrlEncode(Tools.GetCurrentUrlFixed()));
                    else UrlTools.Redirect("AccessDenied"); // TODO:
                }
            }

            if (!permissions.CanEdit && permissions.CanEditWithApproval)
            {
                // Hard-wire status of draft and minor change checkboxes
                model.MinorChangeEnabled = false;
                model.SaveAsDraftEnabled = false;
                model.SaveAsDraft = true;
            }

            // Setup categories
            lstCategories.Enabled = permissions.CanManagePageCategories;
            pnlCategoryCreation.Visible = permissions.CanCreateNewCategories;

            // Setup attachment manager (require at least download permissions)
            attachmentManager.Visible = permissions.CanDownloadAttachments;

            // CAPTCHA
            model.DisplayCaptcha = SessionFacade.LoginKey == null && !Settings.GetDisableCaptchaControl(CurrentWiki);
            //pnlCaptcha.Visible = SessionFacade.LoginKey == null && !Settings.GetDisableCaptchaControl(CurrentWiki);
            //captcha.Visible = pnlCaptcha.Visible;

            // Moderation notice
            pnlApprovalRequired.Visible = !permissions.CanEdit && permissions.CanEditWithApproval;

            // Check and manage editing collisions
            ManageEditingCollisions(currentPage, currentSection);

            //if (!Page.IsPostBack)
            //{
                ManageTemplatesDisplay();

                // Display draft status
                ManageDraft(isDraft);
            //}

            return View(model);
        }

        [HttpPost]
        [CaptchaValidator]
        [ValidateAntiForgeryToken]
        protected void Edit(EditModel model)
        {
            bool isDraft = false;
            bool wasVisible = model.PageNameVisible;
            model.PageNameVisible = true;

            if (!wasVisible && Settings.GetAutoGeneratePageNames(CurrentWiki) && model.NameEnabled)
            {
                model.Name = new HtmlString(GenerateAutoName(model.PageTitle.ToString()));
            }

            model.Name = new HtmlString(model.Name.ToString().Trim());

            Page.Validate("nametitle");
            Page.Validate("captcha");
            if (!Page.IsValid)
            {
                if (!rfvTitle.IsValid || !rfvName.IsValid || !cvName1.IsValid || !cvName2.IsValid)
                {
                    model.PageNameVisible = true;
                    model.ManualNameVisible = false;
                }

                return;
            }

            model.PageNameVisible = wasVisible;

            // Check permissions
            if (currentPage == null)
            {
                // Check permissions for creating new pages
                if (!canCreateNewPages) UrlTools.Redirect("AccessDenied"); // TODO:
            }
            else
            {
                // Check permissions for editing current page
                if (!canEdit && !canEditWithApproval) UrlTools.Redirect("AccessDenied"); // TODO:
            }

            model.MinorChangeVisible = true;
            model.SaveAsDraftVisible = true;

            // Verify edit with approval
            if (!canEdit && canEditWithApproval)
            {
                model.SaveAsDraft = true;
            }

            // Check for scripts (Administrators can always add SCRIPT tags)
            if (!SessionFacade.GetCurrentGroupNames(CurrentWiki).Contains(Settings.GetAdministratorsGroup(CurrentWiki)) && !Settings.GetScriptTagsAllowed(CurrentWiki))
            {
                Regex r = new Regex(@"\<script.*?\>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (r.Match(editor.GetContent()).Success)
                {
                    lblResult.Text = @"<span style=""color: #FF0000;"">" + Messages.ScriptDetected + "</span>";
                    return;
                }
            }

            bool redirect = true;
            if (sender == btnSaveAndContinue) redirect = false;

            lblResult.Text = "";
            lblResult.CssClass = "";

            string username = "";
            if (SessionFacade.LoginKey == null) username = Request.UserHostAddress;
            else username = SessionFacade.CurrentUsername;

            IPagesStorageProviderV50 provider = FindAppropriateProvider();

            // Create list of selected categories
            List<CategoryInfo> categories = new List<CategoryInfo>();
            for (int i = 0; i < lstCategories.Items.Count; i++)
            {
                if (lstCategories.Items[i].Selected)
                {
                    CategoryInfo cat = Pages.FindCategory(CurrentWiki, lstCategories.Items[i].Value);

                    // Sanity check
                    if (cat.Provider == provider) categories.Add(cat);
                }
            }

            model.Comment = model.Comment.Trim();
            model.Description =model.Description.Trim();

            SaveMode saveMode = SaveMode.Backup;
            if (model.SaveAsDraft) saveMode = SaveMode.Draft;
            if (model.MinorChange) saveMode = SaveMode.Normal;

            if (model.NameEnabled)
            {
                // Find page, if inexistent create it
                Log.LogEntry("Page update requested for " + model.Name.ToString(), EntryType.General, username, CurrentWiki);

                string nspace = DetectNamespaceInfo() != null ? DetectNamespaceInfo().Name : null;

                PageContent pg = Pages.FindPage(NameTools.GetFullName(DetectNamespace(), model.Name.ToString()), provider);
                if (pg == null)
                {
                    saveMode = SaveMode.Normal;
                    pg = Pages.SetPageContent(CurrentWiki, nspace, model.Name.ToString(), provider, model.PageTitle.ToString(), username, DateTime.UtcNow, model.Comment, editor.GetContent(),
                    GetKeywords(), model.Description, saveMode);
                    attachmentManager.CurrentPage = pg;
                }
                else
                {
                    Pages.SetPageContent(CurrentWiki, nspace, model.Name.ToString(), provider, model.PageTitle.ToString(), username, DateTime.UtcNow, model.Comment, editor.GetContent(),
                        GetKeywords(), model.Description, saveMode);
                }
                // Save categories binding
                Pages.Rebind(pg, categories.ToArray());

                // If not a draft, remove page draft
                if (saveMode != SaveMode.Draft)
                {
                    Pages.DeleteDraft(pg.FullName, pg.Provider);
                    isDraft = false;
                }
                else isDraft = true;

                ManageDraft();

                lblResult.CssClass = "resultok";
                lblResult.Text = Messages.PageSaved;

                // This is a new page, so only who has page management permissions can execute this code
                // No notification must be sent for drafts awaiting approval
                if (redirect)
                {
                    Collisions.CancelEditingSession(pg, username);
                    string target = UrlTools.BuildUrl(CurrentWiki, Tools.UrlEncode(model.Name.ToString()), GlobalSettings.PageExtension, "?NoRedirect=1");
                    UrlTools.Redirect(target);
                }
                else
                {
                    // Disable PageName, because the name cannot be changed anymore
                    model.NameEnabled = false;
                    model.ManualNameVisible = false;
                }
            }
            else
            {
                // Used for redirecting to a specific section after editing it
                string anchor = "";

                if (currentPage == null) currentPage = Pages.FindPage(CurrentWiki, NameTools.GetFullName(DetectNamespace(), model.Name.ToString()));

                // Save data
                Log.LogEntry("Page update requested for " + currentPage.FullName, EntryType.General, username, CurrentWiki);
                if (!isDraft && currentSection != -1)
                {
                    StringBuilder sb = new StringBuilder(currentPage.Content.Length);
                    int start, len;
                    ExtractSection(currentPage.Content, currentSection, out start, out len, out anchor);
                    if (start > 0) sb.Append(currentPage.Content.Substring(0, start));
                    sb.Append(editor.GetContent());
                    if (start + len < currentPage.Content.Length - 1) sb.Append(currentPage.Content.Substring(start + len));
                    Pages.SetPageContent(currentPage.Provider.CurrentWiki, NameTools.GetNamespace(currentPage.FullName), NameTools.GetLocalName(currentPage.FullName), model.PageTitle.ToString(), username, DateTime.UtcNow, model.Comment, sb.ToString(),
                        GetKeywords(), model.Description, saveMode);
                }
                else
                {
                    Pages.SetPageContent(currentPage.Provider.CurrentWiki, NameTools.GetNamespace(currentPage.FullName), NameTools.GetLocalName(currentPage.FullName), model.PageTitle.ToString(), username, DateTime.UtcNow, model.Comment, editor.GetContent(),
                        GetKeywords(), model.Description, saveMode);
                }

                // Save Categories binding
                Pages.Rebind(currentPage, categories.ToArray());

                // If not a draft, remove page draft
                if (saveMode != SaveMode.Draft)
                {
                    Pages.DeleteDraft(currentPage.FullName, currentPage.Provider);
                    isDraft = false;
                }
                else isDraft = true;

                ManageDraft();

                lblResult.CssClass = "resultok";
                lblResult.Text = Messages.PageSaved;

                // This code is executed every time the page is saved, even when "Save & Continue" is clicked
                // This causes a draft approval notification to be sent multiple times for the same page,
                // but this is the only solution because the user might navigate away from the page after
                // clicking "Save & Continue" but not "Save" or "Cancel" - in other words, it is necessary
                // to take every chance to send a notification because no more chances might be available
                if (!canEdit && canEditWithApproval)
                {
                    Pages.SendEmailNotificationForDraft(currentPage.Provider.CurrentWiki, currentPage.FullName, model.PageTitle.ToString(), model.Comment, username);
                }

                if (redirect)
                {
                    Collisions.CancelEditingSession(currentPage, username);
                    string target = UrlTools.BuildUrl(CurrentWiki, Tools.UrlEncode(currentPage.FullName), GlobalSettings.PageExtension, "?NoRedirect=1",
                        (!string.IsNullOrEmpty(anchor) ? ("#" + anchor + "_" + currentSection.ToString()) : ""));
                    UrlTools.Redirect(target);
                }
            }
        }

        /// <summary>
        /// Manages the draft status display.
        /// </summary>
        private void ManageDraft(EditModel model, bool isDraft)
        {
            if (isDraft)
            {
                model.SaveAsDraft = true;
                model.MinorChangeEnabled = false;
                pnlDraft.Visible = true;
                lblDraftInfo.Text = lblDraftInfo.Text.Replace("##USER##",
                    Users.UserLink(CurrentWiki, currentPage.User, true)).Replace("##DATETIME##",
                    Preferences.AlignWithTimezone(CurrentWiki, currentPage.LastModified).ToString(Settings.GetDateTimeFormat(CurrentWiki))).Replace("##VIEWCHANGES##",
                    string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", UrlTools.BuildUrl(CurrentWiki, "Diff?Page=",
                    Tools.UrlEncode(currentPage.FullName), "&amp;Rev1=Current&amp;Rev2=Draft"),
                    Messages.ViewChanges));
            }
            else
            {
                pnlDraft.Visible = false;
            }
        }

        /// <summary>
        /// Detects the permissions for the current user.
        /// </summary>
        /// <remarks><b>currentPage</b> should be set before calling this method.</remarks>
        private Permissions DetectPermissions(PageContent currentPage)
        {
            string currentUser = SessionFacade.GetCurrentUsername();
            string[] currentGroups = SessionFacade.GetCurrentGroupNames(CurrentWiki);

            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));

            var permissions = new Permissions();

            if (currentPage != null)
            {
                bool canEdit;
                bool canEditWithApproval;
                Pages.CanEditPage(currentPage.Provider.CurrentWiki, currentPage.FullName, currentUser, currentGroups, out canEdit, out canEditWithApproval);
                permissions.CanEdit = canEdit;
                permissions.CanEditWithApproval = canEditWithApproval;
                permissions.CanCreateNewPages = false; // Least privilege
                permissions.CanCreateNewCategories = authChecker.CheckActionForNamespace(Pages.FindNamespace(CurrentWiki, NameTools.GetNamespace(currentPage.FullName)),
                    Actions.ForNamespaces.ManageCategories, currentUser, currentGroups);
                permissions.CanManagePageCategories = authChecker.CheckActionForPage(currentPage.FullName, Actions.ForPages.ManageCategories, currentUser, currentGroups);
                permissions.CanDownloadAttachments = authChecker.CheckActionForPage(currentPage.FullName, Actions.ForPages.DownloadAttachments, currentUser, currentGroups);
            }
            else
            {
                NamespaceInfo ns = DetectNamespaceInfo();
                permissions.CanCreateNewPages = authChecker.CheckActionForNamespace(ns, Actions.ForNamespaces.CreatePages, currentUser, currentGroups);
                permissions.CanCreateNewCategories = authChecker.CheckActionForNamespace(ns, Actions.ForNamespaces.ManageCategories, currentUser, currentGroups);
                permissions.CanManagePageCategories = permissions.CanCreateNewCategories;
                permissions.CanDownloadAttachments = authChecker.CheckActionForNamespace(ns, Actions.ForNamespaces.DownloadAttachments, currentUser, currentGroups);
            }

            return permissions;
        }

        private class Permissions
        {
            public bool CanEdit { get; set; } = false;
            public bool CanEditWithApproval { get; set; } = false;
            public bool CanCreateNewPages { get; set; } = false;
            public bool CanCreateNewCategories { get; set; } = false;
            public bool CanManagePageCategories { get; set; } = false;
            public bool CanDownloadAttachments { get; set; } = false;
        }

        /// <summary>
        /// Populates the categories for the current namespace and provider, selecting the ones specified.
        /// </summary>
        /// <param name="toSelect">The categories to select.</param>
        /// <param name="currentPage"></param>
        private void PopulateCategories(CategoryInfo[] toSelect, PageContent currentPage)
        {
            IPagesStorageProviderV60 provider = FindAppropriateProvider(currentPage);
            List<CategoryInfo> cats = Pages.GetCategories(CurrentWiki, DetectNamespaceInfo());
            lstCategories.Items.Clear();
            foreach (CategoryInfo c in cats)
            {
                if (c.Provider == provider)
                {
                    ListItem itm = new ListItem(NameTools.GetLocalName(c.FullName), c.FullName);
                    if (Array.Find<CategoryInfo>(toSelect, delegate (CategoryInfo s) { return s.FullName == c.FullName; }) != null)
                        itm.Selected = true;
                    lstCategories.Items.Add(itm);
                }
            }
        }

        /// <summary>
        /// Finds the appropriate provider to use for operations.
        /// </summary>
        /// <returns>The provider.</returns>
        private IPagesStorageProviderV60 FindAppropriateProvider(PageContent currentPage)
        {
            IPagesStorageProviderV60 provider = null;

            if (currentPage != null) provider = currentPage.Provider;
            else
            {
                NamespaceInfo currentNamespace = DetectNamespaceInfo();
                provider =
                    currentNamespace == null ?
                    Collectors.CollectorsBox.PagesProviderCollector.GetProvider(GlobalSettings.DefaultPagesProvider, CurrentWiki) :
                    currentNamespace.Provider;
            }

            return provider;
        }

        // This regex is duplicated from Formatter.cs
        private static readonly Regex FullCodeRegex = new Regex(@"@@.+?@@", RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Finds the start and end positions of a section of the content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="section">The section ID.</param>
        /// <param name="start">The index of the first character of the section.</param>
        /// <param name="len">The length of the section.</param>
        /// <param name="anchor">The anchor ID of the section.</param>
        private static void ExtractSection(string content, int section, out int start, out int len, out string anchor)
        {
            // HACK: @@...@@ escapes headers: must reproduce behavior here
            Match m = FullCodeRegex.Match(content);
            while (m.Success)
            {
                string newContent = m.Value.Replace("=", "$"); // Do not alter positions
                content = content.Remove(m.Index, m.Length);
                content = content.Insert(m.Index, newContent);
                m = FullCodeRegex.Match(content, m.Index + m.Length - 1);
            }

            List<HPosition> hPos = Formatter.DetectHeaders(content);
            start = 0;
            len = content.Length;
            anchor = "";
            int level = -1;
            bool found = false;
            for (int i = 0; i < hPos.Count; i++)
            {
                if (hPos[i].ID == section)
                {
                    start = hPos[i].Index;
                    len = len - start;
                    level = hPos[i].Level; // Level is used to edit the current section AND all the subsections
                                           // Set the anchor value so that it's possible to redirect the user to the just edited section
                    anchor = Formatter.BuildHAnchor(hPos[i].Text);
                    found = true;
                    break;
                }
            }
            if (found)
            {
                int diff = len;
                for (int i = 0; i < hPos.Count; i++)
                {
                    if (hPos[i].Index > start && // Next section (Hx)
                        hPos[i].Index - start < diff && // The nearest section
                        hPos[i].Level <= level)
                    { // Of the same level or higher
                        len = hPos[i].Index - start - 1;
                        diff = hPos[i].Index - start;
                    }
                }
            }
        }

        /// <summary>
        /// Sets a set of keywords in the appropriate textbox.
        /// </summary>
        /// <param name="keywords">The keywords.</param>
        private string SetKeywords(string[] keywords)
        {
            if (keywords == null || keywords.Length == 0) return "";

            StringBuilder sb = new StringBuilder(50);
            for (int i = 0; i < keywords.Length; i++)
            {
                sb.Append(keywords[i]);
                if (i != keywords.Length - 1) sb.Append(",");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Loads a content template when the query strings specifies it.
        /// </summary>
        /// <returns>The content of the selected template.</returns>
        private string LoadTemplateIfAppropriate(EditModel model)
        {
            if (string.IsNullOrEmpty(Request["Template"])) return "";
            ContentTemplate template = Templates.Find(CurrentWiki, Request["Template"]);
            if (template == null) return "";
            else
            {
                model.AutoTemplateLabel = new HtmlString(Localization.Common.Edit.LblAutoTemplate_Text.Replace("##TEMPLATE##", template.Name));
                model.AutoTemplateVisible = true;
                return template.Content;
            }
        }

        /// <summary>
        /// Generates an automatic page name.
        /// </summary>
        /// <param name="title">The page title.</param>
        /// <returns>The name.</returns>
        private static string GenerateAutoName(string title)
        {
            // Replace all non-alphanumeric characters with dashes
            if (title.Length == 0) return "";

            StringBuilder buffer = new StringBuilder(title.Length);

            foreach (char ch in title.Normalize(NormalizationForm.FormD).Replace("\"", "").Replace("'", ""))
            {
                var unicat = char.GetUnicodeCategory(ch);
                if (unicat == System.Globalization.UnicodeCategory.LowercaseLetter ||
                    unicat == System.Globalization.UnicodeCategory.UppercaseLetter ||
                    unicat == System.Globalization.UnicodeCategory.DecimalDigitNumber)
                {
                    buffer.Append(ch);
                }
                else if (unicat != System.Globalization.UnicodeCategory.NonSpacingMark) buffer.Append("-");
            }

            while (buffer.ToString().IndexOf("--") >= 0)
            {
                buffer.Replace("--", "-");
            }

            return buffer.ToString().Trim('-');
        }

        /// <summary>
        /// Verifies for editing collisions, and if no collision is found, "locks" the page
        /// </summary>
        private void ManageEditingCollisions(PageContent currentPage, int currentSection)
        {
            if (currentPage == null) return;

            lblRefreshLink.Text = @"<a href=""" +
                UrlTools.BuildUrl(CurrentWiki, "Edit.aspx?Page=", Tools.UrlEncode(currentPage.FullName), (Request["Section"] != null ? "&amp;Section=" + currentSection.ToString() : "")) +
                @""">" + Messages.Refresh + " &raquo;</a>";

            string username = Request.UserHostAddress;
            if (SessionFacade.LoginKey != null) username = SessionFacade.CurrentUsername;

            if (Collisions.IsPageBeingEdited(currentPage, username))
            {
                pnlCollisions.Visible = true;
                lblConcurrentEditingUsername.Text = "(" + Users.UserLink(CurrentWiki, Collisions.WhosEditing(currentPage)) + ")";
                if (Settings.GetDisableConcurrentEditing(CurrentWiki))
                {
                    lblSaveDisabled.Visible = true;
                    lblSaveDangerous.Visible = false;
                    btnSave.Enabled = false;
                    btnSaveAndContinue.Enabled = false;
                }
                else
                {
                    lblSaveDisabled.Visible = false;
                    lblSaveDangerous.Visible = true;
                    btnSave.Enabled = true;
                    btnSaveAndContinue.Enabled = true;
                }
            }
            else
            {
                pnlCollisions.Visible = false;
                btnSave.Enabled = true;
                btnSaveAndContinue.Enabled = true;
                Collisions.RenewEditingSession(currentPage, username);
            }
        }

        /// <summary>
        /// Manages the display of the template selection controls.
        /// </summary>
        private void ManageTemplatesDisplay()
        {
            // Hide templates selection if there aren't any or if the editor is not in WikiMarkup mode
            if (Templates.GetTemplates(CurrentWiki).Count == 0 || !editor.IsInWikiMarkup())
            {
                btnTemplates.Visible = false;
                pnlTemplates.Visible = false;
            }
            else btnTemplates.Visible = true;
        }


    }
}