using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ThreadingTimer = System.Threading.Timer;

namespace exeploreHelp
{
	public partial class Form1 : Form
	{ 
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct SHELLEXECUTEINFO
		{
			public int cbSize;
			public uint fMask;
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.LPTStr)] public string lpVerb;
			[MarshalAs(UnmanagedType.LPTStr)] public string lpFile;
			[MarshalAs(UnmanagedType.LPTStr)] public string lpParameters;
			[MarshalAs(UnmanagedType.LPTStr)] public string lpDirectory;
			public int nShow;
			public IntPtr hInstApp;
			public IntPtr lpIDList;
			[MarshalAs(UnmanagedType.LPTStr)] public string lpClass;
			public IntPtr hkeyClass;
			public uint dwHotKey;
			public IntPtr hIcon;
			public IntPtr hProcess;
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CloseHandle(IntPtr hObject);

		const int SEE_MASK_INVOKEIDLIST = 0x0000000C;
		const uint INFINITE = 0xFFFFFFFF;
		private string filePath;
		private ThreadingTimer checkWindowTimer;

		public Form1()
		{
			this.ShowInTaskbar = false;
			this.WindowState = FormWindowState.Minimized;
			Thread.Sleep(1000);
			filePath = ReadFilePathFromConfig();
			if (string.IsNullOrWhiteSpace(filePath))
			{
				Console.WriteLine("�������ļ���ȡ���ļ�·����Ч��");
				return;
			}
			Console.WriteLine($"��ȡ�������ļ��е��ļ�·��: {filePath}");

			// ��ʼ�� SHELLEXECUTEINFO
			SHELLEXECUTEINFO sei = new SHELLEXECUTEINFO();
			sei.cbSize = Marshal.SizeOf(sei);
			sei.lpVerb = "properties";                            
			sei.lpFile = filePath;
			sei.nShow = 5; // SW_SHOW
			sei.fMask = SEE_MASK_INVOKEIDLIST | 0x00000040;

			if (!ShellExecuteEx(ref sei))
			{
				Console.WriteLine("�޷������Դ��ڣ�");
			}
			else
			{
				Console.WriteLine("���Դ����Ѵ򿪣�������ʱ����鴰��״̬...");
				Thread.Sleep(5000);
				checkWindowTimer = new ThreadingTimer(CheckWindowStatus, null, 0, 1000);
			}
		}

		private string ReadFilePathFromConfig()
		{
			try
			{
				string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigMain.config");
				if (File.Exists(configFilePath))
				{
					return File.ReadAllText(configFilePath).Trim();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"��ȡ�����ļ����ִ���: {ex.Message}");
			}
			return "";
		}

		private void CheckWindowStatus(object state)
		{
			string windowTitle = System.IO.Path.GetFileName(filePath) + " ����";
			IntPtr hwnd = FindWindow(null, windowTitle);

			if (hwnd == IntPtr.Zero)
			{
				Console.WriteLine("���Դ����ѹرգ��������˳���");
				Application.Exit();
			}
		}
	}
}