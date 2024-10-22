using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AuraSyncControl
{
    internal static class Program
    {
        // 定义互斥量
        static Mutex mutex = new Mutex(true, "{华硕神光同步息屏灯光控制-ShuSong}");

        // 导入User32库中的API，用于显示现有实例窗口
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd); // 检查窗口是否最小化

        const int SW_RESTORE = 9; // 用于还原最小化的窗口

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 检查是否已经运行了一个实例
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());

                // 释放互斥量
                mutex.ReleaseMutex();
            }
            else
            {
                // 已经有实例在运行
                MessageBox.Show("程序已在运行");
            }
        }
    }
}