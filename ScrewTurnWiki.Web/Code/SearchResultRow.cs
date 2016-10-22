using System;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Web.Code
{
    /// <summary>
    /// Represents a search result in a format useful for screen display.
    /// </summary>
    public class SearchResultRow
    {
        public const string Page = "page";
        public const string Message = "message";
        public const string File = "file";
        public const string Attachment = "attachment";

        private string link;
        private string type;
        private string title;
        private string formattedExcerpt;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CompactSearchResult" /> class.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <param name="type">The result type.</param>
        /// <param name="title">The title.</param>
        /// <param name="formattedExcerpt">The formatted page excerpt.</param>
        public SearchResultRow(string link, string type, string title, string formattedExcerpt)
        {
            this.link = link;
            this.type = type;
            this.title = title;
            this.formattedExcerpt = formattedExcerpt;
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        public string Link
        {
            get { return link; }
        }

        /// <summary>
        /// Gets the type of the result.
        /// </summary>
        public string Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title
        {
            get { return title; }
        }

        /// <summary>
        /// Gets the formatted excerpt.
        /// </summary>
        public string FormattedExcerpt
        {
            get { return formattedExcerpt; }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="T:SearchResultRow" /> class.
        /// </summary>
        /// <param name="result">The result to use.</param>
        /// <returns>The instance.</returns>
        public static SearchResultRow CreateInstance(SearchResult result)
        {
            //string queryStringKeywords = "HL=" + GetKeywordsForQueryString(result.Matches);
            string queryStringKeywords = "HL=";

            if (result.DocumentType == DocumentType.Page)
            {
                PageDocument doc = result.Document as PageDocument;
                return new SearchResultRow(doc.PageFullName + GlobalSettings.PageExtension + "?" + queryStringKeywords, Page,
                    FormattingPipeline.PrepareTitle(Tools.DetectCurrentWiki(), doc.Title, false, FormattingContext.PageContent, doc.PageFullName),
                    string.IsNullOrEmpty(doc.HighlightedContent) ? doc.Content : doc.HighlightedContent);
            }
            else if (result.DocumentType == DocumentType.Message)
            {
                MessageDocument doc = result.Document as MessageDocument;
                PageContent content = Pages.FindPage(doc.Wiki, doc.PageFullName);

                return new SearchResultRow(content.FullName + GlobalSettings.PageExtension + "?" + queryStringKeywords + "&amp;Discuss=1#" + Tools.GetMessageIdForAnchor(doc.DateTime), Message,
                    FormattingPipeline.PrepareTitle(Tools.DetectCurrentWiki(), doc.Subject, false, FormattingContext.MessageBody, content.FullName) + " (" +
                    FormattingPipeline.PrepareTitle(Tools.DetectCurrentWiki(), content.Title, false, FormattingContext.MessageBody, content.FullName) +
                    ")", doc.HighlightedBody);
            }
            else if (result.DocumentType == DocumentType.File)
            {
                FileDocument fileDoc = result.Document as FileDocument;

                string[] fileParts = fileDoc.FileName.Split(new char[] { '|' });

                return new SearchResultRow("GetFile?File=" + Tools.UrlEncode(fileDoc.FileName.Substring(fileParts[0].Length + 1)) +
                    "&amp;Provider=" + Tools.UrlEncode(fileParts[0]),
                    File, fileParts[1], fileDoc.HighlightedFileContent);
            }
            else if (result.DocumentType == DocumentType.Attachment)
            {
                PageAttachmentDocument attnDoc = result.Document as PageAttachmentDocument;
                PageContent content = Pages.FindPage(attnDoc.Wiki, attnDoc.PageFullName);

                return new SearchResultRow(content.FullName + GlobalSettings.PageExtension, Attachment,
                    attnDoc.FileName + " (" +
                    FormattingPipeline.PrepareTitle(Tools.DetectCurrentWiki(), content.Title, false, FormattingContext.PageContent, content.FullName) +
                    ")", attnDoc.HighlightedFileContent);
            }
            else throw new NotSupportedException();
        }
    }
}