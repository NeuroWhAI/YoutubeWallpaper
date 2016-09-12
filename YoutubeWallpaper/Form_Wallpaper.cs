using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YoutubeWallpaper
{
    public partial class Form_Wallpaper : Form
    {
        public Form_Wallpaper()
        {
            InitializeComponent();
        }

        //#############################################################################################

        protected bool m_isFixed = false;
        public bool IsFixed
        { get { return m_isFixed; } }

        public string Uri
        {
            get { return this.webBrowser_page.Url.ToString(); }
            set { this.webBrowser_page.Navigate(value); }
        }

        public int Volume
        {
            get
            {
                uint temp = 0;
                WinApi.waveOutGetVolume(IntPtr.Zero, out temp);
                return (int)((double)(temp & 0xFFFF) * 100 / 0xFFFF);
            }
            set
            {
                uint vol = (uint)((double)0xFFFF * value / 100) & 0xFFFF;
                WinApi.waveOutSetVolume(IntPtr.Zero, (vol << 16) | vol);
            }
        }

        //#############################################################################################

        public void PerformClickWallpaper(int x, int y)
        {
            // TODO: 
        }

        //#############################################################################################

        private void Form_Wallpaper_Load(object sender, EventArgs e)
        {
            m_isFixed = BehindDesktopIcon.FixBehindDesktopIcon(this.Handle);

            if (m_isFixed)
            {
                using (var g = this.CreateGraphics())
                {
                    var location = Screen.PrimaryScreen.Bounds.Location;
                    var size = Screen.PrimaryScreen.Bounds.Size;

                    PointF dpi = new PointF(96.0f / g.DpiX, 96.0f / g.DpiY);

                    this.Location = new Point((int)(location.X * dpi.X),
                        (int)(location.Y * dpi.Y));
                    this.Size = new Size((int)(size.Width * dpi.X),
                        (int)(size.Height * dpi.Y));
                }
            }
            else
            {
                this.Close();
            }
        }
    }
}
