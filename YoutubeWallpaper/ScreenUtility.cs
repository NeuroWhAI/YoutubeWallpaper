using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace YoutubeWallpaper
{
    public class ScreenUtility
    {
        public static void FillScreen(Form form, Screen screen)
        {
            using (var g = form.CreateGraphics())
            {
                var location = screen.Bounds.Location;
                var size = screen.Bounds.Size;

                PointF dpi = new PointF(96.0f / g.DpiX, 96.0f / g.DpiY);

                form.Location = new Point((int)(location.X * dpi.X),
                    (int)(location.Y * dpi.Y));
                form.Size = new Size((int)(size.Width * dpi.X),
                    (int)(size.Height * dpi.Y));
            }
        }
    }
}
