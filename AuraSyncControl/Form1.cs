using AuraServiceLib;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AuraSyncControl
{
    public partial class Form1 : Form
    {

        // �����Ҫ�� Windows API ����
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

        [DllImport("user32.dll")]
        public static extern bool UnregisterPowerSettingNotification(IntPtr Handle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // ������Ϣ����
        const uint WM_SYSCOMMAND = 0x0112;
        const uint SC_MONITORPOWER = 0xF170;

        // �����Դ���õ�GUID����ʾ����Դ״̬��GUID
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
            contextMenu.Items.Add("�ر���Ļ", null, CloseScreen_Click);
            contextMenu.Items.Add("��ʾ�ƹ�", null, (_, _) => { OpenRGB(); });
            contextMenu.Items.Add("�رյƹ�", null, (_, _) => { CloseRGB(); });
            contextMenu.Items.Add("��ʾ��ҳ��", null, Show_Click);
            contextMenu.Items.Add("�˳�", null, Exit_Click);


            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
 
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // ��������ͼ��
                Visible = true,
                BalloonTipTitle = "��ʾ",
                BalloonTipText = "Ӧ�ó�������С��������",
                Text = "��˶���ͬ��Ϣ���ƹ����"
            };

            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.DoubleClick += (_, _) =>
            {

                Show();
                WindowState = FormWindowState.Normal;// ��ʾ����

            };

            
            Resize += MainForm_Resize; //��С���¼�
        }

        /// <summary>
        /// �رյƹ�,�����еƹ���ʾΪ͸��ɫ
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
        /// �򿪵ƹ� �ͷſ���
        /// </summary>
        private void OpenRGB()
        {
            AuraSdk.ReleaseControl(0x00000000);
        }

        private void Show_Click(object? sender, EventArgs e)
        {
            // ��ʾ����
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            // �˳�Ӧ�ó���
            Application.Exit();
        }

        private void CloseScreen_Click(object? sender, EventArgs e)
        {
            SendMessage(this.Handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // ��ȫ�ִ��ھ��������Ϣ���ر���ʾ��
            SendMessage(this.Handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)2);
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            // �����屻��С��ʱ�����ش��岢��ʾ֪ͨͼ��
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // ע����ʾ����Դ״̬�仯��֪ͨ
            notificationHandle = RegisterPowerSettingNotification(this.Handle, ref GUID_MONITOR_POWER_ON, 0);
            if (notificationHandle == IntPtr.Zero)
            {
                MessageBox.Show("ע���Դ״̬֪ͨʧ��");
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
                    // 0 ��ʾ��ʾ���رգ�1 ��ʾ��ʾ������
                    if (ps.Data == 0)
                    {
                        Debug.WriteLine("��ʾ���ѹر�");
                        CloseRGB();


                    }
                    else
                    {
                        Debug.WriteLine("��ʾ���ѿ���");
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
