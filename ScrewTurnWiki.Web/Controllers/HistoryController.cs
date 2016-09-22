using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Code.Attributes;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models.Wiki;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class HistoryController : PageController
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public HistoryController(ApplicationSettings settings) : base(settings)
        {
        }

        [HttpGet]
        [CheckExistPage(PageParamName = "page")]
        [CheckActionForPageAttribute(Action = CheckActionForPageAttribute.ActionForPages.ReadPage, Order = 2)]
        [CheckActionForPageAttribute(Action = CheckActionForPageAttribute.ActionForPages.ManagePage, Order = 3)]
        public ActionResult Rollback(string page, int revision)
        {
            Log.LogEntry("Page rollback requested for " + CurrentPage.FullName + " to rev. " + revision.ToString(), EntryType.General, SessionFacade.GetCurrentUsername(), CurrentWiki);
            Pages.Rollback(CurrentPage, revision);

            var model = GetModel(revision, null, null);

            return View("History", model);
        }

        [HttpGet]
        [CheckExistPage(PageParamName = "page", Order = 1)]
        [CheckActionForPageAttribute(Action = CheckActionForPageAttribute.ActionForPages.ReadPage, Order = 2)]
        public ActionResult GetHistory(string page, int? revision, string rev1, string rev2)
        {
            var model = GetModel(revision, rev1, rev2);
            return View("History", model);
        }


        private HistoryModel GetModel(int? revision, string rev1, string rev2)
        {
            var model = new HistoryModel();
            base.PrepareDefaultModel(model, CurrentNamespace, CurrentPageFullName);
            model.PageFullNameEncode = Tools.UrlEncode(CurrentPage.FullName);

            model.Title = Messages.HistoryTitle + " - " + Settings.GetWikiTitle(CurrentWiki);

            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            var canRollback = authChecker.CheckActionForPage(CurrentPage.FullName, Actions.ForPages.ManagePage,
                    SessionFacade.GetCurrentUsername(), SessionFacade.GetCurrentGroupNames(CurrentWiki));

            model.LblTitle = new MvcHtmlString(Messages.PageHistory + ": " +
                             FormattingPipeline.PrepareTitle(CurrentWiki, CurrentPage.Title, false,
                                 FormattingContext.PageContent, CurrentPage.FullName));

            //string rev1 =  Request["Rev1"]
            //string rev2 =  Request["Rev2"]

            List<int> revisions = Pages.GetBackups(CurrentPage);
            revisions.Reverse();
            // Populate dropdown lists
            var rev1List = new List<SelectListItem>();
            var rev2List = new List<SelectListItem>();
            rev2List.Add(new SelectListItem()
            {
                Text = Messages.Current,
                Value = "Current",
                Selected = rev2 != null && rev2.Equals("Current")
            });

            for (int i = 0; i < revisions.Count; i++)
            {
                rev1List.Add(new SelectListItem()
                {
                    Text = revisions[i].ToString(),
                    Value = revisions[i].ToString(),
                    Selected = rev1 != null && rev1.Equals(revisions[i].ToString())
                });
                rev2List.Add(new SelectListItem()
                {
                    Text = revisions[i].ToString(),
                    Value = revisions[i].ToString(),
                    Selected = rev2 != null && rev2.Equals(revisions[i].ToString())
                });
            }

            model.Rev1List = rev1List;
            model.Rev2List = rev2List;

            if (revisions.Count == 0) model.BtnCompareEnabled = false;


            PrintHistory(model, canRollback, revision);

            return model;
        }

        /// <summary>
        /// Prints the history.
        /// </summary>
        private void PrintHistory(HistoryModel model, bool canRollback, int? revision)
        {
            StringBuilder sb = new StringBuilder();

            if (revision == null)
            {
                // Show version list
                List<int> revisions = Pages.GetBackups(CurrentPage);
                revisions.Reverse();

                List<RevisionRow> result = new List<RevisionRow>(revisions.Count + 1);

                result.Add(new RevisionRow(-1, CurrentPage, false));

                foreach (int rev in revisions)
                {
                    PageContent content = Pages.GetBackupContent(CurrentPage, rev);

                    result.Add(new RevisionRow(rev, content, canRollback));
                }

                model.RevisionRows = result;
            }
            else
            {
                int rev = (int)revision; //- 1;
                //if (!int.TryParse(Request["Revision"], out rev)) UrlTools.Redirect(CurrentPage.FullName + GlobalSettings.PageExtension);

                List<int> backups = Pages.GetBackups(CurrentPage);
                if (!backups.Contains(rev))
                {
                    UrlTools.Redirect(CurrentPage.FullName + GlobalSettings.PageExtension);
                    return;
                }
                PageContent revisionPage = Pages.GetBackupContent(CurrentPage, rev);
                sb.Append(@"<table class=""box"" cellpadding=""0"" cellspacing=""0""><tr><td>");
                sb.Append(@"<p style=""text-align: center;""><b>");
                if (rev > 0)
                {
                    sb.Append(@"<a href=""");
                    UrlTools.BuildUrl(CurrentWiki, sb, Tools.UrlEncode(CurrentPage.FullName), "/History?Revision=", Tools.GetVersionString((int)(rev - 1)));

                    sb.Append(@""">&laquo; ");
                    sb.Append(Messages.OlderRevision);
                    sb.Append("</a>");
                }
                else
                {
                    sb.Append("&laquo; ");
                    sb.Append(Messages.OlderRevision);
                }

                sb.Append(@" - <a href=""");
                UrlTools.BuildUrl(CurrentWiki, sb, Tools.UrlEncode(CurrentPage.FullName), "/History");
                sb.Append(@""">");
                sb.Append(Messages.BackToHistory);
                sb.Append("</a> - ");

                if (rev < backups.Count - 1)
                {
                    sb.Append(@"<a href=""");
                    UrlTools.BuildUrl(CurrentWiki, sb, Tools.UrlEncode(CurrentPage.FullName), "/History?Revision=", Tools.GetVersionString((int)(rev + 1)));

                    sb.Append(@""">");
                    sb.Append(Messages.NewerRevision);
                    sb.Append(" &raquo;</a>");
                }
                else
                {
                    sb.Append(@"<a href=""");
                    UrlTools.BuildUrl(CurrentWiki, sb, Tools.UrlEncode(CurrentPage.FullName), GlobalSettings.PageExtension);
                    sb.Append(@""">");
                    sb.Append(Messages.CurrentRevision);
                    sb.Append("</a>");
                }
                sb.Append("</b></p></td></tr></table><br />");

                sb.Append(@"<h3 class=""separator"">");
                sb.Append(Messages.PageRevision);
                sb.Append(": ");
                sb.Append(Preferences.AlignWithTimezone(CurrentWiki, revisionPage.LastModified).ToString(Settings.GetDateTimeFormat(CurrentWiki)));
                sb.Append("</h3><br />");

                sb.Append(FormattingPipeline.FormatWithPhase3(CurrentWiki, FormattingPipeline.FormatWithPhase1And2(CurrentWiki, revisionPage.Content,
                    false, FormattingContext.PageContent, CurrentPage.FullName).Replace(Formatter.EditSectionPlaceHolder, ""), FormattingContext.PageContent, CurrentPage.FullName));
            }

            model.LblHistory = new MvcHtmlString(sb.ToString());
        }

        [HttpGet]
        [CheckExistPage(PageParamName = "page", Order = 1)]
        [CheckActionForPageAttribute(Action = CheckActionForPageAttribute.ActionForPages.ReadPage, Order = 2)]
        public ActionResult Diff(string page, string rev1, string rev2)
        {
            var model = new DiffModel();
            base.PrepareDefaultModel(model, CurrentNamespace, CurrentPageFullName);
            model.PageFullNameEncode = Tools.UrlEncode(CurrentPage.FullName);

            model.Title = Messages.DiffTitle + " - " + Settings.GetWikiTitle(CurrentWiki);

            if (page == null || rev1 == null || rev2 == null)
            {
                return RedirectToHome();
            }

            StringBuilder sb = new StringBuilder();

            //PageContent pageContent = Pages.FindPage(CurrentWiki, page);
            //if (pageContent == null)
            //{
            //    Redirect();
            //    return;
            //}


            int revision1 = -1;
            int revision2 = -1;
            string rev1Text = "";
            string rev2Text = "";

            PageContent rev1Content = null;
            PageContent rev2Content = null;
            bool draft = false;

            // Load rev1 content
            if (int.TryParse(rev1, out revision1))
            {
                rev1Content = Pages.GetBackupContent(CurrentPage, revision1);
                rev1Text = revision1.ToString();
                if (revision1 >= 0 && rev1Content == null && Pages.GetBackupContent(CurrentPage, revision1 - 1) != null) rev1Content = CurrentPage;
                if (rev1Content == null) return RedirectToHome();
            }
            else
            {
                // Look for current
                if (rev1.ToLowerInvariant() == "current")
                {
                    rev1Content = CurrentPage;
                    rev1Text = Messages.Current;
                }
                else return RedirectToHome();
            }

            if (int.TryParse(rev2, out revision2))
            {
                rev2Content = Pages.GetBackupContent(CurrentPage, revision2);
                rev2Text = revision2.ToString();
                if (revision2 >= 0 && rev2Content == null && Pages.GetBackupContent(CurrentPage, revision2 - 1) != null) rev2Content = CurrentPage;
                if (rev2Content == null) return RedirectToHome();
            }
            else
            {
                // Look for current or draft
                if (rev2.ToLowerInvariant() == "current")
                {
                    rev2Content = CurrentPage;
                    rev2Text = Messages.Current;
                }
                else if (rev2.ToLowerInvariant() == "draft")
                {
                    rev2Content = Pages.GetDraft(CurrentPage);
                    rev2Text = Messages.Draft;
                    draft = true;
                    if (rev2Content == null) return RedirectToHome();
                }
                else return RedirectToHome();
            }

            model.LblTitle = new MvcHtmlString(Messages.DiffingPageTitle.Replace("##PAGETITLE##",
                FormattingPipeline.PrepareTitle(CurrentWiki, CurrentPage.Title, false, FormattingContext.PageContent,
                    CurrentPage.FullName)).Replace("##REV1##", rev1Text).Replace("##REV2##", rev2Text));

            if (!draft)
            {
                model.LblBack = new MvcHtmlString(string.Format(@"<a href=""{0}"">&laquo; {1}</a>",
                    UrlTools.BuildUrl(CurrentWiki, Tools.UrlEncode(page), "/History?Rev1=",
                        rev1, "&amp;Rev2=", rev2),
                    Messages.Back));
            }

            sb.Append(Messages.DiffColorKey);
            sb.Append("<br /><br />");

            string result = DiffTools.DiffRevisions(rev1Content.Content, rev2Content.Content);

            sb.Append(result);

            model.LblDiff = new MvcHtmlString(sb.ToString());

            return View("Diff", model);
        }

        private RedirectToRouteResult RedirectToHome()
        {
            return RedirectToAction("Page", "Wiki", new
            {
                page = Settings.GetDefaultPage(CurrentWiki)
            });
            //UrlTools.RedirectHome(CurrentWiki);
        }

    }
}