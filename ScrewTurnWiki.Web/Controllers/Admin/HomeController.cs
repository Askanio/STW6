using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Mvc;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Code.Attributes;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models.Admin.Home;

namespace ScrewTurn.Wiki.Web.Controllers.Admin
{
    [RoutePrefix("Admin")]
    [CheckActionForGlobals(Action = CheckActionForGlobalsAttribute.ActionForGlobals.ManageConfiguration)]
    public class HomeController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public HomeController(ApplicationSettings settings) : base(settings)
        {
        }

        [HttpGet]
        [Route("MissingPages")]
        public ActionResult MissingPages()
        {
            var model = new MissingPagesModel();
            base.PrepareModel(model, AdminMenu.MissingPages);
            model.WantedPages = GetRptPages();

            return View("~/Views/Admin/Home/MissingPages.cshtml", model);
        }

        private List<WantedPageRow> GetRptPages()
        {
            List<WantedPageRow> result = new List<WantedPageRow>(50);

            Dictionary <string, List<string>> links = Pages.GetWantedPages(CurrentWiki, null);
            foreach (KeyValuePair<string, List<string>> pair in links)
                result.Add(new WantedPageRow(CurrentWiki, WebUtility.HtmlEncode(Settings.GetRootNamespaceName(CurrentWiki)), "", pair.Key, pair.Value));

            foreach (NamespaceInfo nspace in Pages.GetNamespaces(CurrentWiki))
            {
                links = Pages.GetWantedPages(CurrentWiki, nspace.Name);
                foreach (KeyValuePair<string, List<string>> pair in links)
                    result.Add(new WantedPageRow(CurrentWiki, nspace.Name, nspace.Name + ".", pair.Key, pair.Value));
            }

            return result;
        }

        #region OrphanPages

        [HttpGet]
        [Route("OrphanPages")]
        public ActionResult OrphanPages()
        {
            var model = new OrphanPagesModel();
            base.PrepareModel(model, AdminMenu.OrphanPages);

            int orphans = Pages.GetOrphanedPages(CurrentWiki, null as NamespaceInfo).Length;
            foreach (NamespaceInfo nspace in Pages.GetNamespaces(CurrentWiki))
                orphans += Pages.GetOrphanedPages(CurrentWiki, nspace).Length;
            model.OrphanPagesCount = orphans;
            model.IndexProviders = GetIndexProviders();

            return View("~/Views/Admin/Home/OrphanPages.cshtml", model);
        }

        private List<IndexRow> GetIndexProviders()
        {
            List<IndexRow> result = new List<IndexRow>(5);

            foreach (IPagesStorageProviderV60 prov in Collectors.CollectorsBox.PagesProviderCollector.GetAllProviders(CurrentWiki))
            {
                result.Add(new IndexRow("PagesRebuild", prov));
            }

            foreach (IFilesStorageProviderV60 prov in Collectors.CollectorsBox.FilesProviderCollector.GetAllProviders(CurrentWiki))
            {
                result.Add(new IndexRow("FilesRebuild", prov));
            }

            return result;
        }

        [HttpGet]
        [Route("RebuildPageLinks")]
        public ActionResult RebuildPageLinks()
        {
            RebuildPageLinks(Pages.GetPages(CurrentWiki, null));
            foreach (NamespaceInfo nspace in Pages.GetNamespaces(CurrentWiki))
                RebuildPageLinks(Pages.GetPages(CurrentWiki, nspace));

            return RedirectToAction("OrphanPages");
        }

        /// <summary>
        /// Rebuilds the page links for the specified pages.
        /// </summary>
        /// <param name="pages">The pages.</param>
        private void RebuildPageLinks(IList<PageContent> pages)
        {
            foreach (PageContent page in pages)
                Pages.StorePageOutgoingLinks(page);
        }

