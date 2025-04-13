using System.Drawing;
using System.Drawing.Drawing2D;

namespace AnythingButton
{
    public class UtilityIcon
    {
        public static Bitmap ResizeIcon(Image originalImage, int size = 24)
        {

            Bitmap resizedBitmap = new Bitmap(size, size);
            Graphics graphics = Graphics.FromImage(resizedBitmap);

            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(originalImage, new Rectangle(0, 0, size, size));

            return resizedBitmap;
        }

    }
}
