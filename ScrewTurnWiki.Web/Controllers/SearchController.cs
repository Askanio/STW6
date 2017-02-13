using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Code.Attributes;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models.Common;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class SearchController : PageController
    {

        public SearchController(ApplicationSettings appSettings) : base(appSettings)
        {
        }

        private const string OpensearchXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<OpenSearchDescription xmlns=""http://a9.com/-/spec/opensearch/1.1/"">
	<ShortName>{0}</ShortName>
	<Description>{1}</Description>
	<Url type=""text/html"" method=""get"" template=""{2}Search?AllNamespaces=1&amp;FilesAndAttachments=1&amp;Query={3}""/>
	<Image width=""16"" height=""16"" type=""image/x-icon"">{2}{4}</Image>
	<InputEncoding>UTF-8</InputEncoding>
	<SearchForm>{2}Search</SearchForm>
</OpenSearchDescription>";

        private const int MaxResults = 30;

        private readonly Dictionary<string, SearchOptions> searchModeMap =
            new Dictionary<string, SearchOptions>() { { "1", SearchOptions.AtLeastOneWord }, { "2", SearchOptions.AllWords }, { "3", SearchOptions.ExactPhrase } };

        /// <summary>
        /// Generates the OpenSearch description XML document and renders it to output.
        /// </summary>
        [HttpGet]
        public ActionResult GetOpenSearchDescription()
        {
            //Response.Clear();
            //Response.AddHeader("content-type", "application/opensearchdescription+xml");
            //Response.AddHeader("content-disposition", "inline;filename=search.xml");
            //Response.Write(
            //    string.Format(xml,
            //        Settings.GetWikiTitle(CurrentWiki),
            //        Settings.GetWikiTitle(CurrentWiki) + " - Search",
            //        Settings.GetMainUrl(CurrentWiki),
            //        "{searchTerms}",
            //        "Images/SearchIcon.ico"));
            //Response.End();

            string xml = string.Format(OpensearchXml,
                    Settings.GetWikiTitle(CurrentWiki),
                    Settings.GetWikiTitle(CurrentWiki) + " - Search",
                    Settings.GetMainUrl(CurrentWiki),
                    "{searchTerms}",
                    "Images/SearchIcon.ico");

            return this.Content(xml, "application/opensearchdescription+xml");
        }

        [HttpGet]
        [DetectNamespaceFromPage(PageParamName = "page", Order = 1)]
        public ActionResult Search(string page, string allNamespaces, string filesAndAttachments, string searchUncategorized, string mode, string query)
        {
            //bool allNamespaces = false;
            //if (Request["AllNamespaces"] != null)
            //    allNamespaces = Request["AllNamespaces"] == "1";
            //bool filesAndAttachments = false;
            //if (Request["FilesAndAttachments"] != null)
            //    filesAndAttachments = Request["FilesAndAttachments"] == "1";
            //bool searchUncategorized = true;
            //if (Request["SearchUncategorized"] != null)
            //    searchUncategorized = Request["SearchUncategorized"] == "1";
            //int mode = 0;
            //if (Request["Mode"] != null)
            //    Int32.TryParse(Request["Mode"], out mode);
            //string query = null;
            //if (Request["Query"] != null)
            //    query = Request["Query"];

            bool isAllNamespaces = false;
            if (allNamespaces != null)
                isAllNamespaces = allNamespaces == "1";
            bool isFilesAndAttachments = false;
            if (filesAndAttachments != null)
                isFilesAndAttachments = filesAndAttachments == "1";
            bool isSearchUncategorized = true;
            if (searchUncategorized != null)
                isSearchUncategorized = searchUncategorized == "1";

            var model = new SearchModel();

            base.PrepareDefaultModel(model, CurrentNamespace, CurrentPageFullName);

            model.Namespace = CurrentNamespace;

            model.Title = Messages.SearchTitle + " - " + Settings.GetWikiTitle(CurrentWiki);

            //txtQuery.Focus();

                //string[] queryStringCategories = null;
                //if (categories != null)
                //{
                //    queryStringCategories = categories.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //    Array.Sort(queryStringCategories);
                //}

            model.IsAllNamespaces = isAllNamespaces;

                model.IsUncategorizedPages = isSearchUncategorized;

                if (query != null)
                {
                    model.IsAllNamespaces = allNamespaces == "1";
                    model.IsFilesAndAttachments = isFilesAndAttachments;
                }

                List<CategoryInfo> allCategories = Pages.GetCategories(CurrentWiki, DetectNamespaceInfo());
                var lstCategories = new List<SelectListItem>();
                // Populate categories list and select specified categories if any
           foreach (CategoryInfo cat in allCategories)
            lstCategories.Add(new SelectListItem(){ Selected = false, Text = NameTools.GetLocalName(cat.FullName) , Value = cat.FullName });

            model.Categories = lstCategories;

            model.AtLeastOneWord = true;

            // Select mode if specified
            if (mode != null)
                {
                    switch (mode)
                    {
                        case "1":
                            model.AtLeastOneWord = true;
                            model.AllWords = false;
                            model.ExactPhrase = false;
                            break;
                        case "2":
                            model.AtLeastOneWord = false;
                            model.AllWords = true;
                            model.ExactPhrase = false;
                            break;
                        default:
                            model.AtLeastOneWord = false;
                            model.AllWords = false;
                            model.ExactPhrase = true;
                            break;
                    }
                }

                if (query != null)
                {
                    model.Query = query;

                    //btnGo_Click(sender, e);
                }

            return View("~/Views/Common/Search/Search.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult SearchResult(SearchModel model)
        {
            //if (ModelState.IsValid)
            //{

            //}

            if (PageHelper.ExistsCurrentNamespace(CurrentWiki, model.Namespace))
            {
                CurrentNamespace = model.Namespace;
            }
            

            SearchOptions mode = SearchOptions.ExactPhrase;
            if (model.AtLeastOneWord)
                mode = SearchOptions.AtLeastOneWord;
            if (model.AllWords)
                mode = SearchOptions.AllWords;

            List<CategoryInfo> allCategories = Pages.GetCategories(CurrentWiki, DetectNamespaceInfo());
            List<string> selectedCategories;
            // Populate categories list and select specified categories if any
            var selected = model.Categories.Where(t => t.Selected).ToList();
            if (selected.Any())
            {
                selectedCategories = new List<string>(model.Categories.Count(t => t.Selected));
                foreach (CategoryInfo cat in allCategories)
                    if (selected.Any(t => t.Value == cat.FullName))
                        selectedCategories.Add(cat.FullName);
            }
            else
            {
                selectedCategories = new List<string>(allCategories.Count);
                foreach (CategoryInfo cat in allCategories)
                    selectedCategories.Add(cat.FullName);
            }

            var searchResult = PerformSearch(model.Query, mode, selectedCategories, model.IsUncategorizedPages, model.IsAllNamespaces, model.IsFilesAndAttachments);
            return PartialView("~/Views/Common/Search/SearchResult.cshtml", searchResult);
        }

        /// <summary>
        /// Performs a search.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="mode">The search mode.</param>
        /// <param name="categories">The selected categories.</param>
        /// <param name="searchUncategorized">A value indicating whether to search uncategorized pages.</param>
        /// <param name="searchInAllNamespacesAndCategories">A value indicating whether to search in all namespaces and categories.</param>
        /// <param name="searchFilesAndAttachments">A value indicating whether to search files and attachments.</param>
        private List<SearchResultRow> PerformSearch(string query, SearchOptions mode, List<string> categories, bool searchUncategorized, bool searchInAllNamespacesAndCategories, bool searchFilesAndAttachments)
        {
            List<SearchResult> results = null;
            //DateTime begin = DateTime.Now;
            try
            {
                List<SearchField> searchFields = new List<SearchField>(2) { SearchField.Title, SearchField.Content };
                if (searchFilesAndAttachments) searchFields.AddRange(new SearchField[] { SearchField.FileName, SearchField.FileContent });
                results = SearchClass.Search(CurrentWiki, searchFields.ToArray(), query, mode);
            }
            catch (ArgumentException ex)
            {
                Log.LogEntry("Search threw an exception\n" + ex.ToString(), EntryType.Warning, SessionFacade.CurrentUsername, CurrentWiki);
                results = new List<SearchResult>();
            }
            //DateTime end = DateTime.Now;

            // Build a list of SearchResultRow for display in the repeater
            List<SearchResultRow> rows = new List<SearchResultRow>(Math.Min(results.Count, MaxResults));

            string currentUser = SessionFacade.GetCurrentUsername();
            string[] currentGroups = SessionFacade.GetCurrentGroupNames(CurrentWiki);

            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));

            CategoryInfo[] pageCategories;
            int count = 0;
            foreach (SearchResult res in results)
            {
                // Filter by category
                PageContent currentPage = null;
                pageCategories = new CategoryInfo[0];

                if (res.DocumentType == DocumentType.Page)
                {
                    PageDocument doc = res.Document as PageDocument;
                    currentPage = Pages.FindPage(doc.Wiki, doc.PageFullName);
                    pageCategories = Pages.GetCategoriesForPage(currentPage);

                    // Verify permissions
                    bool canReadPage = authChecker.CheckActionForPage(currentPage.FullName,
                        Actions.ForPages.ReadPage, currentUser, currentGroups);
                    if (!canReadPage) continue; // Skip
                }
                else if (res.DocumentType == DocumentType.Message)
                {
                    MessageDocument doc = res.Document as MessageDocument;
                    currentPage = Pages.FindPage(doc.Wiki, doc.PageFullName);
                    pageCategories = Pages.GetCategoriesForPage(currentPage);

                    // Verify permissions
                    bool canReadDiscussion = authChecker.CheckActionForPage(currentPage.FullName,
                        Actions.ForPages.ReadDiscussion, currentUser, currentGroups);
                    if (!canReadDiscussion) continue; // Skip
                }
                else if (res.DocumentType == DocumentType.Attachment)
                {
                    PageAttachmentDocument doc = res.Document as PageAttachmentDocument;
                    currentPage = Pages.FindPage(doc.Wiki, doc.PageFullName);
                    pageCategories = Pages.GetCategoriesForPage(currentPage);

                    // Verify permissions
                    bool canDownloadAttn = authChecker.CheckActionForPage(currentPage.FullName,
                        Actions.ForPages.DownloadAttachments, currentUser, currentGroups);
                    if (!canDownloadAttn) continue; // Skip
                }
                else if (res.DocumentType == DocumentType.File)
                {
                    FileDocument doc = res.Document as FileDocument;
                    string[] fields = doc.FileName.Split('|');
                    IFilesStorageProviderV60 provider = Collectors.CollectorsBox.FilesProviderCollector.GetProvider(fields[0], CurrentWiki);
                    string directory = Tools.GetDirectoryName(fields[1]);

                    // Verify permissions
                    bool canDownloadFiles = authChecker.CheckActionForDirectory(provider, directory,
                        Actions.ForDirectories.DownloadFiles, currentUser, currentGroups);
                    if (!canDownloadFiles) continue; // Skip
                }

                var currentNamespace = CurrentNamespace;
                if (string.IsNullOrEmpty(currentNamespace)) currentNamespace = null;

                if (currentPage != null)
                {
                    // Check categories match, if page is set

                    if (searchInAllNamespacesAndCategories ||
                        Array.Find(pageCategories,
                        delegate (CategoryInfo c) {
                            return categories.Contains(c.FullName);
                        }) != null || pageCategories.Length == 0 && searchUncategorized)
                    {

                        // ... then namespace
                        if (searchInAllNamespacesAndCategories ||
                            NameTools.GetNamespace(currentPage.FullName) == currentNamespace)
                        {

                            rows.Add(SearchResultRow.CreateInstance(res));
                            count++;
                        }
                    }
                }
                else
                {
                    // No associated page (-> file), add result
                    rows.Add(SearchResultRow.CreateInstance(res));
                    count++;
                }

                if (count >= MaxResults) break;
            }

            return rows;
        }
    }
}