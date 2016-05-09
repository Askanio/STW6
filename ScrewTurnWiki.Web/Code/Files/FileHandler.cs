using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Localization.Messages;

namespace ScrewTurn.Wiki.Web.Code.Files
{
    public class FileHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //var t = "\n \r"; // "&lt;img&gt;text&lt;img&gt;";
            //var tt = WebUtility.HtmlEncode(WebUtility.HtmlEncode(t));

            // http://msdn.microsoft.com/ru-ru/magazine/cc163463.aspx

            // Set up the response settings
            //context.Response.ContentType = "image/jpeg";
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.BufferOutput = false;

            NameValueCollection nvc = HttpUtility.ParseQueryString(HttpUtility.HtmlDecode(context.Server.UrlDecode(context.Request.QueryString.ToString())));

            string filename = nvc.Get("File");//context.Request.QueryString["File"];// +ss;
            if (filename == null)
            {
                context.Response.StatusCode = 404;
                context.Response.Write(Messages.FileNotFound);
                return;
            }

            string currentWiki = Tools.DetectCurrentWiki();

            // Remove ".." sequences that might be a security issue
            filename = filename.Replace("..", "");

            var fileExt = ImageHelper.GetExtFileName(ref filename);

            var page = nvc.Get("Page"); //context.Request["Page"]
            bool isPageAttachment = !string.IsNullOrEmpty(page);
            PageContent pageContent = isPageAttachment ? Pages.FindPage(currentWiki, page) : null;
            if (isPageAttachment && pageContent == null)
            {
                context.Response.StatusCode = 404;
                context.Response.Write(Messages.FileNotFound);
                return;
            }

            IFilesStorageProviderV60 provider;
            string prov = nvc.Get("Provider"); //context.Request["Provider"]
            if (!string.IsNullOrEmpty(prov)) provider = Collectors.CollectorsBox.FilesProviderCollector.GetProvider(prov, currentWiki);
            else
            {
                if (isPageAttachment)
                    provider = FilesAndAttachments.FindPageAttachmentProvider(currentWiki, pageContent.FullName, filename);
                else
                    provider = FilesAndAttachments.FindFileProvider(currentWiki, filename);
            }

            if (provider == null)
            {
                context.Response.StatusCode = 404;
                context.Response.Write("File not found.");
                return;
            }

            // Use canonical path format (leading with /)
            if (!isPageAttachment)
            {
                if (!filename.StartsWith("/")) filename = "/" + filename;
                filename = filename.Replace("\\", "/");
            }

            // Verify permissions
            bool canDownload = false;

            AuthChecker authChecker = new AuthChecker(Collectors.CollectorsBox.GetSettingsProvider(currentWiki));

            if (isPageAttachment)
            {
                canDownload = authChecker.CheckActionForPage(pageContent.FullName, Actions.ForPages.DownloadAttachments,
                    SessionFacade.GetCurrentUsername(), SessionFacade.GetCurrentGroupNames(currentWiki));
            }
            else
            {
                string dir = Tools.GetDirectoryName(filename);
                canDownload = authChecker.CheckActionForDirectory(provider, dir,
                     Actions.ForDirectories.DownloadFiles, SessionFacade.GetCurrentUsername(),
                     SessionFacade.GetCurrentGroupNames(currentWiki));
            }
            if (!canDownload)
            {
                context.Response.StatusCode = 401;
                return;
            }

            long size = -1;

            if (fileExt.ImgFormat == null)
            {
                FileDetails details = null;
                if (isPageAttachment) details = provider.GetPageAttachmentDetails(pageContent.FullName, filename);
                else details = provider.GetFileDetails(filename);

                if (details != null) size = details.Size;
                else
                {
                    Log.LogEntry(
                        "Attempted to download an inexistent file/attachment (" +
                        (pageContent != null ? pageContent.FullName + "/" : "") + filename + ")", EntryType.Warning,
                        Log.SystemUsername, currentWiki);
                    context.Response.StatusCode = 404;
                    context.Response.Write("File not found.");
                    return;
                }
            }

            string mime = "";
            try
            {
                string ext = Path.GetExtension(filename);
                if (ext.StartsWith(".")) ext = ext.Substring(1).ToLowerInvariant(); // Remove trailing dot
                mime = GetMimeType(ext);
                //mime = GetMimeType(fileExt.Extension);
            }
            catch
            {
                // ext is null -> no mime type -> abort
                context.Response.Write(filename + "<br />");
                context.Response.StatusCode = 404;
                context.Response.Write("File not found.");
                //mime = "application/octet-stream";
                return;
            }

