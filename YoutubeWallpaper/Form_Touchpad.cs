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
    public partial class Form_Touchpad : Form
    {
        public Form_Touchpad()
        {
            InitializeComponent();
        }

        //########################################################################################################

        protected Form_Wallpaper m_target = null;
        public Form_Wallpaper Target
        {
            get { return m_target; }
            set
            {
                m_target = value;

                if (value != null)
                {
                    var size = value.OwnerScreen.rcMonitor.Size;
                    this.ClientSize = new Size(size.Width / 4, size.Height / 4);
                }
            }
        }

        //########################################################################################################

        protected Point CursorToTarget(Point cursor)
        {
            return new Point((int)(cursor.X * ((double)m_target.Size.Width / this.ClientSize.Width)),
                (int)(cursor.Y * ((double)m_target.Size.Height / this.ClientSize.Height)));
        }

        //########################################################################################################

        private void Form_Touchpad_Load(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void Form_Touchpad_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_target != null && !m_target.IsDisposed)
                {
                    var cursor = CursorToTarget(e.Location);
                    m_target.PerformClickWallpaper(cursor.X, cursor.Y);

                    this.Activate();
                }
            }
        }

        private void Form_Touchpad_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                case Keys.Enter:
                    this.Close();
                    break;
            }
        }

        private void Form_Touchpad_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_target != null && !m_target.IsDisposed)
            {
                var cursor = CursorToTarget(e.Location);
                m_target.MoveCursor(cursor.X, cursor.Y);
            }
        }

        private void Form_Touchpad_MouseEnter(object sender, EventArgs e)
        {
            if (m_target != null && !m_target.IsDisposed)
            {
                m_target.ShowCursor(true);
            }
        }

        private void Form_Touchpad_MouseLeave(object sender, EventArgs e)
        {
            if (m_target != null && !m_target.IsDisposed)
            {
                m_target.ShowCursor(false);
            }
        }

        private void Form_Touchpad_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_target != null && !m_target.IsDisposed)
            {
                m_target.ShowCursor(false);
            }
        }
    }
}
