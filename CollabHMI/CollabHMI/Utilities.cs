using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CollabHMI
{
    public class Utilities
    {
        public static Bitmap DownScaleBitmap(Bitmap origin, float downScaleFactor)
        {
            Bitmap resized = new Bitmap(origin, new Size((int)(origin.Width * 1.0f / downScaleFactor),
                                                           (int)(origin.Height * 1.0f / downScaleFactor)));
            return resized;
        }
        public static Bitmap ResizeBitmap(Bitmap origin, Size targetSize)
        {
            return new Bitmap(origin, targetSize);
        }
        public static BitmapImage ToBitmapImage(Bitmap bitmap, ImageFormat imgFormat)
        {
            if (bitmap == null)
            {
                return null;
            }
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, imgFormat);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                //bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        public static Bitmap BitmapImageToJpegBitmap(BitmapImage bmpImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new JpegBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bmpImage));
                enc.Save(outStream);
                Bitmap bmp = new Bitmap(outStream);
                return bmp;
            }
        }
    }
}
