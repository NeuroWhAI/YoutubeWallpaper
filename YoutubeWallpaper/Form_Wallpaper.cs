using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using CefSharp;
using CefSharp.WinForms;

namespace YoutubeWallpaper
{
    public partial class Form_Wallpaper : Form
    {
        public Form_Wallpaper(int ownerScreenIndex = 0)
        {
            InitializeComponent();


            this.webBrowser_page = new ChromiumWebBrowser(new CefSharp.Web.HtmlString(EmptyHtml));
            this.Controls.Add(this.webBrowser_page);
            this.webBrowser_page.Dock = DockStyle.Fill;
            this.webBrowser_page.FrameLoadEnd += WebBrowser_page_FrameLoadEnd;


            OwnerScreenIndex = ownerScreenIndex;


            PinToBackground();
        }

        private void WebBrowser_page_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            // 바로 Load를 호출하면 작동이 안되니까
            // 기본 시작 페이지 로드가 일부 완료되었다면 목표 URI를 Load함.

            this.webBrowser_page.FrameLoadEnd -= WebBrowser_page_FrameLoadEnd;

            if (!string.IsNullOrEmpty(VideoId))
            {
                var screen = OwnerScreen.rcMonitor;

                string html = Properties.Resources.Template;
                html = html.Replace("WALLPAPER_HEIGHT", screen.Height.ToString());
                html = html.Replace("WALLPAPER_WIDTH", screen.Width.ToString());

                if (IsPlaylist)
                {
                    html = html.Replace("PLAYLIST_ID", VideoId);
                    html = html.Replace("VIDEO_ID", string.Empty);
                }
                else
                {
                    html = html.Replace("PLAYLIST_ID", string.Empty);
                    html = html.Replace("VIDEO_ID", VideoId);
                }

                this.webBrowser_page.LoadHtml(html);

                Paused = false;
            }
        }

        //#############################################################################################

        private readonly string EmptyHtml = "<!doctype html><html><head><title>...</title></head><body></body></html>";

        private ChromiumWebBrowser webBrowser_page;

        private bool m_isFixed = false;
        public bool IsFixed
        { get { return m_isFixed; } }

        private string m_videoId;
        public string VideoId
        {
            get { return m_videoId; }
            set { m_videoId = value; }
        }

        public bool IsPlaylist { get; set; } = false;

        private int m_latestVolume = 100;
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
                m_latestVolume = value;

                uint vol = (uint)((double)0xFFFF * value / 100) & 0xFFFF;
                WinApi.waveOutSetVolume(IntPtr.Zero, (vol << 16) | vol);
            }
        }

        private int m_ownerScreenIndex = 0;
        public int OwnerScreenIndex
        {
            get { return m_ownerScreenIndex; }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value >= Screen.AllScreens.Length)
                    value = 0;

                if (m_ownerScreenIndex != value)
                {
                    m_ownerScreenIndex = value;
                    
                    PinToBackground();
                }
            }
        }
        public WinApi.MONITORINFO OwnerScreen
        {
            get
            {
                if (OwnerScreenIndex < ScreenUtility.Screens.Length)
                    return ScreenUtility.Screens[OwnerScreenIndex];
                return new WinApi.MONITORINFO()
                {
                    rcMonitor = Screen.PrimaryScreen.Bounds,
                    rcWork = Screen.PrimaryScreen.WorkingArea,
                };
            }
        }

        public bool AutoMute
        { get; set; } = false;

        public bool AutoTogglePlay
        { get; set; } = true;

        private Task m_checkParent = null;
        private bool m_onRunning = false;
        private EventWaitHandle m_waitHandle = null;

        private readonly object m_lockFlag = new object();
        private bool m_needUpdate = false;

        private bool m_wasOverlayed = false;

        public bool Paused { get; private set; } = false;

        //#############################################################################################

        public void PauseVideo()
        {
            this.webBrowser_page.ExecuteScriptAsync("pauseVideo", new object[] { });
            Paused = true;
        }

        public void PlayVideo()
        {
            this.webBrowser_page.ExecuteScriptAsync("playVideo", new object[] { });
            Paused = false;
        }

        //#############################################################################################

        protected bool PinToBackground()
        {
            m_isFixed = BehindDesktopIcon.FixBehindDesktopIcon(this.Handle);

            if (m_isFixed)
            {
                ScreenUtility.FillScreen(this, OwnerScreen);
            }


            return m_isFixed;
        }

        protected void CheckParent(object thisHandle)
        {
            IntPtr me = (IntPtr)thisHandle;


            while (m_onRunning)
            {
                bool isChildOfProgman = false;


                var progman = WinApi.FindWindow("Progman", null);

                WinApi.EnumChildWindows(progman, new WinApi.EnumWindowsProc((handle, lparam) =>
                {
                    if (handle == me)
                    {
                        isChildOfProgman = true;
                        return false;
                    }

                    return true;
                }), IntPtr.Zero);


                if (isChildOfProgman == false)
                {
                    lock (m_lockFlag)
                    {
                        m_needUpdate = true;
                    }
                }


                m_waitHandle.WaitOne(2000);
            }
        }

        //#############################################################################################

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            lock (m_lockFlag)
            {
                m_needUpdate = true;
            }
        }

        private void Form_Wallpaper_Load(object sender, EventArgs e)
        {
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;


            // 생성자에서 배경에 고정되었을테니 DPI의 영향에서 벗어난다.
            // 이때 모니터 정보들을 다시 구하면 DPI의 영향을 받지 않는 해상도가 나온다.
            ScreenUtility.Initialize();


            // 그렇게 구해진 올바른 해상도로 다시 배경에 고정한다.
            if (PinToBackground())
            {
                m_waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                m_onRunning = true;
                m_checkParent = Task.Factory.StartNew(CheckParent, this.Handle);


                this.timer_check.Start();
            }
            else
            {
                this.Close();
            }
        }

        private void Form_Wallpaper_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;


            this.timer_check.Stop();


            if (m_checkParent != null)
            {
                m_onRunning = false;
                m_waitHandle.Set();
                m_checkParent.Wait(TimeSpan.FromSeconds(10.0));
                m_checkParent = null;

                m_waitHandle.Dispose();
            }


            this.webBrowser_page.LoadHtml(EmptyHtml);
        }

        private void timer_check_Tick(object sender, EventArgs e)
        {
            bool needUpdate = false;
            lock (m_lockFlag)
            {
                needUpdate = m_needUpdate;
                m_needUpdate = false;
            }

            if (needUpdate)
            {
                PinToBackground();
            }


            // 배경이 다른 프로그램에 의해 가려졌고
            if (ScreenUtility.IsOverlayed(this))
            {
                // 이번이 처음으로 가려진거면
                if (m_wasOverlayed == false)
                {
                    m_wasOverlayed = true;

                    if (this.AutoMute)
                    {
                        // 음소거
                        WinApi.waveOutSetVolume(IntPtr.Zero, 0);
                    }

                    if (this.AutoTogglePlay)
                    {
                        // 일시정지
                        PauseVideo();
                    }
                }
            }
            else if (m_wasOverlayed)
            {
                // 가려지지 않았고 이전에 가려진적이 있었으면

                m_wasOverlayed = false;

                // 볼륨 복구
                this.Volume = m_latestVolume;

                if (this.AutoTogglePlay)
                {
                    // 재생
                    PlayVideo();
                }
            }
        }
    }
}
