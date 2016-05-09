using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace ScrewTurn.Wiki.Web.Code
{
    public static class ImageHelper
    {
        private const long QualityPoster = 90; // TODO: Move to Settings

        public static FileDataExtension GetExtFileName(ref string filename)
        {
            int position = filename.LastIndexOf(".", StringComparison.InvariantCulture);
            if (position == -1)
                return new FileDataExtension(String.Empty);

            var split = filename.Substring(position + 1).Split('|');
            String extension = split[0];

            int sizeW = 0;
            int sizeMax = 0;

            if (split.Length > 1)
            {
                filename = filename.Substring(0, position + 1);
                filename = String.Concat(filename, split[0]);

                if (split[1].StartsWith("m"))
                {
                    if (!Int32.TryParse(split[1].Substring(1, split[1].Length - 1), out sizeMax))
                        sizeMax = 0;
                }
                else if (split[1].StartsWith("w"))
                {
                    if (!Int32.TryParse(split[1].Substring(1, split[1].Length - 1), out sizeW))
                        sizeW = 0;
                }
            }
            return new FileDataExtension(extension) { SizeWidth = sizeW, SizeMax = sizeMax };
        }

        public static void OverlayingText(Stream pictureStream, Stream destinationStream, FileDataExtension fileDataExtension, string text)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(pictureStream))
                {
                    using (Bitmap newImage = new Bitmap(bmp.Size.Width, bmp.Size.Height, PixelFormat.Format24bppRgb))
                    {
                        using (Graphics canvas = Graphics.FromImage(newImage))
                        {
                            canvas.SmoothingMode = SmoothingMode.AntiAlias;
                            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            canvas.DrawImage(bmp, new Rectangle(new Point(0, 0), bmp.Size));

                            //var brush = new LinearGradientBrush(new Point(0, bmp.Size.Height - FontHeight),
                            //                                    new Point(0, bmp.Size.Height), Color.AliceBlue,
                            //                                    Color.Navy);

                            Font font = new Font("Arial", 16, FontStyle.Regular, GraphicsUnit.Pixel);
                            SizeF textSize = canvas.MeasureString(text, font);
                            PointF pointF = new PointF(bmp.Size.Width - textSize.Width - 10, bmp.Size.Height - textSize.Height - 10);
                            canvas.DrawString(text, font, new SolidBrush(Color.WhiteSmoke), pointF); // Color.FromArgb(100, 0, 0, 0)

                            ImageCodecInfo encoder = GetEnCodec(fileDataExtension.ImgFormat);
                            EncoderParameter paramRatio = new EncoderParameter(Encoder.Quality, QualityPoster);
                            EncoderParameters enParams = new EncoderParameters(1);
                            enParams.Param[0] = paramRatio;

                            newImage.Save(destinationStream, encoder, enParams);
                        }
                    }
                }
                //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                //Font font = new Font("Verdana", 20, FontStyle.Bold);
                //SolidBrush text = new SolidBrush(Color.FromArgb(photoSlideInfo.ColorWatermark));
                //SizeF textSize = g.MeasureString(Constantes.WatermarkEmail, font);
                //PointF pointF = new PointF((backImg.Width - textSize.Width) / 2, (backImg.Height - textSize.Height) / 2);
                //g.DrawString(Constantes.WatermarkEmail, font, text, pointF);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void GetThumbnail(Stream pictureStream, Stream destinationStream, FileDataExtension fileDataExtension)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(pictureStream))
                {
                    Size newSize = CalculateDimensions(bmp.Size, fileDataExtension);
                    using (Bitmap newImage = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format24bppRgb))
                    {
                        using (Graphics canvas = Graphics.FromImage(newImage))
                        {
                            canvas.SmoothingMode = SmoothingMode.AntiAlias;
                            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            canvas.DrawImage(bmp, new Rectangle(new Point(0, 0), newSize));

                            // Thumbnail always in jpeg
                            ImageCodecInfo encoder = GetEnCodec(ImageFormat.Jpeg); //(fileDataExtension.ImgFormat);
                            EncoderParameter paramRatio = new EncoderParameter(Encoder.Quality, QualityPoster);
                            EncoderParameters enParams = new EncoderParameters(1);
                            enParams.Param[0] = paramRatio;

                            newImage.Save(destinationStream, encoder, enParams);
                            //size = new Size(newImage.Width, newImage.Height);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private static Size CalculateDimensions(Size oldSize, FileDataExtension fileDataExtension)
        {
            Size newSize = new Size();

            if (fileDataExtension.SizeMax != 0)
            {
                if (oldSize.Height > oldSize.Width)
                {
                    newSize.Width = (int)(oldSize.Width * ((float)fileDataExtension.SizeMax / (float)oldSize.Height));
                    newSize.Height = fileDataExtension.SizeMax;
                }
                else
                {
                    newSize.Width = fileDataExtension.SizeMax;
                    newSize.Height = (int)(oldSize.Height * ((float)fileDataExtension.SizeMax / (float)oldSize.Width));
                }
            }
            else if (fileDataExtension.SizeWidth != 0)
            {
                newSize.Width = fileDataExtension.SizeWidth;
                newSize.Height = (int)(oldSize.Height * ((float)fileDataExtension.SizeWidth / (float)oldSize.Width));
            }
            return newSize;
        }

        public static ImageFormat DetectImageFormat(string format)
        {
            switch (format.ToLower())
            {
                case "jpg":
                case "jpeg":
                    return ImageFormat.Jpeg;
                case "png":
                    return ImageFormat.Png;
                case "gif":
                    return ImageFormat.Gif;
                case "bmp":
                    return ImageFormat.Bmp;
                case "emf":
                    return ImageFormat.Emf;
                case "exif":
                    return ImageFormat.Exif;
                case "ico":
                    return ImageFormat.Icon;
                case "tif":
                case "tiff":
                    return ImageFormat.Tiff;
                case "wmf":
                    return ImageFormat.Wmf;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get graphic codec
        /// </summary>
        /// <param name="imageFormat">Type codec</param>
        /// <returns></returns>
        private static ImageCodecInfo GetEnCodec(ImageFormat imageFormat)
        {
            ImageCodecInfo[] decoders = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo decoder in decoders)
            {
                if (decoder.FormatID == imageFormat.Guid)
                    return decoder;
            }
            return null;
        }

        public static bool IsImage(string name)
        {
            string ext = System.IO.Path.GetExtension(name.ToLowerInvariant());
            return ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".png" || ext == ".tif" || ext == ".tiff" || ext == ".bmp";
        }
    }
}