        [HttpGet]
        [Route("RebuildProvidersIndex")]
        public ActionResult RebuildProvidersIndex(string command, string providerType)
        {
            //if (command == null || providerType= null)


            Log.LogEntry("Index rebuild requested for " + providerType, EntryType.General, SessionFacade.GetCurrentUsername(), CurrentWiki);

            if (command == "PagesRebuild")
            {

                // Clear the pages search index for the current wiki
                SearchClass.ClearIndex(CurrentWiki);

                IPagesStorageProviderV60 pagesProvider = Collectors.CollectorsBox.PagesProviderCollector.GetProvider(providerType, CurrentWiki);

                // Index all pages of the wiki
                List<NamespaceInfo> namespaces = new List<NamespaceInfo>(pagesProvider.GetNamespaces());
                namespaces.Add(null);
                foreach (NamespaceInfo nspace in namespaces)
                {
                    // Index pages of the namespace
                    PageContent[] pages = pagesProvider.GetPages(nspace);
                    foreach (PageContent page in pages)
                    {
                        // Index page
                        SearchClass.IndexPage(page);

                        // Index page messages
                        Message[] messages = pagesProvider.GetMessages(page.FullName);
                        foreach (Message message in messages)
                        {
                            SearchClass.IndexMessage(message, page);

                            // Search for replies
                            Message[] replies = message.Replies;
                            foreach (Message reply in replies)
                            {
                                // Index reply
                                SearchClass.IndexMessage(reply, page);
                            }
                        }
                    }
                }
            }

            else if (command == "FilesRebuild")
            {
                // Clear the files search index for the current wiki
                SearchClass.ClearFilesIndex(CurrentWiki);

                IFilesStorageProviderV60 filesProvider = Collectors.CollectorsBox.FilesProviderCollector.GetProvider(providerType, CurrentWiki);

                // Index all files of the wiki
                // 1. List all directories (add the root directory: null)
                // 2. List all files in each directory
                // 3. Index each file
                List<string> directories = new List<string>(filesProvider.ListDirectories(null));
                directories.Add(null);
                foreach (string directory in directories)
                {
                    string[] files = filesProvider.ListFiles(directory);
                    foreach (string file in files)
                    {
                        byte[] fileContent;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            filesProvider.RetrieveFile(file, stream);
                            fileContent = new byte[stream.Length];
                            stream.Seek(0, SeekOrigin.Begin);
                            stream.Read(fileContent, 0, (int)stream.Length);
                        }

                        // Index the file
                        string tempDir = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Guid.NewGuid().ToString());
                        if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
                        string tempFile = Path.Combine(tempDir, file.Substring(file.LastIndexOf('/') + 1));
                        using (FileStream writer = System.IO.File.Create(tempFile))
                        {
                            writer.Write(fileContent, 0, fileContent.Length);
                        }
                        SearchClass.IndexFile(filesProvider.GetType().FullName + "|" + file, tempFile, CurrentWiki);
                        Directory.Delete(tempDir, true);
                    }
                }

                // Index all attachment of the wiki
                string[] pagesWithAttachments = filesProvider.GetPagesWithAttachments();
                foreach (string page in pagesWithAttachments)
                {
                    string[] attachments = filesProvider.ListPageAttachments(page);
                    foreach (string attachment in attachments)
                    {
                        byte[] fileContent;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            filesProvider.RetrievePageAttachment(page, attachment, stream);
                            fileContent = new byte[stream.Length];
                            stream.Seek(0, SeekOrigin.Begin);
                            stream.Read(fileContent, 0, (int)stream.Length);
                        }

                        // Index the attached file
                        string tempDir = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Guid.NewGuid().ToString());
                        if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
                        string tempFile = Path.Combine(tempDir, attachment);
                        using (FileStream writer = System.IO.File.Create(tempFile))
                        {
                            writer.Write(fileContent, 0, fileContent.Length);
                        }
                        SearchClass.IndexPageAttachment(attachment, tempFile, Pages.FindPage(CurrentWiki, page));
                        Directory.Delete(tempDir, true);
                    }
                }
            }

            Log.LogEntry("Index rebuild completed for " + providerType, EntryType.General, SessionFacade.GetCurrentUsername(), CurrentWiki);

            return RedirectToAction("OrphanPages");
        }

        #endregion

        [HttpGet]
        [Route("BulkEmail")]
        public ActionResult BulkEmail()
        {
            var model = new BulkEmailModel();
            base.PrepareModel(model, AdminMenu.BulkEmail);
            model.Groups = GetGroups();

            return View("~/Views/Admin/Home/BulkEmail.cshtml", model);
        }

        private List<SelectListItem> GetGroups()
        {
            var lstGroups = new List<SelectListItem>();
            string anon = Settings.GetAnonymousGroup(CurrentWiki);
            foreach (UserGroup group in Users.GetUserGroups(CurrentWiki))
                if (group.Name != anon)
                    lstGroups.Add(new SelectListItem()
                    {
                        Selected = true,
                        Text = group.Name,
                        Value = group.Name
                    });

            return lstGroups;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("BulkEmail")]
        public ActionResult BulkEmail(BulkEmailModel model)
        {
            model.Result = "";

            if (ModelState.IsValid)
            {
                List<string> emails = new List<string>();
                foreach (var item in model.Groups)
                {
                    if (item.Selected)
                    {
                        UserGroup group = Users.FindUserGroup(CurrentWiki, item.Value);
                        if (group != null)
                        {
                            foreach (string user in group.Users)
                            {
                                UserInfo u = Users.FindUser(CurrentWiki, user);
                                if (u != null) emails.Add(u.Email);
                            }
                        }
                    }
                }

                EmailTools.AsyncSendMassEmail(emails.ToArray(), GlobalSettings.SenderEmail,
                    model.Subject, model.Body, false);

                model.Result = Messages.MassEmailSent;
            }
            base.PrepareModel(model, AdminMenu.BulkEmail);
            //model.Groups = GetGroups();

            return View("~/Views/Admin/Home/BulkEmail.cshtml", model);
        }
    }
}