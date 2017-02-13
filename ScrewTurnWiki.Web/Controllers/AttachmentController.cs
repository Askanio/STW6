using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Models.Wiki;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class AttachmentController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public AttachmentController(ApplicationSettings settings) : base(settings)
        {
        }

        [ChildActionOnly]
        public ActionResult AttachmentViewer(string pageFullName)
        {
            var models = new List<AttachmentViewerModel>();

            if (pageFullName != null)
            {
                foreach (
                    IFilesStorageProviderV60 provider in
                        Collectors.CollectorsBox.FilesProviderCollector.GetAllProviders(CurrentWiki))
                {
                    string[] attachments = provider.ListPageAttachments(pageFullName);
                    foreach (string s in attachments)
                    {
                        var model = new AttachmentViewerModel();
                        model.Name = s;
                        model.Size = Tools.BytesToString(provider.GetPageAttachmentDetails(pageFullName, s).Size);
                        model.Link = "GetFile?File=" + Tools.UrlEncode(s).Replace("'", "&#39;") +
                                     "&amp;AsStreamAttachment=1&amp;Provider=" +
                                     provider.GetType().FullName + "&amp;IsPageAttachment=1&amp;Page=" +
                                     Tools.UrlEncode(pageFullName);
                        models.Add(model);
                    }
                }
            }
            return PartialView(models);
        }
    }
}