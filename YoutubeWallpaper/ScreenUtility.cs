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
            form.Location = screen.Bounds.Location;
            form.Size = screen.Bounds.Size;
        }
    }
}
