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
				Console.WriteLine("从配置文件读取的文件路径无效！");
				return;
			}
			Console.WriteLine($"读取到配置文件中的文件路径: {filePath}");

			// 初始化 SHELLEXECUTEINFO
			SHELLEXECUTEINFO sei = new SHELLEXECUTEINFO();
			sei.cbSize = Marshal.SizeOf(sei);
			sei.lpVerb = "properties";                            
			sei.lpFile = filePath;
			sei.nShow = 5; // SW_SHOW
			sei.fMask = SEE_MASK_INVOKEIDLIST | 0x00000040;

			if (!ShellExecuteEx(ref sei))
			{
				Console.WriteLine("无法打开属性窗口！");
			}
			else
			{
				Console.WriteLine("属性窗口已打开，启动定时器检查窗口状态...");
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
				Console.WriteLine($"读取配置文件出现错误: {ex.Message}");
			}
			return "";
		}

		private void CheckWindowStatus(object state)
		{
			string windowTitle = System.IO.Path.GetFileName(filePath) + " 属性";
			IntPtr hwnd = FindWindow(null, windowTitle);

			if (hwnd == IntPtr.Zero)
			{
				Console.WriteLine("属性窗口已关闭，主程序退出。");
				Application.Exit();
			}
		}
	}
}