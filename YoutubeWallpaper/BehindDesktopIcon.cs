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


            if (progman == IntPtr.Zero)
                return false;


            IntPtr result = IntPtr.Zero;
            WinApi.SendMessageTimeout(progman,
                0x052C,
                new IntPtr(0),
                IntPtr.Zero,
                WinApi.SendMessageTimeoutFlags.SMTO_NORMAL,
                10000,
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


            WinApi.SetParent(formHandle, progman);


            if (workerw == IntPtr.Zero)
            {
                // defView가 progman에 있는 상황이거나 그 외.

                WinApi.SetWindowPos(formHandle, new IntPtr(1)/*HWND_BOTTOM*/, 0, 0, 0, 0,
                    WinApi.SetWindowPosFlags.SWP_NOSIZE | WinApi.SetWindowPosFlags.SWP_NOMOVE);
            }
            else
            {
                WinApi.ShowWindow(workerw, 0/*HIDE*/);
            }


            return true;
        }
    }
}
