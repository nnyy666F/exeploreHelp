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
			String exe_version = "�汾 0.2.1";

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
						// ǿ��ɾ���ļ�
						if (arg == "/r" && main_arg == "-d") 
						{
							continue;
						}

						// ��ȡ�ļ�����ʱ��
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
						Console.WriteLine("��ʾ�ļ�ʱ��");
						
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
								Console.WriteLine($"����ʱ�䣺{fileInfo.CreationTime}��ʱ��������룩��{createTimeStampInMs}��");
							}
							else if (mode == 1)
							{
								Console.WriteLine($"�޸�ʱ�䣺{fileInfo.LastWriteTime}��ʱ��������룩��{lastWriteTimeStampInMs}��");
							}
							else if (mode == 2)
							{
								Console.WriteLine($"����ʱ�䣺{fileInfo.LastAccessTime}��ʱ��������룩��{lastAccessTimeStampInMs}��");
							} 
							else
							{
								Console.WriteLine($"����ʱ�䣺{fileInfo.CreationTime}��ʱ��������룩��{createTimeStampInMs}��\n�޸�ʱ�䣺{fileInfo.LastWriteTime}��ʱ��������룩��{lastWriteTimeStampInMs}��\n����ʱ�䣺{fileInfo.LastAccessTime}��ʱ��������룩��{lastAccessTimeStampInMs}��");
							}
						}
						else
						{
							Console.WriteLine("�ļ�������");
						}
						break;
					case "-i":
						Console.WriteLine("��ʾ�ļ���Ϣ");
						try
						{
							// ���������ļ�·����������������ļ��ڵ�ǰĿ¼��
							string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigMain.config");
							using (StreamWriter writer = new StreamWriter(configFilePath, false))
							{
								writer.WriteLine(file_path);
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"д���ļ�·���������ļ����ִ���: {ex.Message}");
						}
						Application.Run(new Form1());
						break;
					case "-a":
						Console.WriteLine($"�����ļ� {file_path} ��λ��");
						try
						{
							ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe");
							startInfo.Arguments = $"/select,\"{file_path}\"";
							if (!File.Exists(file_path))
							{
								Console.WriteLine("�����ҵ��ļ�������......");
							}
							Process.Start(startInfo);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
						break;
					case "-tt":
						Console.WriteLine("�޸��ļ�ʱ��");
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
									// ����Ŀ��ʱ���ַ���
									DateTime targetDateTime = DateTime.ParseExact(cmd_2, "yyyy-MM-dd - HH:mm:ss", null);					

									// ���ô���ʱ�䡢�޸�ʱ��ͷ���ʱ��ΪĿ��ʱ��
									fileInfo.CreationTime = targetDateTime;
									fileInfo.LastWriteTime = targetDateTime;
									fileInfo.LastAccessTime = targetDateTime;
								}
								else if (mode == 0)
								{

									// ����ʱ������Ժ���Ϊ��λ��1���� = 10000��100���룩
									if (long.TryParse(cmd_2, out long targetTimeStamp))
									{
										// �������ʱ�������ĳ���ϴ��ֵ�������Ժ���Ϊ��λ�ĵ�ǰʱ���ʱ�����һ���ϴ�����������Ϊ�����뼶���
										long currentTimeStampInMs = DateTime.UtcNow.ToFileTimeUtc() / 10000;
										if (targetTimeStamp > currentTimeStampInMs * 1000000)
										{
											// ��Ϊ�����뼶��ת��Ϊ���뼶��
											targetTimeStamp /= 10000;
										}
										fileInfo.LastWriteTimeUtc = DateTime.FromFileTimeUtc(targetTimeStamp * 10000);
									}
									else
									{
										Console.WriteLine("ʱ�����ʽ��Ч");
									}

								}

								Console.WriteLine($"�ѳɹ����ļ� {file_path} ��ʱ���޸�Ϊ {fileInfo.LastWriteTime}");
							}
							catch (Exception e)
							{
								Console.WriteLine($"�޸��ļ�ʱ��ʱ����: {e.Message}");
							}
						}
						else
						{
							Console.WriteLine("�ļ�������");
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
								// �����Ƿ���Ҫǿ��ɾ������ /r ����)
								if (isForceDelete)
								{
									Console.WriteLine($"����ɾ�� {file_path}");
									DeleteFileOrDirectoryUsingSHFileOperation(file_path, 1);
								}
								else
								{
									Console.WriteLine($"�ƶ�������վ {file_path}");
									DeleteFileOrDirectoryUsingSHFileOperation(file_path);
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine($"ɾ������ʧ��: {ex.Message}");
							}
						}
						else
						{
							Console.WriteLine("�ļ����ļ��в����ڣ�");
						}
						break;
					default:
						Console.WriteLine("δ֪����");
						break;
				}

				Thread.Sleep(2000);
				System.Environment.Exit(0);
			}
			else
			{
				Console.WriteLine("explorerHelp - " + exe_version);
				Console.WriteLine();
				Console.WriteLine("�����в���˵����");
				Console.WriteLine("  -v   ��ʾ�汾��Ϣ");
				Console.WriteLine("  -t   ��ʾ�ļ�ʱ�䣨��ͨ����Ӷ�������鿴�ض�ʱ�䣬�� /ct�鿴����ʱ�䡢/wt�鿴�޸�ʱ�䡢/at�鿴����ʱ�䣩");
				Console.WriteLine("  -a   �����ļ�λ��");
				Console.WriteLine("  -tt  �޸��ļ�ʱ�䣨��ͨ����� /fs ������ʱ�����ʽ�޸��ļ�����޸�ʱ�䣬Ҳ�ɰ�ָ������ʱ���ʽ�޸��ļ��������޸ġ�����ʱ�䣩");
				Console.WriteLine("  -d   ɾ���ļ�������� /r ��������ǿ��ɾ����������ɾ����");
				Console.WriteLine();
				Console.WriteLine("ʹ�÷���ʾ����");
				Console.WriteLine("  explorerHelp -v            # ��ʾ����汾��Ϣ");
				Console.WriteLine("  explorerHelp -t <�ļ�·��> [/ct|/wt|/at]  # ��ʾ�ļ���Ӧ�Ĵ������޸Ļ����ʱ��");
				Console.WriteLine("  explorerHelp -a <�ļ�·��>  # �����ļ���λ��");
				Console.WriteLine("  explorerHelp -tt <�ļ�·��> [/fs <ʱ���������ʱ��>] # �޸��ļ���ʱ�����ָ��ʱ���޸��ļ�ʱ��");
				Console.WriteLine("  explorerHelp -d <�ļ�·��> [/r]  # ɾ��ָ���ļ�����ѡ���Ƿ�ǿ��ɾ����");
				Console.WriteLine();
				Console.WriteLine("��лʹ�ã�");

				System.Environment.Exit(0);
			}
		}

		// SHFileOperation ɾ���ļ����ļ���
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
		��ʾ����ui��ɾ������
		
		*/
		const uint FOF_ALLOWUNDO = 0x0040; // ֧�ֻ���վ
		//�ƶ�������վ
		private static void DeleteFileOrDirectoryUsingSHFileOperation(string path)
		{
			SHFILEOPSTRUCT fileOp = new SHFILEOPSTRUCT
			{
				wFunc = FO_DELETE,
				pFrom = path + "\0\0",  // ʹ��˫���ַ���β
				pTo = null,
				fFlags = (ushort)(/*FOF_SILENT | FOF_NOCONFIRMATION | */FOF_ALLOWUNDO),
				lpszProgressTitle = "ɾ���ļ�/�ļ���..."
			};

			int result = SHFileOperation(ref fileOp);
			if (result != 0)
			{
				Console.WriteLine($"ɾ������ʧ�ܣ��������: {result}");
			}
			else
			{
				Console.WriteLine($"�ļ�/�ļ����ѳɹ�ɾ��: {path}");
			}
		}
		//����ɾ���ļ����ļ���
		private static void DeleteFileOrDirectoryUsingSHFileOperation(string path,int mode)
		{
			SHFILEOPSTRUCT fileOp = new SHFILEOPSTRUCT
			{
				wFunc = FO_DELETE,
				pFrom = path + "\0\0",  // ʹ��˫���ַ���β
				pTo = null,
				//fFlags = (ushort)(/*FOF_SILENT | FOF_NOCONFIRMATION | */FOF_ALLOWUNDO),
				lpszProgressTitle = "ɾ���ļ�/�ļ���..."
			};

			int result = SHFileOperation(ref fileOp);
			if (result != 0)
			{
				Console.WriteLine($"ɾ������ʧ�ܣ��������: {result}");
			}
			else
			{
				Console.WriteLine($"�ļ�/�ļ����ѳɹ�ɾ��: {path}");
			}
		}
	}
}