            // Prepare response
            context.Response.Clear();
            context.Response.ContentType = mime;
            ////Response.AddHeader("content-type", mime);
            string streamattach = nvc.Get("AsStreamAttachment"); // context.Request["AsStreamAttachment"]
            if (streamattach != null)
            {
                context.Response.AddHeader("content-disposition", "attachment;filename=\"" + Path.GetFileName(filename) + "\"");
            }
            else
            {
                context.Response.AddHeader("content-disposition", "inline;filename=\"" + Path.GetFileName(filename) + "\"");
            }
            if (size != -1)
                context.Response.AddHeader("content-length", size.ToString(CultureInfo.InvariantCulture)); // NOTE: 2

            bool retrieved = false;
            if (fileExt.ImgFormat != null) // Image file
            {
                bool enableOverlayingText = Settings.GetEnableOverlayingTextOnAnImage(currentWiki);
                using (var sourceStream = new MemoryStream())
                {

                    if (isPageAttachment)
                    {
                        try
                        {
                            if (fileExt.IsFullSize && !enableOverlayingText)
                                retrieved = provider.RetrievePageAttachment(pageContent.FullName, filename, context.Response.OutputStream);
                            else
                                retrieved = provider.RetrievePageAttachment(pageContent.FullName, filename, sourceStream);
                        }
                        catch (ArgumentException ex)
                        {
                            Log.LogEntry(
                                "Attempted to download an inexistent attachment (" + pageContent.FullName + "/" + filename +
                                ")\n" + ex.ToString(), EntryType.Warning, Log.SystemUsername, currentWiki);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (fileExt.IsFullSize && !enableOverlayingText)
                                retrieved = provider.RetrieveFile(filename, context.Response.OutputStream);
                            else
                                retrieved = provider.RetrieveFile(filename, sourceStream);
                        }
                        catch (ArgumentException ex)
                        {
                            Log.LogEntry(
                                "Attempted to download an inexistent file/attachment (" + filename + ")\n" + ex.ToString(),
                                EntryType.Warning, Log.SystemUsername, currentWiki);
                        }
                    }

                    if (retrieved)
                    {
                        if (!fileExt.IsFullSize)
                            ImageHelper.GetThumbnail(sourceStream, context.Response.OutputStream, fileExt);
                        else if (enableOverlayingText)
                            ImageHelper.OverlayingText(sourceStream, context.Response.OutputStream, fileExt, Settings.GetOverlayingTextOnAnImage(currentWiki));
                        //size = context.Response.OutputStream.Length;
                    }
                }
            }
            else // Other file
            {
                if (isPageAttachment)
                {
                    try
                    {
                        retrieved = provider.RetrievePageAttachment(pageContent.FullName, filename, context.Response.OutputStream);
                    }
                    catch (ArgumentException ex)
                    {
                        Log.LogEntry("Attempted to download an inexistent attachment (" + pageContent.FullName + "/" + filename + ")\n" + ex.ToString(), EntryType.Warning, Log.SystemUsername, currentWiki);
                    }
                }
                else
                {
                    try
                    {
                        retrieved = provider.RetrieveFile(filename, context.Response.OutputStream);
                    }
                    catch (ArgumentException ex)
                    {
                        Log.LogEntry("Attempted to download an inexistent file/attachment (" + filename + ")\n" + ex.ToString(), EntryType.Warning, Log.SystemUsername, currentWiki);
                    }
                }
            }

            if (!retrieved)
            {
                context.Response.StatusCode = 404;
                context.Response.Write("File not found.");
                return;
            }

            // Set the cache duration accordingly to the file date/time
            ////Response.AddFileDependency(filename);
            ////Response.Cache.SetETagFromFileDependencies();
            ////Response.Cache.SetLastModifiedFromFileDependencies();
            context.Response.Cache.SetETag(filename.GetHashCode().ToString() + "-" + size.ToString());
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetSlidingExpiration(true);
            context.Response.Cache.SetValidUntilExpires(true);
            context.Response.Cache.VaryByParams["File"] = true;
            context.Response.Cache.VaryByParams["Provider"] = true;
            context.Response.Cache.VaryByParams["Page"] = true;
            context.Response.Cache.VaryByParams["IsPageAttachment"] = true;

            if (context.Response.IsClientConnected)
                context.Response.Flush();
            context.Response.End();
            //HttpContext.Current.ApplicationInstance.CompleteRequest();

            //byte[] image = null;
            //if (!String.IsNullOrEmpty(context.Request.QueryString["id"]))
            //{
            //    var id = context.Request.QueryString["id"];
            //    image = PictureHelper.GetPhoto(id, context.Request.QueryString["size"]);
            //}
            //if (image != null)
            //    context.Response.BinaryWrite(image); 
        }

        private string GetMimeType(string ext)
        {
            string mime = "";
            if (MimeTypes.Types.TryGetValue(ext, out mime)) return mime;
            else return "application/octet-stream";
        }

        #endregion
    }
}