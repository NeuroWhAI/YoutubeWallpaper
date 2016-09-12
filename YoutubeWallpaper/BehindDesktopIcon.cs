using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeWallpaper
{
    public class BehindDesktopIcon
    {
        public static bool FixBehindDesktopIcon(IntPtr formHandle)
        {
            IntPtr progman = WinApi.FindWindow("Progman", null);


            IntPtr result = IntPtr.Zero;
            WinApi.SendMessageTimeout(progman,
                0x052C,
                new IntPtr(0),
                IntPtr.Zero,
                WinApi.SendMessageTimeoutFlags.SMTO_NORMAL,
                1000,
                out result);


            IntPtr workerw = IntPtr.Zero;

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
            

            if (workerw != IntPtr.Zero)
            {
                WinApi.ShowWindow(workerw, 0/*HIDE*/);

                WinApi.SetParent(formHandle, progman);
            }


            return workerw != IntPtr.Zero;
        }
    }
}
