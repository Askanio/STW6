using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Localization.Messages;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class RssController : PageController
    {
        public RssController(ApplicationSettings settings) : base(settings)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            RssFeedsMode rssFeedsMode = Settings.GetRssFeedsMode(CurrentWiki);
            if (rssFeedsMode == RssFeedsMode.Disabled)
                return new HttpNotFoundResult();

            string currentUsername = SessionFacade.GetCurrentUsername();
            string[] currentGroups = SessionFacade.GetCurrentGroupNames(CurrentWiki);

            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));

            if (SessionFacade.LoginKey == null)
            {
                // Look for username/password in the query string
                if (Request["Username"] != null && Request["Password"] != null)
                {
                    // Try to authenticate
                    UserInfo u = Users.FindUser(CurrentWiki, Request["Username"]);
                    if (u != null)
                    {
                        // Very "dirty" way - pages should not access Providers
                        if (u.Provider.TestAccount(u, Request["Password"]))
                        {
                            // Valid account
                            currentUsername = Request["Username"];
                            currentGroups = Users.FindUser(CurrentWiki, currentUsername).Groups;
                        }
                    }
                    else
                    {
                        // Check for built-in admin account
                        if (Request["Username"].Equals("admin") && Request["Password"].Equals(GlobalSettings.GetMasterPassword()))
                        {
                            currentUsername = "admin";
                            currentGroups = new string[] { Settings.GetAdministratorsGroup(CurrentWiki) };
                        }
                    }
                }
            }
            
            Response.ClearContent();
            Response.ContentType = "text/xml;charset=UTF-8";
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;

            if (Request["Page"] != null)
            {
                PageContent page = Pages.FindPage(CurrentWiki, Request["Page"]);
                if (page == null) // return;
                    return new HttpNotFoundResult();

                if (Request["Discuss"] == null)
                {
                    // Check permission for the page
                    bool canReadPage = authChecker.CheckActionForPage(page.FullName, Actions.ForPages.ReadPage, currentUsername, currentGroups);
                    if (!canReadPage)
                    {
                        //Response.StatusCode = 401;
                        return new HttpStatusCodeResult(401);
                    }

                    // Start an XML writer for the output stream
                    using (XmlWriter rss = XmlWriter.Create(Response.OutputStream))
                    {
                        // Build an RSS header
                        BuildRssHeader(rss);

                        // Build the channel element
                        BuildChannelHead(rss, Settings.GetWikiTitle(CurrentWiki) + " - " + Formatter.StripHtml(FormattingPipeline.PrepareTitle(CurrentWiki, page.Title, false, FormattingContext.PageContent, page.FullName)),
                            Settings.GetMainUrl(CurrentWiki) + page.FullName + GlobalSettings.PageExtension,
                            Settings.GetMainUrl(CurrentWiki) + UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, "RSS?Page=", page.FullName),
                            Formatter.StripHtml(page.Title) + " - " + Messages.PageUpdates);

                        // Write the item element
                        rss.WriteStartElement("item");
                        rss.WriteStartElement("title");
                        rss.WriteCData(Formatter.StripHtml(FormattingPipeline.PrepareTitle(CurrentWiki, page.Title, false, FormattingContext.PageContent, page.FullName)));
                        rss.WriteEndElement();
                        rss.WriteElementString("link", Settings.GetMainUrl(CurrentWiki) + page.FullName + GlobalSettings.PageExtension);

                        UserInfo user = Users.FindUser(CurrentWiki, page.User);
                        string username = user != null ? Users.GetDisplayName(user) : page.User;

                        // Create the description tag
                        rss.WriteStartElement("description");
                        if (rssFeedsMode == RssFeedsMode.Summary)
                        {
                            rss.WriteCData(Formatter.StripHtml(page.Title) + ": " + Messages.ThePageHasBeenUpdatedBy + " " +
                                page.User + (page.Comment.Length > 0 ? ".<br />" + page.Comment : "."));
                        }
                        else
                        {
                            rss.WriteCData(FormattedContent.GetFormattedPageContent(CurrentWiki, page));
                        }
                        rss.WriteEndElement();

                        // Write the remaining elements
                        rss.WriteElementString("author", username);
                        rss.WriteElementString("pubDate", page.LastModified.ToUniversalTime().ToString("R"));
                        rss.WriteStartElement("guid");
                        rss.WriteAttributeString("isPermaLink", "false");
                        rss.WriteString(GetGuid(page.FullName, page.LastModified));
                        rss.WriteEndElement();

                        // Complete the item element
                        CompleteCurrentElement(rss);

                        // Complete the channel element
                        CompleteCurrentElement(rss);

                        // Complete the rss element
                        CompleteCurrentElement(rss);

                        // Finish off
                        rss.Flush();
                        rss.Close();
                    }
                }
                else
                {
                    // Check permission for the discussion
                    bool canReadDiscussion = authChecker.CheckActionForPage(page.FullName, Actions.ForPages.ReadDiscussion, currentUsername, currentGroups);
                    if (!canReadDiscussion)
                    {
                        //Response.StatusCode = 401;
                        return new HttpStatusCodeResult(401);
                    }

                    List<Message> messages = new List<Message>(Pages.GetPageMessages(page));
                    // Un-tree Messages
                    messages = UnTreeMessages(messages);
                    // Sort from newer to older
                    messages.Sort(new MessageDateTimeComparer(true));

                    // Start an XML writer for the output stream
                    using (XmlWriter rss = XmlWriter.Create(Response.OutputStream))
                    {
                        // Build an RSS header
                        BuildRssHeader(rss);

                        // Build the channel element
                        BuildChannelHead(rss, Settings.GetWikiTitle(CurrentWiki) + " - " + Formatter.StripHtml(FormattingPipeline.PrepareTitle(CurrentWiki, page.Title, false, FormattingContext.PageContent, page.FullName)) + " - Discussion Updates",
                            Settings.GetMainUrl(CurrentWiki) + page.FullName + GlobalSettings.PageExtension + "?Discuss=1",
                            Settings.GetMainUrl(CurrentWiki) + UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, "RSS?Page=", page.FullName, "&Discuss=1"),
                            Settings.GetWikiTitle(CurrentWiki) + " - " + Formatter.StripHtml(FormattingPipeline.PrepareTitle(CurrentWiki, page.Title, false, FormattingContext.PageContent, page.FullName)) + " - Discussion Updates");

                        for (int i = 0; i < messages.Count; i++)
                        {
                            // Write the item element
                            rss.WriteStartElement("item");
                            rss.WriteStartElement("title");
                            rss.WriteCData(Formatter.StripHtml(FormattingPipeline.PrepareTitle(CurrentWiki, messages[i].Subject, false, FormattingContext.MessageBody, page.FullName)));
                            rss.WriteEndElement();
                            rss.WriteElementString("link", Settings.GetMainUrl(CurrentWiki) + page.FullName + GlobalSettings.PageExtension + "?Discuss=1");

                            UserInfo user = Users.FindUser(CurrentWiki, messages[i].Username);
                            string username = user != null ? Users.GetDisplayName(user) : messages[i].Username;

                            // Create the description tag
                            rss.WriteStartElement("description");
                            if (rssFeedsMode == RssFeedsMode.Summary)
                            {
                                rss.WriteCData(Messages.AMessageHasBeenPostedBy.Replace("##SUBJECT##", messages[i].Subject) + " " + username + ".");
                            }
                            else
                            {
                                rss.WriteCData(FormattingPipeline.FormatWithPhase3(CurrentWiki, FormattingPipeline.FormatWithPhase1And2(CurrentWiki, messages[i].Body, false, FormattingContext.MessageBody, page.FullName), FormattingContext.MessageBody, page.FullName));
                            }
                            rss.WriteEndElement();

                            // Write the remaining elements
                            rss.WriteElementString("author", username);
                            rss.WriteElementString("pubDate", messages[i].DateTime.ToUniversalTime().ToString("R"));
                            rss.WriteStartElement("guid");
                            rss.WriteAttributeString("isPermaLink", "false");
                            rss.WriteString(GetGuid(page.FullName + "-" + messages[i].ID.ToString(), messages[i].DateTime));
                            rss.WriteEndElement();

                            // Complete the item element
                            CompleteCurrentElement(rss);
                        }

                        // Complete the channel element
                        CompleteCurrentElement(rss);

                        // Complete the rss element
                        CompleteCurrentElement(rss);

                        // Finish off
                        rss.Flush();
                        rss.Close();
                    }
                }
            }
            else
            {
                if (Request["Discuss"] == null)
                {
                    // All page updates

                    // Start an XML writer for the output stream
                    using (XmlWriter rss = XmlWriter.Create(Response.OutputStream))
                    {

                        // Build an RSS header
                        BuildRssHeader(rss);

                        bool useCat = false;
                        string cat = "";
                        if (Request["Category"] != null)
                        {
                            useCat = true;
                            cat = Request["Category"];
                        }

                        // Build the channel element
                        BuildChannelHead(rss, Settings.GetWikiTitle(CurrentWiki) + " - " + Messages.PageUpdates,
                            Settings.GetMainUrl(CurrentWiki),
                            Settings.GetMainUrl(CurrentWiki) + UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, "RSS", (useCat ? ("?Category=" + cat) : "")),
                            Messages.RecentPageUpdates);

                        RecentChange[] ch = RecentChanges.GetAllChanges(CurrentWiki);
                        Array.Reverse(ch);
                        for (int i = 0; i < ch.Length; i++)
                        {

                            // Suppress this entry if we've already reported this page (so we don't create duplicate entries in the feed page)
                            bool duplicateFound = false;
                            for (int j = 0; j < i; j++)
                            {
                                if (ch[j].Page == ch[i].Page)
                                {
                                    duplicateFound = true;
                                    break;
                                }
                            }
                            if (duplicateFound) continue;

                            // Skip message-related entries
                            if (!IsPageChange(ch[i].Change)) continue;

                            PageContent p = Pages.FindPage(CurrentWiki, ch[i].Page);
                            if (p != null)
                            {
                                // Check permissions for every page
                                bool canReadThisPage = authChecker.CheckActionForPage(p.FullName, Actions.ForPages.ReadPage, currentUsername, currentGroups);
                                if (!canReadThisPage) continue;

                                if (useCat)
                                {
                                    CategoryInfo[] infos = Pages.GetCategoriesForPage(p);
                                    if (infos.Length == 0 && cat != "-") continue;
                                    else if (infos.Length != 0)
                                    {
                                        bool found = false;
                                        for (int k = 0; k < infos.Length; k++)
                                        {
                                            if (infos[k].FullName == cat)
                                            {
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (!found) continue;
                                    }
                                }
                            }

                            // Check namespace
                            if (p != null && NameTools.GetNamespace(p.FullName) != CurrentNamespace) continue;

                            // Skip deleted and renamed pages as their category binding is unknown
                            if (p == null) continue; // && useCat) continue;

                            // Write the item element
                            rss.WriteStartElement("item");
                            rss.WriteStartElement("title");
                            rss.WriteCData(Formatter.StripHtml(FormattingPipeline.PrepareTitle(CurrentWiki, ch[i].Title, false, FormattingContext.PageContent, p.FullName)));
                            rss.WriteEndElement();
                            if (ch[i].Change != Change.PageDeleted && p != null)
                                if (ch[i].Change != Change.PageDeleted && p != null)
                                {
                                    rss.WriteElementString("link", Settings.GetMainUrl(CurrentWiki) + ch[i].Page + GlobalSettings.PageExtension);
                                }
                                else rss.WriteElementString("link", Settings.GetMainUrl(CurrentWiki));

                            UserInfo user = Users.FindUser(CurrentWiki, ch[i].User);
                            string username = user != null ? Users.GetDisplayName(user) : ch[i].User;

                            rss.WriteElementString("author", username);

                            // Create the description tag
                            StringBuilder sb = new StringBuilder();
                            if (rssFeedsMode == RssFeedsMode.Summary || p == null)
                            {
                                switch (ch[i].Change)
                                {
                                    case Change.PageUpdated:
                                        sb.Append(Messages.ThePageHasBeenUpdatedBy);
                                        break;
                                    case Change.PageDeleted:
                                        sb.Append(Messages.ThePageHasBeenDeletedBy);
                                        break;
                                    case Change.PageRenamed:
                                        sb.Append(Messages.ThePageHasBeenRenamedBy);
                                        break;
                                    case Change.PageRolledBack:
                                        sb.Append(Messages.ThePageHasBeenRolledBackBy);
                                        break;
                                }
                                sb.Append(" " + username + (ch[i].Description.Length > 0 ? ".<br />" + ch[i].Description : "."));
                            }
                            else
                            {
                                // p != null
                                sb.Append(FormattedContent.GetFormattedPageContent(CurrentWiki, p));
                            }
                            rss.WriteStartElement("description");
                            rss.WriteCData(sb.ToString());
                            rss.WriteEndElement();

                            // Write the remaining elements
                            rss.WriteElementString("pubDate", ch[i].DateTime.ToUniversalTime().ToString("R"));
                            rss.WriteStartElement("guid");
                            rss.WriteAttributeString("isPermaLink", "false");
                            rss.WriteString(GetGuid(ch[i].Page, ch[i].DateTime));
                            rss.WriteEndElement();

                            // Complete the item element
                            rss.WriteEndElement();
                        }

                        // Complete the channel element
                        CompleteCurrentElement(rss);

                        // Complete the rss element
                        CompleteCurrentElement(rss);

                        // Finish off
                        rss.Flush();
                        rss.Close();
                    }
                }
                else
                {
                    // All discussion updates

                    // Start an XML writer for the output stream
                    using (XmlWriter rss = XmlWriter.Create(Response.OutputStream))
                    {

                        // Build an RSS header
                        BuildRssHeader(rss);

                        bool useCat = false;
                        string cat = "";
                        if (Request["Category"] != null)
                        {
                            useCat = true;
                            cat = Request["Category"];
                        }

                        // Build the channel element
                        BuildChannelHead(rss, Settings.GetWikiTitle(CurrentWiki) + " - " + Messages.DiscussionUpdates,
                            Settings.GetMainUrl(CurrentWiki),
                            Settings.GetMainUrl(CurrentWiki) + UrlTools.BuildWikiUrl(CurrentWiki, CurrentNamespace, "RSS", (useCat ? ("?Category=" + cat) : "")),
                            Messages.RecentDiscussionUpdates);

                        RecentChange[] ch = RecentChanges.GetAllChanges(CurrentWiki);
                        Array.Reverse(ch);
                        for (int i = 0; i < ch.Length; i++)
                        {
                            // Skip page-related entries
                            if (!IsMessageChange(ch[i].Change)) continue;

                            PageContent p = Pages.FindPage(CurrentWiki, ch[i].Page);
                            if (p != null)
                            {
                                // Check permissions for every page
                                bool canReadThisPageDiscussion = authChecker.CheckActionForPage(p.FullName, Actions.ForPages.ReadDiscussion, currentUsername, currentGroups);
                                if (!canReadThisPageDiscussion) continue;

                                if (useCat)
                                {
                                    CategoryInfo[] infos = Pages.GetCategoriesForPage(p);
                                    if (infos.Length == 0 && cat != "-") continue;
                                    else if (infos.Length != 0)
                                    {
                                        bool found = false;
                                        for (int k = 0; k < infos.Length; k++)
                                        {
                                            if (infos[k].FullName == cat)
                                            {
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (!found) continue;
                                    }
                                }

                                // Check namespace
                                if (NameTools.GetNamespace(p.FullName) != CurrentNamespace) continue;

                                // Write the item element
                                rss.WriteStartElement("item");
                                rss.WriteStartElement("title");
                                rss.WriteCData(Messages.Discussion + ": " + Formatter.StripHtml(FormattingPipeline.PrepareTitle(CurrentWiki, ch[i].Title, false, FormattingContext.PageContent, p.FullName)));
                                rss.WriteEndElement();

                                string id = Tools.GetMessageIdForAnchor(ch[i].DateTime);
                                if (ch[i].Change != Change.MessageDeleted)
                                {
                                    rss.WriteElementString("link", Settings.GetMainUrl(CurrentWiki) + ch[i].Page + GlobalSettings.PageExtension + "?Discuss=1#" + id);
                                }
                                else rss.WriteElementString("link", Settings.GetMainUrl(CurrentWiki) + ch[i].Page + GlobalSettings.PageExtension + "?Discuss=1");

                                string messageContent = FindMessageContent(ch[i].Page, id);

                                UserInfo user = Users.FindUser(CurrentWiki, ch[i].User);
                                string username = user != null ? Users.GetDisplayName(user) : ch[i].User;

                                // Create the description tag
                                StringBuilder sb = new StringBuilder();
                                if (rssFeedsMode == RssFeedsMode.Summary || messageContent == null)
                                {
                                    switch (ch[i].Change)
                                    {
                                        case Change.MessagePosted:
                                            sb.Append(Messages.AMessageHasBeenPostedBy.Replace("##SUBJECT##", ch[i].MessageSubject));
                                            break;
                                        case Change.MessageEdited:
                                            sb.Append(Messages.AMessageHasBeenEditedBy.Replace("##SUBJECT##", ch[i].MessageSubject));
                                            break;
                                        case Change.MessageDeleted:
                                            sb.Append(Messages.AMessageHasBeenDeletedBy.Replace("##SUBJECT##", ch[i].MessageSubject));
                                            break;
                                    }
                                    sb.Append(" " + username + (ch[i].Description.Length > 0 ? ".<br />" + ch[i].Description : "."));
                                }
                                else
                                {
                                    sb.Append(FormattingPipeline.FormatWithPhase3(CurrentWiki, FormattingPipeline.FormatWithPhase1And2(CurrentWiki, messageContent, false, FormattingContext.MessageBody, null), FormattingContext.MessageBody, null));
                                }
                                rss.WriteStartElement("description");
                                rss.WriteCData(sb.ToString());
                                rss.WriteEndElement();

                                // Write the remaining elements
                                rss.WriteElementString("author", username);
                                rss.WriteElementString("pubDate", ch[i].DateTime.ToUniversalTime().ToString("R"));
                                rss.WriteStartElement("guid");
                                rss.WriteAttributeString("isPermaLink", "false");
                                rss.WriteString(GetGuid(ch[i].Page, ch[i].DateTime));
                                rss.WriteEndElement();

                                // Complete the item element
                                rss.WriteEndElement();
                            }
                        }

                        // Complete the channel element
                        CompleteCurrentElement(rss);

                        // Complete the rss element
                        CompleteCurrentElement(rss);

                        // Finish off
                        rss.Flush();
                        rss.Close();
                    }
                }
            }

            return View("~/Views/Wiki/Rss.cshtml");
        }

        /// <summary>
        /// Gets a valid and consistent GUID for RSS items.
        /// </summary>
        /// <param name="item">The item name, for example the page name.</param>
        /// <param name="editDateTime">The last date/time the item was modified.</param>
        /// <returns>The GUID.</returns>
        private string GetGuid(string item, DateTime editDateTime)
        {
            return Hash.Compute(item + editDateTime.ToString("yyyyMMddHHmmss"));
        }

        // Atom namespace constants
        private const string atomPrefix = "atom";
        private const string atomNs = "http://www.w3.org/2005/Atom";

        /// <summary>
        /// Sends the RSS header to the output stream.
        /// </summary>
        /// <param name="rss">The output stream.</param>
        private void BuildRssHeader(XmlWriter rss)
        {
            // Create a version 2.0 heading
            rss.WriteStartElement("rss");
            rss.WriteAttributeString("version", "2.0");
            // Define the atom namespace so we can include a self-referencing link
            rss.WriteAttributeString("xmlns", atomPrefix, null, atomNs);
        }

        /// <summary>
        /// Sends the channel head to the output stream.
        /// </summary>
        /// <param name="rss">The output stream.</param>
        /// <param name="title">The title.</param>
        /// <param name="link">The link.</param>
        /// <param name="selfLink">The self link (atom).</param>
        /// <param name="description">The description.</param>
        /// <returns>The complete channel head.</returns>
        private void BuildChannelHead(XmlWriter rss, string title, string link, string selfLink, string description)
        {
            rss.WriteStartElement("channel");
            rss.WriteStartElement("title");
            rss.WriteCData(title);
            rss.WriteEndElement();
            rss.WriteElementString("link", link);
            rss.WriteStartElement(atomPrefix, "link", atomNs);
            rss.WriteAttributeString("href", selfLink);
            rss.WriteAttributeString("rel", "self");
            rss.WriteAttributeString("type", "application/rss+xml");
            rss.WriteEndElement();
            rss.WriteStartElement("description");
            rss.WriteCData(description);
            rss.WriteEndElement();
            rss.WriteElementString("pubDate", DateTime.Now.ToString("R"));
            rss.WriteElementString("generator", "ScrewTurn Wiki RSS Feed Generator");
        }

        /// <summary>
        /// Completes an element in the output stream.
        /// </summary>
        /// <param name="rss">The output stream.</param>
        private void CompleteCurrentElement(XmlWriter rss)
        {
            rss.WriteEndElement();
        }

        /// <summary>
        /// Tries to find the content of a message.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <param name="messageId">The ID of the message, built using Tools.GetMessageIdForAnchor(...).</param>
        /// <returns>The message content, or <c>null</c>.</returns>
        private string FindMessageContent(string pageName, string messageId)
        {
            PageContent page = Pages.FindPage(CurrentWiki, pageName);
            if (page == null) return null;

            Message[] messages = Pages.GetPageMessages(page);
            if (messages.Length == 0) return null;

            List<Message> linearMessages = UnTreeMessages(messages);
            foreach (Message msg in linearMessages)
            {
                if (messageId == Tools.GetMessageIdForAnchor(msg.DateTime)) return msg.Body;
            }

            return null;
        }

        /// <summary>
        /// Deconstructs a tree of messages and converts it into a flat list.
        /// </summary>
        /// <param name="messages">The input tree.</param>
        /// <returns>The resulting flat list.</returns>
        private static List<Message> UnTreeMessages(IEnumerable<Message> messages)
        {
            List<Message> output = new List<Message>(20);
            output.AddRange(messages);
            foreach (Message msg in messages)
            {
                output.AddRange(UnTreeMessages(msg.Replies));
            }
            return output;
        }

        /// <summary>
        /// Determines whether a change refers to page content.
        /// </summary>
        /// <param name="change">The change.</param>
        /// <returns><c>true</c> if the change refers to page content.</returns>
        private static bool IsPageChange(Change change)
        {
            return change == Change.PageUpdated || change == Change.PageRolledBack ||
                change == Change.PageRenamed || change == Change.PageDeleted;
        }

        /// <summary>
        /// Determines whether a change refers to a message.
        /// </summary>
        /// <param name="change">The change.</param>
        /// <returns><c>true</c> if the change refers to a message.</returns>
        private static bool IsMessageChange(Change change)
        {
            return change == Change.MessagePosted || change == Change.MessageEdited || change == Change.MessageDeleted;
        }

    }
}