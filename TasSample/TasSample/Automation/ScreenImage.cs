using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using Drawing = System.Drawing;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using System;

namespace TasSample.Automation
{
    public class ScreenImage
    {
        public static Color GetColor(Point point)
        {
            Drawing.Bitmap bmp = new Drawing.Bitmap(1, 1, PixelFormat.Format32bppArgb);

            using (Drawing.Graphics g = Drawing.Graphics.FromImage(bmp))
            {
                g.CopyFromScreen((int)point.X, (int)point.Y, 0, 0, new Drawing.Size(1, 1), Drawing.CopyPixelOperation.SourceCopy);
            }

            Drawing.Color c = bmp.GetPixel(0, 0);

            return Color.FromRgb(c.R, c.G, c.B);
        }

        public static ScreenImage GetImage(Point leftTop, Size size)
        {
            Drawing.Bitmap bitmap = new Drawing.Bitmap((int)size.Width, (int)size.Height, PixelFormat.Format32bppArgb);

            using (Drawing.Graphics g = Drawing.Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen((int)leftTop.X, (int)leftTop.Y, 0, 0, new Drawing.Size((int)size.Width, (int)size.Height), Drawing.CopyPixelOperation.SourceCopy);
            }

            return new ScreenImage(bitmap);
        }

        public static ScreenImage GetImage(Point leftTop, Point rightBottom)
        {
            Vector v = rightBottom - leftTop;

            return GetImage(leftTop, new Size(v.X, v.Y));
        }

        private Drawing.Bitmap bitmap;

        private ScreenImage(Drawing.Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public void Save(string filePath, ScreenImageFormat format)
        {
            ImageFormat imageFormat = (ImageFormat)typeof(ImageFormat).GetProperty(format.ToString()).GetValue(null, null);

            this.bitmap.Save(filePath, imageFormat);
        }
    }
}
