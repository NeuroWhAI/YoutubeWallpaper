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
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;
using CefSharp;
using CefSharp.WinForms;

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
                                Process.Start(@"https://neurowhai.tistory.com/370");


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
                catch (Exception e)
                {
#if DEBUG
                    throw;
#else
                    MessageBox.Show("알 수 없는 이유로 업데이트 확인에 실패하였습니다.\n" + e.Message, "Warning!",
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
        }

        protected void ShowController()
        {
            this.notifyIcon_tray.Visible = false;

            this.Show();
        }

        //#########################################################################################################

        protected void PlayWallpaper()
        {
            // https://developers.google.com/youtube/player_parameters 참조


            StopWallpaper();


            m_wallpaper = new Form_Wallpaper(m_option.ScreenIndex);
            m_wallpaper.Shuffle = m_option.Shuffle;
            m_wallpaper.Volume = m_option.Volume;
            SetOverlayJob(m_option.JobWhenOverlayed);
            m_wallpaper.Show();


            if (m_wallpaper.IsFixed)
            {
                m_wallpaper.IsPlaylist = (m_option.IdType == Option.Type.Playlist);
                m_wallpaper.VideoId = m_option.Id;
            }
            else
            {
                MessageBox.Show("배경화면을 설정할 수 없습니다.", "Error!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                StopWallpaper();
            }
        }

        protected void ToggleWallpaper()
        {
            if (m_wallpaper != null)
            {
                if (m_wallpaper.Paused)
                {
                    m_wallpaper.PlayVideo();
                }
                else
                {
                    m_wallpaper.PauseVideo();
                }
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

        protected void StopWallpaper()
        {
            if (m_wallpaper != null)
            {
                m_wallpaper.Close();
                m_wallpaper.Dispose();
                m_wallpaper = null;
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
            if (!url.StartsWith("http"))
            {
                return;
            }

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

            this.checkBox_shuffle.Checked = m_option.Shuffle;

            this.textBox_id.Text = m_option.Id;

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

            m_option.Shuffle = this.checkBox_shuffle.Checked;

            m_option.Id = this.textBox_id.Text;

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
            var cefSettings = new CefSettings();
            cefSettings.CefCommandLineArgs["autoplay-policy"] = "no-user-gesture-required";

            Cef.Initialize(cefSettings);


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
            StopWallpaper();


            Cef.Shutdown();


            RestoreAeroPeek();


            this.notifyIcon_tray.Visible = false;
        }

        //#########################################################################################################

        private void button_apply_Click(object sender, EventArgs e)
        {
            SaveOption();


            PlayWallpaper();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            SaveOption();
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

        private void ToolStripMenuItem_toggleWallpaper_Click(object sender, EventArgs e)
        {
            ToggleWallpaper();
        }

        private void ToolStripMenuItem_mute_Click(object sender, EventArgs e)
        {
            MuteWallpaper();
        }

        private void ToolStripMenuItem_nextScreen_Click(object sender, EventArgs e)
        {
            NextScreen();
        }

        private void ToolStripMenuItem_stopWallpaper_Click(object sender, EventArgs e)
        {
            StopWallpaper();
        }

        //#########################################################################################################

        private void ToolStripMenuItem_openBlog_Click(object sender, EventArgs e)
        {
            Process.Start(@"https://neurowhai.tistory.com/370");
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

        private void ToolStripMenuItem_toggleWallpaperInTray_Click(object sender, EventArgs e)
        {
            ToggleWallpaper();
        }

        private void ToolStripMenuItem_muteInTray_Click(object sender, EventArgs e)
        {
            MuteWallpaper();
        }

        private void ToolStripMenuItem_stopWallpaperInTray_Click(object sender, EventArgs e)
        {
            StopWallpaper();
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

        //#########################################################################################################

        private void radioButton_type_list_CheckedChanged(object sender, EventArgs e)
        {
            this.checkBox_shuffle.Enabled = this.radioButton_type_list.Checked;
        }

        private void checkBox_shuffle_CheckedChanged(object sender, EventArgs e)
        {
            m_option.Shuffle = this.checkBox_shuffle.Checked;

            if (m_wallpaper != null)
            {
                PlayWallpaper();
            }
        }
    }
}
