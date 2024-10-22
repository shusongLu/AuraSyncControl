using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AuraSyncControl
{
    internal static class Program
    {
        // ���廥����
        static Mutex mutex = new Mutex(true, "{��˶���ͬ��Ϣ���ƹ����-ShuSong}");

        // ����User32���е�API��������ʾ����ʵ������
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd); // ��鴰���Ƿ���С��

        const int SW_RESTORE = 9; // ���ڻ�ԭ��С���Ĵ���

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // ����Ƿ��Ѿ�������һ��ʵ��
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());

                // �ͷŻ�����
                mutex.ReleaseMutex();
            }
            else
            {
                // �Ѿ���ʵ��������
                MessageBox.Show("������������");
            }
        }
    }
}