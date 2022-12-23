using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageResizing.Infrastructure
{
    public class BitmapParser
    {
        public static BitmapSource BitmapToBitmapSource(Bitmap src)
        {
            BitmapData bitmapData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, src.PixelFormat);
            BitmapSource bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height, src.HorizontalResolution, src.VerticalResolution,
                PixelFormats.Bgr32, null, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            src.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using MemoryStream outStream = new();

            BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            bitmapEncoder.Save(outStream);

            return new Bitmap(new System.Drawing.Bitmap(outStream));
        }
    }
}
