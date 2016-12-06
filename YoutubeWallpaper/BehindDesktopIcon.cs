using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace YoutubeWallpaper
{
    public static class BehindDesktopIcon
    {
        public static bool FixBehindDesktopIcon(IntPtr formHandle)
        {
            IntPtr progman = WinApi.FindWindow("Progman", null);
            
            if (progman == IntPtr.Zero)
                return false;


            IntPtr workerw = IntPtr.Zero;
            
            // 여러번 시도함.
            for (int step = 0; step < 8; ++step)
            {
                // 한번씩은 건너뜀.
                if (step % 2 == 0)
                {
                    IntPtr result = IntPtr.Zero;
                    WinApi.SendMessageTimeout(progman,
                        0x052C,
                        new IntPtr(0),
                        IntPtr.Zero,
                        WinApi.SendMessageTimeoutFlags.SMTO_NORMAL,
                        10000,
                        out result);
                }


                WinApi.EnumWindows(new WinApi.EnumWindowsProc((tophandle, topparamhandle) =>
                {
                    IntPtr p = WinApi.FindWindowEx(tophandle,
                        IntPtr.Zero,
                        "SHELLDLL_DefView",
                        IntPtr.Zero);

                    if (p != IntPtr.Zero)
                    {
                        workerw = WinApi.FindWindowEx(IntPtr.Zero,
                            tophandle,
                            "WorkerW",
                            IntPtr.Zero);
                    }

                    return true;
                }), IntPtr.Zero);


                if (workerw == IntPtr.Zero)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    break;
                }
            }

            if (workerw == IntPtr.Zero)
                return false;


            WinApi.ShowWindow(workerw, 0/*HIDE*/);
            WinApi.SetParent(formHandle, progman);


            return true;
        }
    }
}
