using System;
using System.Drawing.Imaging;

namespace ScrewTurn.Wiki.Web.Code
{
    public class FileDataExtension
    {
        public string Extension { get; set; }

        /// <summary>
        /// Custom max with of picture
        /// </summary>
        public int SizeWidth { get; set; }

        /// <summary>
        /// Custom max size of picture
        /// </summary>
        public int SizeMax { get; set; }

        public bool IsFullSize
        {
            get { return SizeWidth == 0 && SizeMax == 0; }
        }

        public ImageFormat ImgFormat { get; set; }

        public FileDataExtension(String extension = null)
        {
            Extension = extension;
            ImgFormat = ImageHelper.DetectImageFormat(extension);
        }
    }
}