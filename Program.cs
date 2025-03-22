using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace exeploreHelp
{
	internal static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			ApplicationConfiguration.Initialize();
			int mode = -1;
			String exe_version = "版本 0.2.1";

			if (args.Length == 1)
			{
				if (args[0] == "-v")
				{
					Console.WriteLine(exe_version);
					System.Environment.Exit(0);
				}
			}
			else if (args.Length > 1)
			{
				string cmd = "";
				string cmd_2 = "";
				string main_arg = "";
				string file_path = "";
				int arg_count = 0;

				foreach (var arg in args)
				{
					cmd += arg + " ";
					if (arg_count == 0)
					{
						main_arg = arg;
					}
					else if (arg_count == 1)
					{
						// 强制删除文件
						if (arg == "/r" && main_arg == "-d") 
						{
							continue;
						}

						// 获取文件访问时间
						if (arg == "/ct" && main_arg == "-t")
						{
							continue;
						}
						if (arg == "/wt" && main_arg == "-t")
						{
							continue;
						}
						if (arg == "/at" && main_arg == "-t")
						{
							continue;
						}
						if (arg == "/fs" && main_arg == "-tt")
						{
							continue;
						}

						file_path = arg;
					}
					else if (arg_count == 2)
					{
						cmd_2 = arg;
					}

					arg_count++;
				}

				Console.WriteLine(cmd);
				switch (main_arg)
				{
					case "-t":
						Console.WriteLine("显示文件时间");
						
						if (cmd.Contains("/ct"))
						{
							mode = 0;
						} else if (cmd.Contains("/wt"))
						{
							mode = 1;
						} else if (cmd.Contains("/at"))
						{
							mode = 2;
						}

						if (File.Exists(file_path) && Path.GetExtension(file_path).Length > 0 || Directory.Exists(file_path))
						{
							FileInfo fileInfo = new FileInfo(file_path);
							long createTimeStampInMs = fileInfo.CreationTimeUtc.ToFileTimeUtc() / 10000;
							long lastWriteTimeStampInMs = fileInfo.LastWriteTimeUtc.ToFileTimeUtc() / 10000;
							long lastAccessTimeStampInMs = fileInfo.LastAccessTimeUtc.ToFileTimeUtc() / 10000;

							if (mode == 0)
							{
								Console.WriteLine($"创建时间：{fileInfo.CreationTime}（时间戳（毫秒）：{createTimeStampInMs}）");
							}
							else if (mode == 1)
							{
								Console.WriteLine($"修改时间：{fileInfo.LastWriteTime}（时间戳（毫秒）：{lastWriteTimeStampInMs}）");
							}
							else if (mode == 2)
							{
								Console.WriteLine($"访问时间：{fileInfo.LastAccessTime}（时间戳（毫秒）：{lastAccessTimeStampInMs}）");
							} 
							else
							{
								Console.WriteLine($"创建时间：{fileInfo.CreationTime}（时间戳（毫秒）：{createTimeStampInMs}）\n修改时间：{fileInfo.LastWriteTime}（时间戳（毫秒）：{lastWriteTimeStampInMs}）\n访问时间：{fileInfo.LastAccessTime}（时间戳（毫秒）：{lastAccessTimeStampInMs}）");
							}
						}
						else
						{
							Console.WriteLine("文件不存在");
						}
						break;
					case "-i":
						Console.WriteLine("显示文件信息");
						try
						{
							// 构建配置文件路径，这里假设配置文件在当前目录下
							string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigMain.config");
							using (StreamWriter writer = new StreamWriter(configFilePath, false))
							{
								writer.WriteLine(file_path);
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"写入文件路径到配置文件出现错误: {ex.Message}");
						}
						Application.Run(new Form1());
						break;
					case "-a":
						Console.WriteLine($"查找文件 {file_path} 的位置");
						try
						{
							ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe");
							startInfo.Arguments = $"/select,\"{file_path}\"";
							if (!File.Exists(file_path))
							{
								Console.WriteLine("待查找的文件不存在......");
							}
							Process.Start(startInfo);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
						break;
					case "-tt":
						Console.WriteLine("修改文件时间");
						if (cmd.Contains("/fs"))
						{
							mode = 0;
						}
						if (File.Exists(file_path) && Path.GetExtension(file_path).Length > 0 || Directory.Exists(file_path))
						{
							try
							{
								FileInfo fileInfo = new FileInfo(file_path);
								if (mode != 0)
								{
									// 解析目标时间字符串
									DateTime targetDateTime = DateTime.ParseExact(cmd_2, "yyyy-MM-dd - HH:mm:ss", null);					

									// 设置创建时间、修改时间和访问时间为目标时间
									fileInfo.CreationTime = targetDateTime;
									fileInfo.LastWriteTime = targetDateTime;
									fileInfo.LastAccessTime = targetDateTime;
								}
								else if (mode == 0)
								{

									// 更新时间戳（以毫秒为单位，1毫秒 = 10000个100纳秒）
									if (long.TryParse(cmd_2, out long targetTimeStamp))
									{
										// 假设如果时间戳大于某个较大的值（比如以毫秒为单位的当前时间的时间戳的一个较大倍数），则认为是纳秒级别的
										long currentTimeStampInMs = DateTime.UtcNow.ToFileTimeUtc() / 10000;
										if (targetTimeStamp > currentTimeStampInMs * 1000000)
										{
											// 认为是纳秒级别，转换为毫秒级别
											targetTimeStamp /= 10000;
										}
										fileInfo.LastWriteTimeUtc = DateTime.FromFileTimeUtc(targetTimeStamp * 10000);
									}
									else
									{
										Console.WriteLine("时间戳格式无效");
									}

								}

								Console.WriteLine($"已成功将文件 {file_path} 的时间修改为 {fileInfo.LastWriteTime}");
							}
							catch (Exception e)
							{
								Console.WriteLine($"修改文件时间时出错: {e.Message}");
							}
						}
						else
						{
							Console.WriteLine("文件不存在");
						}
						break;
					case "-d":
						if (File.Exists(file_path) && Path.GetExtension(file_path).Length > 0 || Directory.Exists(file_path))
						{
							try
							{
								bool isForceDelete = false;
								if (cmd.Contains("/r"))
								{
									isForceDelete = true;
								}
								// 根据是否需要强制删除（有 /r 参数)
								if (isForceDelete)
								{
									Console.WriteLine($"永久删除 {file_path}");
									DeleteFileOrDirectoryUsingSHFileOperation(file_path, 1);
								}
								else
								{
									Console.WriteLine($"移动到回收站 {file_path}");
									DeleteFileOrDirectoryUsingSHFileOperation(file_path);
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine($"删除操作失败: {ex.Message}");
							}
						}
						else
						{
							Console.WriteLine("文件或文件夹不存在！");
						}
						break;
					default:
						Console.WriteLine("未知命令");
						break;
				}

				Thread.Sleep(2000);
				System.Environment.Exit(0);
			}
			else
			{
				Console.WriteLine("explorerHelp - " + exe_version);
				Console.WriteLine();
				Console.WriteLine("命令行参数说明：");
				Console.WriteLine("  -v   显示版本信息");
				Console.WriteLine("  -t   显示文件时间（可通过添加额外参数查看特定时间，如 /ct查看创建时间、/wt查看修改时间、/at查看访问时间）");
				Console.WriteLine("  -a   查找文件位置");
				Console.WriteLine("  -tt  修改文件时间（可通过添加 /fs 参数以时间戳方式修改文件最后修改时间，也可按指定日期时间格式修改文件创建、修改、访问时间）");
				Console.WriteLine("  -d   删除文件（可添加 /r 参数进行强制删除，即永久删除）");
				Console.WriteLine();
				Console.WriteLine("使用方法示例：");
				Console.WriteLine("  explorerHelp -v            # 显示程序版本信息");
				Console.WriteLine("  explorerHelp -t <文件路径> [/ct|/wt|/at]  # 显示文件对应的创建、修改或访问时间");
				Console.WriteLine("  explorerHelp -a <文件路径>  # 查找文件的位置");
				Console.WriteLine("  explorerHelp -tt <文件路径> [/fs <时间戳或日期时间>] # 修改文件的时间戳或按指定时间修改文件时间");
				Console.WriteLine("  explorerHelp -d <文件路径> [/r]  # 删除指定文件（可选择是否强制删除）");
				Console.WriteLine();
				Console.WriteLine("感谢使用！");

				System.Environment.Exit(0);
			}
		}

		// SHFileOperation 删除文件或文件夹
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct SHFILEOPSTRUCT
		{
			public IntPtr hwnd;
			public uint wFunc;
			[MarshalAs(UnmanagedType.LPTStr)] public string pFrom;
			[MarshalAs(UnmanagedType.LPTStr)] public string pTo;
			public ushort fFlags;
			public ushort fAnyOperationsAborted;
			public IntPtr hNameMappings;
			[MarshalAs(UnmanagedType.LPTStr)] public string lpszProgressTitle;
		}

		const uint FO_DELETE = 0x0003;
		const uint FOF_NOCONFIRMATION = 0x0010;
		const uint FOF_SILENT = 0x0004;
		/*
		FOF_SILENT | FOF_NOCONFIRMATION |
		显示出来ui和删除进度
		
		*/
		const uint FOF_ALLOWUNDO = 0x0040; // 支持回收站
		//移动到回收站
		private static void DeleteFileOrDirectoryUsingSHFileOperation(string path)
		{
			SHFILEOPSTRUCT fileOp = new SHFILEOPSTRUCT
			{
				wFunc = FO_DELETE,
				pFrom = path + "\0\0",  // 使用双空字符结尾
				pTo = null,
				fFlags = (ushort)(/*FOF_SILENT | FOF_NOCONFIRMATION | */FOF_ALLOWUNDO),
				lpszProgressTitle = "删除文件/文件夹..."
			};

			int result = SHFileOperation(ref fileOp);
			if (result != 0)
			{
				Console.WriteLine($"删除操作失败，错误代码: {result}");
			}
			else
			{
				Console.WriteLine($"文件/文件夹已成功删除: {path}");
			}
		}
		//永久删除文件或文件夹
		private static void DeleteFileOrDirectoryUsingSHFileOperation(string path,int mode)
		{
			SHFILEOPSTRUCT fileOp = new SHFILEOPSTRUCT
			{
				wFunc = FO_DELETE,
				pFrom = path + "\0\0",  // 使用双空字符结尾
				pTo = null,
				//fFlags = (ushort)(/*FOF_SILENT | FOF_NOCONFIRMATION | */FOF_ALLOWUNDO),
				lpszProgressTitle = "删除文件/文件夹..."
			};

			int result = SHFileOperation(ref fileOp);
			if (result != 0)
			{
				Console.WriteLine($"删除操作失败，错误代码: {result}");
			}
			else
			{
				Console.WriteLine($"文件/文件夹已成功删除: {path}");
			}
		}
	}
}