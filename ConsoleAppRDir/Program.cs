using System;
using System.IO;
using System.Text;

namespace ConsoleAppRDir
{
	class Program
	{
		class _file_info
		{
			public string name;
			public long size;

			public _file_info(string Name, long Size)
			{
				name = Name;
				size = Size;
			}
		}

		class _dir_info
		{
			public string path;
			public string name;
			public int deep = 0;
			public _dir_info[] sub_dirs = null;
			public _file_info[] files = null;

			public long total_size = 0;

			public _dir_info(string Root)
			{
				path = Root;
				name = Path.GetFileName(Root);
			}

			_dir_info(string Root, int Deep)
			{
				path = Root;
				name = Path.GetFileName(Root);

				deep = Deep;
			}

			public static void FillDirs(ref _dir_info dir_info)
			{
				string[] directories = Directory.GetDirectories(dir_info.path);
				string[] files = Directory.GetFiles(dir_info.path);

				dir_info.files = new _file_info[files.Length];

				long sum_file_size = 0;

				for(int i = 0; i < files.Length; ++i)
				{
					string file = files[i];
					FileInfo info = new FileInfo(file);

					sum_file_size += info.Length;

					dir_info.files[i] = new _file_info(Path.GetFileName(file), info.Length);
				}

				dir_info.total_size = sum_file_size;
				
				dir_info.sub_dirs = new _dir_info[directories.Length];

				for(int i = 0; i < directories.Length; ++i)
				{
					string directory = directories[i];

					dir_info.sub_dirs[i] = new _dir_info(directory, dir_info.deep + 1);

					FillDirs(ref dir_info.sub_dirs[i]);

					dir_info.total_size += dir_info.sub_dirs[i].total_size;
				}
			}
		}

		static bool IsQuite = false;
		static bool IsHumanread = false;
		static FileStream LogFs = null;

		static void log(string str)
		{
			if(!IsQuite){
				Console.WriteLine(str);
			}
			else{
				Byte[] buff = new UTF8Encoding(true).GetBytes(str + "\n");
				LogFs.Write(buff, 0, buff.Length);
			}
		}

		static void print_dirs(_dir_info dir_info)
		{
			string size_format(long size)
			{
				if(size < 1024)
				{
					return string.Format("{0} b", size);
				}
				else if(size < 1048576)
				{
					return string.Format("{0:0.00} Kb", size / 1024.0f);
				}
				else if(size < 1073741824)
				{
					return string.Format("{0:0.00} Mb", size / 1048576.0f);
				}
				else if(size < 1099511627776)
				{
					return string.Format("{0:0.00} Gb", size / 1073741824.0f);
				}
				else
				{
					return string.Format("{0:0.00} Tb", size / 1099511627776.0f);
				}
			}

			string lines = "-";
			for(int i = 0; i < dir_info.deep; ++i) lines += "-";

			log(lines + dir_info.name + " " + (IsHumanread ? size_format(dir_info.total_size) : dir_info.total_size.ToString() + " b"));

			foreach(_file_info file_info in dir_info.files)
			{
				log(lines + file_info.name + " " + (IsHumanread ? size_format(file_info.size) : file_info.size.ToString() + " b"));
			}

			foreach(_dir_info sub_dir in dir_info.sub_dirs)
			{
				print_dirs(sub_dir);
			}
		}

		static void Main(string[] args)
		{
			string TargetPath = ".";
			string LogFile = null;
			
			for(int i = 0; i < args.Length; ++i)
			{
				switch(args[i])
				{
					case "-p":
						if(i < args.Length - 1){
							TargetPath = args[++i];
						}
						break;

					case "-o":
						if(i < args.Length - 1)
						{
							LogFile = args[++i];
						}
						break;

					case "-q":
						IsQuite = true;
						break;

					case "-h":
						IsHumanread = true;
						break;
				}
			}

			Console.WriteLine("Target path:\n\t" + TargetPath);

			if(IsQuite){

				if(LogFile == null){
					DateTime date_time = DateTime.Now;
					LogFile = @".\" + date_time.ToString("yyyy-MM-dd") + ".txt";
				}
				
				LogFs = File.OpenWrite(LogFile);

				Console.WriteLine("Log out:\n\t" + LogFile);
			}

			_dir_info dir_info = new _dir_info(TargetPath);

			_dir_info.FillDirs(ref dir_info);

			print_dirs(dir_info);

			if(IsQuite) LogFs.Close();

			Console.WriteLine("ok");
		}
	}
}
