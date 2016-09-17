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

namespace YoutubeWallpaper
{
    public partial class Form_Wallpaper : Form
    {
        public Form_Wallpaper(int ownerScreenIndex = 0)
        {
            InitializeComponent();


            OwnerScreenIndex = ownerScreenIndex;


            PinToBackground();
        }

        //#############################################################################################

        protected bool m_isFixed = false;
        public bool IsFixed
        { get { return m_isFixed; } }

        public string Uri
        {
            get { return this.webBrowser_page.Url.ToString(); }
            set
            {
                this.webBrowser_page.Navigate(value);

                UpdatePlayerHandle();
            }
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

        protected IntPtr m_playerHandle = IntPtr.Zero;
        protected IntPtr PlayerHandle
        {
            get
            {
                if (m_playerHandle != IntPtr.Zero)
                    return m_playerHandle;

                return UpdatePlayerHandle();
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
        public Screen OwnerScreen
        {
            get
            {
                if (OwnerScreenIndex < Screen.AllScreens.Length)
                    return Screen.AllScreens[OwnerScreenIndex];
                return Screen.PrimaryScreen;
            }
        }

        protected Task m_checkParent = null;
        protected bool m_onRunning = false;
        protected EventWaitHandle m_waitHandle = null;

        protected readonly object m_lockFlag = new object();
        protected bool m_needUpdate = false;

        //#############################################################################################

        protected IntPtr UpdatePlayerHandle()
        {
            IntPtr flash = IntPtr.Zero;
            flash = WinApi.FindWindowEx(this.webBrowser_page.Handle, IntPtr.Zero, "Shell Embedding", IntPtr.Zero);
            flash = WinApi.FindWindowEx(flash, IntPtr.Zero, "Shell DocObject View", IntPtr.Zero);
            flash = WinApi.FindWindowEx(flash, IntPtr.Zero, "Internet Explorer_Server", IntPtr.Zero);
            flash = WinApi.FindWindowEx(flash, IntPtr.Zero, "MacromediaFlashPlayerActiveX", IntPtr.Zero);

            m_playerHandle = flash;


            return flash;
        }

        public void ShowCursor(bool bShow)
        {
            this.panel_cursor.Visible = bShow;
        }

        public void MoveCursor(int x, int y)
        {
            this.panel_cursor.Location = new Point(x - this.panel_cursor.Width / 2,
                y - this.panel_cursor.Height / 2);
        }

        public void PerformClickWallpaper(int x, int y)
        {
            IntPtr flash = this.PlayerHandle;
            if (flash != IntPtr.Zero)
            {
                IntPtr result = IntPtr.Zero;

                // 첫번째 클릭에서 포커스를 잠깐 얻고
                // 두번째 클릭에서 실제로 클릭이 처리되게 하게끔 함.
                // TODO: 아래 방법은 개선되어야 함.
                for (int step = 0; step < 2; ++step)
                {
                    WinApi.SendMessageTimeout(flash, 0x201/*DOWN*/, new IntPtr(1), new IntPtr(WinApi.MakeParam(y, x)),
                        WinApi.SendMessageTimeoutFlags.SMTO_NORMAL, 0, out result);
                    WinApi.SendMessageTimeout(flash, 0x202/*UP*/, new IntPtr(1), new IntPtr(WinApi.MakeParam(y, x)),
                        WinApi.SendMessageTimeoutFlags.SMTO_NORMAL, 0, out result);
                }
            }
        }

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
        }
    }
}
