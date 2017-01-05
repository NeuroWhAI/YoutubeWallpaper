using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace YoutubeWallpaper
{
    public static class ScreenUtility
    {
        public static void FillScreen(Form form, Screen screen)
        {
            form.Location = screen.Bounds.Location;
            form.Size = screen.Bounds.Size;
        }

        public static bool IsOverlayed(Form form)
        {
            /// 전체화면이더라도 무시할 클래스명 목록
            string[] classNamesExcluded =
            {
                "WorkerW",                              // 배경화면
                "ProgMan",
                "ImmersiveLauncher",                    // Win8 시작화면
            };


            // 최상위 컨트롤의 핸들을 얻는다.
            IntPtr foregroundWindow = WinApi.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return false;
            foregroundWindow = WinApi.GetAncestor(foregroundWindow, WinApi.GetAncestorFlags.GetRoot);


            // 자기 자신이면 가려진게 아님.
            if (foregroundWindow == form.Handle)
                return false;


            // 컨트롤의 클래스이름을 얻어옵니다.
            string className = WinApi.GetClassName(foregroundWindow);
            if (className.Length <= 0)
                return false;


            // 제외목록에서 하나라도 해당되는지 확인.
            if (classNamesExcluded.Any((s) => s.ToUpper() == className.ToUpper()))
                return false;


            // 현재 컨트롤이 속하는 모니터의 사각영역을 얻어옵니다.
            WinApi.RECT desktop;
            IntPtr monitor = WinApi.MonitorFromWindow(form.Handle, WinApi.MONITOR_DEFAULTTONEAREST);
            if (monitor == IntPtr.Zero)
            {
                // 모니터를 찾을 수 없으면 현재 윈도우 화면의 핸들로 설정한다.
                IntPtr desktopWnd = WinApi.GetDesktopWindow();
                if (desktopWnd == IntPtr.Zero)
                    return false;

                if (WinApi.GetWindowRect(desktopWnd, out desktop) == false)
                    return false;
            }
            else
            {
                var info = new WinApi.MONITORINFO();
                info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
                if (WinApi.GetMonitorInfo(monitor, ref info) == false)
                    return false;

                desktop = info.rcMonitor;
            }


            // 컨트롤의 작업영역을 알아낸다.
            WinApi.RECT client;
            if (WinApi.GetWindowRect(foregroundWindow, out client) == false)
                return false;


            // 컨트롤이 모니터에 완전히 들어오지 않았거나 크기보다 작으면 전체화면이 아니다.
            if (client.Left > desktop.Left + 1
                ||
                client.Top > desktop.Top + 1
                ||
                client.Right < desktop.Right - 1
                ||
                client.Bottom < desktop.Bottom - 1)
                return false;


            // 여기까지 도달했으면 전체화면이다.
            return true;
        }
    }
}
