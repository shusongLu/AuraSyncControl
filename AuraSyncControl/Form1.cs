using AuraServiceLib;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AuraSyncControl
{
    public partial class Form1 : Form
    {

        // 导入必要的 Windows API 函数
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

        [DllImport("user32.dll")]
        public static extern bool UnregisterPowerSettingNotification(IntPtr Handle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // 定义消息常量
        const uint WM_SYSCOMMAND = 0x0112;
        const uint SC_MONITORPOWER = 0xF170;

        // 定义电源设置的GUID，显示器电源状态的GUID
        private Guid GUID_MONITOR_POWER_ON = new Guid("02731015-4510-4526-99E6-E5A17EBD1AEA");


        private IntPtr notificationHandle;


        private ContextMenuStrip contextMenu;
        IAuraSdk2 AuraSdk;
        private NotifyIcon notifyIcon;
        public Form1()
        {
            InitializeComponent();
            AuraSdk = (IAuraSdk2)new AuraSdk();

            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("关闭屏幕", null, CloseScreen_Click);
            contextMenu.Items.Add("显示灯光", null, (_, _) => { OpenRGB(); });
            contextMenu.Items.Add("关闭灯光", null, (_, _) => { CloseRGB(); });
            contextMenu.Items.Add("显示主页面", null, Show_Click);
            contextMenu.Items.Add("退出", null, Exit_Click);


            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
 
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // 设置托盘图标
                Visible = true,
                BalloonTipTitle = "提示",
                BalloonTipText = "应用程序已最小化到托盘",
                Text = "华硕神光同步息屏灯光控制"
            };

            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.DoubleClick += (_, _) =>
            {

                Show();
                WindowState = FormWindowState.Normal;// 显示窗体

            };

            
            Resize += MainForm_Resize; //最小化事件
        }

        /// <summary>
        /// 关闭灯光,让所有灯光显示为透明色
        /// </summary>
        private void CloseRGB()
        {
            AuraSdk.SwitchMode();
            var device = AuraSdk.Enumerate(0);
            foreach (IAuraSyncDevice dev in device)
            {
                foreach (IAuraRgbLight devLight in dev.Lights)
                {
                    devLight.Color = 0x00000000;
                }
                dev.Apply();
            }

        }

        /// <summary>
        /// 打开灯光 释放控制
        /// </summary>
        private void OpenRGB()
        {
            AuraSdk.ReleaseControl(0x00000000);
        }

        private void Show_Click(object? sender, EventArgs e)
        {
            // 显示窗体
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            // 退出应用程序
            Application.Exit();
        }

        private void CloseScreen_Click(object? sender, EventArgs e)
        {
            SendMessage(this.Handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 向全局窗口句柄发送消息，关闭显示器
            SendMessage(this.Handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)2);
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            // 当窗体被最小化时，隐藏窗体并显示通知图标
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 注册显示器电源状态变化的通知
            notificationHandle = RegisterPowerSettingNotification(this.Handle, ref GUID_MONITOR_POWER_ON, 0);
            if (notificationHandle == IntPtr.Zero)
            {
                MessageBox.Show("注册电源状态通知失败");
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_POWERBROADCAST = 0x0218;
            const int PBT_POWERSETTINGCHANGE = 0x8013;

            if (m.Msg == WM_POWERBROADCAST && m.WParam.ToInt32() == PBT_POWERSETTINGCHANGE)
            {
                var ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(POWERBROADCAST_SETTING))!;
                if (ps.PowerSetting == GUID_MONITOR_POWER_ON)
                {
                    // 0 表示显示器关闭，1 表示显示器开启
                    if (ps.Data == 0)
                    {
                        Debug.WriteLine("显示器已关闭");
                        CloseRGB();


                    }
                    else
                    {
                        Debug.WriteLine("显示器已开启");
                        OpenRGB();
                    }
                }
            }

            base.WndProc(ref m);
        }



        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }
    }
}
