using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Localization.Messages;

namespace ScrewTurn.Wiki.Web.Code
{
    /// <summary>
    /// Represents a page revision for display purposes.
    /// </summary>
    public class RevisionRow
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RevisionRow" /> class.
        /// </summary>
        /// <param name="revision">The revision (<b>-1</b> for current).</param>
        /// <param name="content">The original page content.</param>
        /// <param name="canRollback">A value indicating whether the current user can rollback the page.</param>
        public RevisionRow(int revision, PageContent content, bool canRollback)
        {
            string currentWiki = Tools.DetectCurrentWiki();

            Revision = revision == -1 ? Messages.Current : revision.ToString();
            Title = FormattingPipeline.PrepareTitle(currentWiki, content.Title, false, FormattingContext.PageContent, content.FullName);
            SavedOn = Preferences.AlignWithTimezone(currentWiki, content.LastModified).ToString(Settings.GetDateTimeFormat(currentWiki));
            SavedBy = new MvcHtmlString(Users.UserLink(currentWiki, content.User));
            Comment = content.Comment;
            CanRollback = canRollback;
            UrlToRevision =
                new MvcHtmlString(UrlTools.BuildUrl(currentWiki, Tools.UrlEncode(content.FullName), "/History?Revision=",
                    Revision));
        }

        /// <summary>
        /// Gets the revision.
        /// </summary>
        public string Revision { get; private set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the save date/time.
        /// </summary>
        public string SavedOn { get; private set; }

        /// <summary>
        /// Gets the revision author.
        /// </summary>
        [AllowHtml]
        public MvcHtmlString SavedBy { get; private set; }

        /// <summary>
        /// Gets the comment.
        /// </summary>
        public string Comment { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current user can rollback the page.
        /// </summary>
        public bool CanRollback { get; private set; }

        /// <summary>
        /// Link to revision
        /// </summary>
        [AllowHtml]
        public MvcHtmlString UrlToRevision { get; private set; }

    }
}