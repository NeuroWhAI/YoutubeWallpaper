using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YoutubeWallpaper
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.IO.Directory.SetCurrentDirectory(Application.StartupPath);


            // DPI 호환을 포기함.
            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    WinApi.SetProcessDPIAware();
                }
            }
            catch (EntryPointNotFoundException)
            {
                // OS가 해당 API를 지원하지 않으므로 무시.
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new Form_Main());
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
