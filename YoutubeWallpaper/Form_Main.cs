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
using System.Net;
using System.Diagnostics;

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

        protected bool m_wasAero = true;

        //#########################################################################################################

        protected void ApplyAeroPeek()
        {
            if (WinApi.IsCompositionEnabled(out m_wasAero)
                && m_wasAero == false)
            {
                WinApi.EnableComposition(WinApi.CompositionAction.DWM_EC_ENABLECOMPOSITION);
            }
            else
            {
                m_wasAero = true;
            }
        }

        protected void RestoreAeroPeek()
        {
            if (m_wasAero == false)
            {
                WinApi.EnableComposition(WinApi.CompositionAction.DWM_EC_DISABLECOMPOSITION);
            }
        }

        //#########################################################################################################

        protected bool CheckUpdate()
        {
            using (var client = new WebClient())
            {
                try
                {
                    var info = client.DownloadString(@"https://raw.githubusercontent.com/NeuroWhAI/YoutubeWallpaper/master/YoutubeWallpaper/Properties/AssemblyInfo.cs");

                    int begin = info.LastIndexOf("AssemblyVersion");
                    if (begin >= 0)
                    {
                        begin = info.IndexOf('\"', begin) + 1;
                        int end = info.IndexOf('\"', begin);

                        string version = info.Substring(begin, end - begin);

                        if (Version.Parse(version) > Version.Parse(Application.ProductVersion))
                        {
                            string message = $@"새로운 버전이 확인되었습니다!
종료하고 다운로드 페이지를 여시겠습니까?
최신 버전 : {version}
현재 버전 : {Application.ProductVersion}";

                            if (MessageBox.Show(message, "Update",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                            {
                                Process.Start(@"http://blog.naver.com/neurowhai/220810470139");


                                Application.Exit();


                                return true;
                            }
                        }
                    }
                }
                catch (WebException)
                {
                    MessageBox.Show("업데이트 확인에 실패하였습니다.\n인터넷 연결을 확인하세요.", "Warning!",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception)
                {
#if DEBUG
                    throw;
#else
                    MessageBox.Show("알 수 없는 이유로 업데이트 확인에 실패하였습니다.", "Warning!",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                }
            }


            return false;
        }

        //#########################################################################################################

        protected void HideController()
        {
            this.Hide();

            this.notifyIcon_tray.Visible = true;

            this.notifyIcon_tray.ShowBalloonTip(1000, "Youtube Wallpaper", "Click me to open.", ToolTipIcon.None);
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
            // https://developers.google.com/youtube/player_parameters 참조


            StopWallpaper();


            StringBuilder url = new StringBuilder(@"https://www.youtube.com/");

            if (m_option.IdType == Option.Type.OneVideo)
            {
                url.Append(@"v/");
                url.Append(m_option.Id);
                url.Append(@"?playlist=");
                url.Append(m_option.Id);
            }
            else if (m_option.IdType == Option.Type.Playlist)
            {
                // embed 태그가 원하는대로 작동하지 않으니 v 태그를 사용하되
                // 아주 짧은 영상(Q3HPz3h-_AE)을 먼저 재생하고 자동으로 다음에 재생될 영상을
                // 유저가 설정한 재생목록으로 해두면 v 태그로 재생목록도 재생 가능!
                url.Append(@"v/Q3HPz3h-_AE?listType=playlist&index=0&list=");
                url.Append(m_option.Id);
            }

            // TODO: controls를 1로 하면 귀찮더라도 이전/다음 영상으로 넘어가는 조작이 가능함.
            url.Append(@"&autoplay=1&loop=1&controls=0&showinfo=0&autohide=1&modestbranding=1&rel=0&iv_load_policy=3&playsinline=0&cc_load_policy=0&version=3");

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
            SetOverlayJob(m_option.JobWhenOverlayed);
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

        protected void TogglePlayWallpaper()
        {
            if (m_wallpaper != null)
            {
                m_wallpaper.TogglePlay();

                // 안하면 월페이퍼가 포커스를 가져서 조작이 이상해짐.
                this.Select();
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
            this.trackBar_volume.Value = 0;

            if (m_wallpaper != null)
            {
                m_wallpaper.Volume = 0;
            }
        }

        protected void NextScreen()
        {
            if (m_wallpaper != null)
            {
                // 화면 전환
                m_wallpaper.OwnerScreenIndex++;

                // 전환에 성공하였으면 설정을 저장하고
                // 그렇지 않으면 정지.
                if (m_wallpaper.IsFixed)
                {
                    m_option.ScreenIndex = m_wallpaper.OwnerScreenIndex;
                    m_option.SaveToFile(OptionFile);
                }
                else
                {
                    MessageBox.Show("배경화면을 설정할 수 없습니다.", "Error!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    StopWallpaper();
                }
            }
        }

        //#########################################################################################################

        protected string GetValueInUrl(string url, string valueName)
        {
            int nameIndex = url.IndexOf(valueName);
            if (nameIndex >= 0)
            {
                int beginSub = nameIndex + valueName.Length;

                int endIndex = url.IndexOf('&', beginSub);
                if (endIndex >= 0)
                {
                    return url.Substring(beginSub, endIndex - beginSub);
                }
                else
                {
                    return url.Substring(beginSub);
                }
            }

            return string.Empty;
        }

        protected void ApplyOptionFromYoutubeUrl(string url)
        {
            string listId = GetValueInUrl(url, "list=");
            if (string.IsNullOrEmpty(listId) == false)
            {
                this.radioButton_type_list.Checked = true;
                this.textBox_id.Text = listId;
            }
            else
            {
                string oneId = GetValueInUrl(url, "v=");
                if (string.IsNullOrEmpty(oneId) == false)
                {
                    this.radioButton_type_one.Checked = true;
                    this.textBox_id.Text = oneId;
                }
            }
        }

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

            switch (m_option.JobWhenOverlayed)
            {
                case Option.Job.Nothing:
                    this.radioButton_nothingWhenOverlayed.Checked = true;
                    break;

                case Option.Job.Mute:
                    this.radioButton_muteWhenOverlayed.Checked = true;
                    break;

                case Option.Job.Toggle:
                    this.radioButton_toggleWhenOverlayed.Checked = true;
                    break;
            }
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

            if (this.radioButton_nothingWhenOverlayed.Checked)
                m_option.JobWhenOverlayed = Option.Job.Nothing;
            else if (this.radioButton_muteWhenOverlayed.Checked)
                m_option.JobWhenOverlayed = Option.Job.Mute;
            else if (this.radioButton_toggleWhenOverlayed.Checked)
                m_option.JobWhenOverlayed = Option.Job.Toggle;


            m_option.SaveToFile(OptionFile);
        }

        protected void SetOverlayJob(Option.Job job)
        {
            if (m_wallpaper != null)
            {
                m_wallpaper.AutoMute = (job == Option.Job.Mute);
                m_wallpaper.AutoTogglePlay = (job == Option.Job.Toggle);
            }
        }

        //#########################################################################################################

        private void Form_Main_Load(object sender, EventArgs e)
        {
            ApplyAeroPeek();


            Task.Factory.StartNew(CheckUpdate);


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
            RestoreAeroPeek();


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

        private void toolStripMenuItem_togglePlayWallpaper_Click(object sender, EventArgs e)
        {
            TogglePlayWallpaper();
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
            Process.Start(@"http://blog.naver.com/neurowhai/220810470139");
        }

        //#########################################################################################################

        private void notifyIcon_tray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowController();
        }

        private void notifyIcon_tray_BalloonTipClicked(object sender, EventArgs e)
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

        private void ToolStripMenuItem_togglePlayWallpaperInTray_Click(object sender, EventArgs e)
        {
            TogglePlayWallpaper();
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

        //#########################################################################################################
        
        private void textBox_id_TextChanged(object sender, EventArgs e)
        {
            ApplyOptionFromYoutubeUrl(this.textBox_id.Text);
        }

        //#########################################################################################################

        private void radioButton_nothingWhenOverlayed_CheckedChanged(object sender, EventArgs e)
        {
            SetOverlayJob(Option.Job.Nothing);
        }

        private void radioButton_muteWhenOverlayed_CheckedChanged(object sender, EventArgs e)
        {
            SetOverlayJob(Option.Job.Mute);
        }

        private void radioButton_toggleWhenOverlayed_CheckedChanged(object sender, EventArgs e)
        {
            SetOverlayJob(Option.Job.Toggle);
        }
    }
}
