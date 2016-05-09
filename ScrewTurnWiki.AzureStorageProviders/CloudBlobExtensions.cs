using System.IO;
using System.Text;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ScrewTurn.Wiki.Plugins.AzureStorage
{
    /// <summary>
    /// 
    /// </summary>
    public static class CloudBlobExtensions
    {
        /// <summary>
        /// Uploads a string of text to a block blob. 
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="content">The text to upload, encoded as a UTF-8 string.</param>
        public static void UploadText(this ICloudBlob blob, string content)
        {
            UploadText(blob, content, Encoding.UTF8, null);
        }

        /// <summary>
        /// Uploads a string of text to a block blob. 
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="content">The text to upload.</param>
        /// <param name="encoding">An object that indicates the text encoding to use.</param>
        /// <param name="options">An object that specifies any additional options for the request.</param>
        public static void UploadText(this ICloudBlob blob, string content, Encoding encoding, BlobRequestOptions options)
        {
            UploadByteArray(blob, encoding.GetBytes(content), options);
        }

        /// <summary>
        /// Uploads a file from the file system to a block blob.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="fileName">The path and file name of the file to upload.</param>
        public static void UploadFile(this ICloudBlob blob, string fileName)
        {
            UploadFile(blob, fileName, null);
        }

        /// <summary>
        /// Uploads a file from the file system to a block blob.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="fileName">The path and file name of the file to upload.</param>
        /// <param name="options">An object that specifies any additional options for the request.</param>
        public static void UploadFile(this ICloudBlob blob, string fileName, BlobRequestOptions options)
        {
            using (var data = File.OpenRead(fileName))
            {
                blob.UploadFromStream(data, null, options);
            }
        }

        /// <summary>
        /// Uploads an array of bytes to a block blob.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="content">The array of bytes to upload.</param>
        public static void UploadByteArray(this ICloudBlob blob, byte[] content)
        {
            UploadByteArray(blob, content, null);
        }

        /// <summary>
        /// Uploads an array of bytes to a blob.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="content">The array of bytes to upload.</param>
        /// <param name="options">An object that specifies any additional options for the request.</param>
        public static void UploadByteArray(this ICloudBlob blob, byte[] content, BlobRequestOptions options)
        {
            using (var data = new MemoryStream(content))
            {
                blob.UploadFromStream(data, null, options);
            }
        }

        /// <summary>
        /// Downloads the blob's contents.
        /// </summary>
        /// <returns>The contents of the blob, as a string.</returns>
        public static string DownloadText(this ICloudBlob blob)
        {
            return DownloadText(blob, null);
        }

        /// <summary>
        /// Downloads the blob's contents.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="options">An object that specifies any additional options for the request.</param>
        /// <returns>The contents of the blob, as a string.</returns>
        public static string DownloadText(this ICloudBlob blob, BlobRequestOptions options)
        {
            Encoding encoding = GetDefaultEncoding();

            byte[] array = DownloadByteArray(blob, options);

            return encoding.GetString(array);
        }

        /// <summary>
        /// Downloads the blob's contents to a file.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="fileName">The path and file name of the target file.</param>
        public static void DownloadToFile(this ICloudBlob blob, string fileName)
        {
            DownloadToFile(blob, fileName, null);
        }

        /// <summary>
        /// Downloads the blob's contents to a file.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="fileName">The path and file name of the target file.</param>
        /// <param name="options">An object that specifies any additional options for the request.</param>
        public static void DownloadToFile(this ICloudBlob blob, string fileName, BlobRequestOptions options)
        {
            using (var fileStream = File.Create(fileName))
            {
                blob.DownloadToStream(fileStream, null, options);
            }
        }

        /// <summary>
        /// Downloads the blob's contents as an array of bytes.
        /// </summary>
        /// <returns>The contents of the blob, as an array of bytes.</returns>
        public static byte[] DownloadByteArray(this ICloudBlob blob)
        {
            return DownloadByteArray(blob, null);
        }

        /// <summary>
        /// Downloads the blob's contents as an array of bytes. 
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="options">An object that specifies any additional options for the request.</param>
        /// <returns>The contents of the blob, as an array of bytes.</returns>
        public static byte[] DownloadByteArray(this ICloudBlob blob, BlobRequestOptions options)
        {
            using (var memoryStream = new MemoryStream())
            {
                blob.DownloadToStream(memoryStream, null, options);

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Gets the default encoding for the blob, which is UTF-8.
        /// </summary>
        /// <returns>The default <see cref="Encoding"/> object.</returns>
        private static Encoding GetDefaultEncoding()
        {
            Encoding encoding = Encoding.UTF8;
            return encoding;
        }

    }
}
