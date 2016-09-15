using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace YoutubeWallpaper
{
    public partial class Form_Main : Form
    {
        public Form_Main()
        {
            InitializeComponent();
        }

        //#########################################################################################################

        protected readonly string AppName = "NeuroWhAI_YoutubeWallpaper";

        protected readonly string OptionFile = Path.Combine(Application.StartupPath, "Option.dat");
        protected Option m_option = new Option();

        protected Form_Wallpaper m_wallpaper = null;

        protected Form_Touchpad m_touchpad = null;

        //#########################################################################################################

        protected void HideController()
        {
            this.Hide();

            this.notifyIcon_tray.Visible = true;
        }

        protected void ShowController()
        {
            this.notifyIcon_tray.Visible = false;

            this.Show();
        }

        protected void ShowTouchPad()
        {
            if (m_touchpad == null || m_touchpad.IsDisposed)
            {
                m_touchpad = new Form_Touchpad();
                m_touchpad.Show();
            }
            else
            {
                m_touchpad.WindowState = FormWindowState.Normal;
                m_touchpad.Activate();
            }

            m_touchpad.Target = m_wallpaper;
        }

        //#########################################################################################################

        protected void PlayWallpaper()
        {
            StopWallpaper();


            StringBuilder url = new StringBuilder(@"https://www.youtube.com/");

            if (m_option.IdType == Option.Type.OneVideo)
                url.Append(@"v/");
            else if (m_option.IdType == Option.Type.Playlist)
                url.Append(@"embed?listType=playlist&index=1&list=");

            url.Append(m_option.Id);

            url.Append(@"&autoplay=1&loop=1&controls=0&showinfo=0&autohide=1&modestbranding=1&rel=0");

            if (m_option.IdType == Option.Type.OneVideo)
                url.Append(@"&version=2");

            url.Append("&vq=");

            string quality = "";
            switch (m_option.VideoQuality)
            {
                case Option.Quality.p240:
                    quality = "small";
                    break;

                case Option.Quality.p360:
                    quality = "medium";
                    break;

                case Option.Quality.p480:
                    quality = "large";
                    break;

                case Option.Quality.p720:
                    quality = "hd720";
                    break;

                case Option.Quality.p1080:
                    quality = "hd1080";
                    break;

                case Option.Quality.p1440:
                    quality = "hd1440";
                    break;
            }

            url.Append(quality);


            m_wallpaper = new Form_Wallpaper(m_option.ScreenIndex);
            m_wallpaper.Volume = m_option.Volume;
            m_wallpaper.Show();

            if (m_touchpad != null)
            {
                m_touchpad.Target = m_wallpaper;
            }

            if (m_wallpaper.IsFixed)
            {
                m_wallpaper.Uri = url.ToString();
            }
            else
            {
                MessageBox.Show("배경화면을 설정할 수 없습니다.", "Error!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                StopWallpaper();
            }
        }

        protected void StopWallpaper()
        {
            if (m_wallpaper != null)
            {
                m_wallpaper.Close();
                m_wallpaper.Dispose();
                m_wallpaper = null;
            }
        }

        protected void MuteWallpaper()
        {
            if (m_wallpaper != null)
            {
                m_wallpaper.Volume = 0;
                this.trackBar_volume.Value = 0;
            }
        }

        protected void NextScreen()
        {
            if (m_wallpaper != null)
            {
                m_wallpaper.OwnerScreenIndex++;
                m_option.ScreenIndex = m_wallpaper.OwnerScreenIndex;

                if (m_wallpaper.IsFixed == false)
                {
                    MessageBox.Show("배경화면을 설정할 수 없습니다.", "Error!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);


                    StopWallpaper();
                }
            }
        }

        //#########################################################################################################

        protected void ApplyOptionToUI()
        {
            switch (m_option.IdType)
            {
                case Option.Type.OneVideo:
                    this.radioButton_type_one.Checked = true;
                    break;

                case Option.Type.Playlist:
                    this.radioButton_type_list.Checked = true;
                    break;
            }

            this.textBox_id.Text = m_option.Id;

            switch (m_option.VideoQuality)
            {
                case Option.Quality.p240:
                    this.radioButton_q_small.Checked = true;
                    break;

                case Option.Quality.p360:
                    this.radioButton_q_medium.Checked = true;
                    break;

                case Option.Quality.p480:
                    this.radioButton_q_large.Checked = true;
                    break;

                case Option.Quality.p720:
                    this.radioButton_q_720.Checked = true;
                    break;

                case Option.Quality.p1080:
                    this.radioButton_q_1080.Checked = true;
                    break;

                case Option.Quality.p1440:
                    this.radioButton_q_1440.Checked = true;
                    break;
            }

            this.trackBar_volume.Value = m_option.Volume;
            if (m_wallpaper != null)
                m_wallpaper.Volume = m_option.Volume;
        }

        protected void LoadOption()
        {
            if (File.Exists(OptionFile))
            {
                m_option.LoadFromFile(OptionFile);
            }

            ApplyOptionToUI();
        }

        protected void SaveOption()
        {
            if (this.radioButton_type_one.Checked)
                m_option.IdType = Option.Type.OneVideo;
            else if (this.radioButton_type_list.Checked)
                m_option.IdType = Option.Type.Playlist;

            m_option.Id = this.textBox_id.Text;

            if (this.radioButton_q_small.Checked)
                m_option.VideoQuality = Option.Quality.p240;
            else if (this.radioButton_q_medium.Checked)
                m_option.VideoQuality = Option.Quality.p480;
            else if (this.radioButton_q_large.Checked)
                m_option.VideoQuality = Option.Quality.p720;
            else if (this.radioButton_q_720.Checked)
                m_option.VideoQuality = Option.Quality.p720;
            else if (this.radioButton_q_1080.Checked)
                m_option.VideoQuality = Option.Quality.p1080;
            else if (this.radioButton_q_1440.Checked)
                m_option.VideoQuality = Option.Quality.p1440;

            m_option.Volume = this.trackBar_volume.Value;


            m_option.SaveToFile(OptionFile);
        }

        //#########################################################################################################

        private void Form_Main_Load(object sender, EventArgs e)
        {
            LoadOption();


            // 시작프로그램 여부 알아내기
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                this.ToolStripMenuItem_startup.Checked = (key.GetValue(AppName) != null);
            }

            // 시작프로그램으로 등록되어있다면 저장된 정보로 자동 실행
            if (this.ToolStripMenuItem_startup.Checked)
            {
                Task.Factory.StartNew(new Action(() =>
                {
                    this.Invoke(new Action(() =>
                    {
                        System.Threading.Thread.Sleep(2500);

                        HideController();
                    }));
                }));

                PlayWallpaper();
            }
        }

        private void Form_Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.notifyIcon_tray.Visible = false;
        }

        //#########################################################################################################

        private void button_apply_Click(object sender, EventArgs e)
        {
            SaveOption();


            StopWallpaper();


            PlayWallpaper();
        }

        private void button_restore_Click(object sender, EventArgs e)
        {
            ApplyOptionToUI();
        }

        //#########################################################################################################

        private void ToolStripMenuItem_startup_Click(object sender, EventArgs e)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (this.ToolStripMenuItem_startup.Checked)
                {
                    key.SetValue(AppName, Application.ExecutablePath);
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }

                MessageBox.Show("Success!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ToolStripMenuItem_hideController_Click(object sender, EventArgs e)
        {
            HideController();
        }

        private void ToolStripMenuItem_exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //#########################################################################################################

        private void ToolStripMenuItem_openTouchpad_Click(object sender, EventArgs e)
        {
            ShowTouchPad();
        }

        private void ToolStripMenuItem_stopWallpaper_Click(object sender, EventArgs e)
        {
            StopWallpaper();
        }

        private void ToolStripMenuItem_mute_Click(object sender, EventArgs e)
        {
            MuteWallpaper();
        }

        private void ToolStripMenuItem_nextScreen_Click(object sender, EventArgs e)
        {
            NextScreen();
        }

        //#########################################################################################################

        private void ToolStripMenuItem_openBlog_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://blog.naver.com/neurowhai/220810470139");
        }

        //#########################################################################################################

        private void notifyIcon_tray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowController();
        }

        //#########################################################################################################

        private void ToolStripMenuItem_openController_Click(object sender, EventArgs e)
        {
            ShowController();
        }

        private void ToolStripMenuItem_openTouchpadInTray_Click(object sender, EventArgs e)
        {
            ShowTouchPad();
        }

        private void ToolStripMenuItem_stopWallpaperInTray_Click(object sender, EventArgs e)
        {
            StopWallpaper();
        }

        private void ToolStripMenuItem_muteInTray_Click(object sender, EventArgs e)
        {
            MuteWallpaper();
        }

        private void ToolStripMenuItem_exitInTray_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //#########################################################################################################

        private void trackBar_volume_Scroll(object sender, EventArgs e)
        {
            if (m_wallpaper != null)
            {
                m_wallpaper.Volume = this.trackBar_volume.Value;
            }
        }
    }
}
