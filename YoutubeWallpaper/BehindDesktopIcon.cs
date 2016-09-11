using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace YoutubeWallpaper
{
    public class BehindDesktopIcon
    {
        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x20
        }

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint Msg, IntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        //######################################################################################################################
        
        public static bool FixBehindDesktopIcon(IntPtr formHandle)
        {
            IntPtr progman = FindWindow("Progman", null);


            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(progman,
                0x052C,
                new IntPtr(0),
                IntPtr.Zero,
                SendMessageTimeoutFlags.SMTO_NORMAL,
                1000,
                out result);


            IntPtr workerw = IntPtr.Zero;

            EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = FindWindowEx(tophandle,
                    IntPtr.Zero,
                    "SHELLDLL_DefView",
                    IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    workerw = FindWindowEx(IntPtr.Zero,
                        tophandle,
                        "WorkerW",
                        IntPtr.Zero);
                }

                return true;
            }), IntPtr.Zero);
            

            if (workerw != IntPtr.Zero)
            {
                ShowWindow(workerw, 0/*HIDE*/);
                
                SetParent(formHandle, progman);
            }


            return workerw != IntPtr.Zero;
        }
    }
}
