using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Models.Home;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class AttachmentController : BaseController
    {
        [ChildActionOnly]
        public ActionResult AttachmentViewer(string pageFullName)
        {
            var models = new List<AttachmentViewerModel>();

            if (pageFullName != null)
            {
                string currentWiki = Tools.DetectCurrentWiki(); //TODO: To Base Class

                foreach (
                    IFilesStorageProviderV60 provider in
                        Collectors.CollectorsBox.FilesProviderCollector.GetAllProviders(currentWiki))
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