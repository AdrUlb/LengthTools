using LengthTools.Common;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace LengthTools
{
	class Program
	{
		static readonly string? asmPath = Process.GetCurrentProcess().MainModule?.FileName;
		static readonly string? asmName = Path.GetFileName(asmPath ?? "{assembly path here}");
		static readonly string? asmDir = Path.GetDirectoryName(asmPath);
		
		public static readonly FileVersionInfo? info = asmPath == null ? null : FileVersionInfo.GetVersionInfo(asmPath);
		public static readonly string name = info?.ProductName ?? "{Product name here}";
		public static readonly string version = info?.ProductVersion ?? "{Product version here}";
		public static readonly string copyright = info?.LegalCopyright ?? "{Copyright string here}";

		public static readonly ConsoleColor errorColor = ConsoleColor.Red;

		static void FatalError(string error)
		{
			Console.Write($"{asmName}: ");
			var normalColor = Console.ForegroundColor;
			Console.ForegroundColor = errorColor;
			Console.Write("fatal error: ");
			Console.ForegroundColor = normalColor;
			Console.WriteLine(error);
			Console.WriteLine("compilation terminated.");
			Environment.Exit(-1);
		}

		static void Error(string error)
		{
			Console.Write($"{asmName}: ");
			var normalColor = Console.ForegroundColor;
			Console.ForegroundColor = errorColor;
			Console.Write("error: ");
			Console.ForegroundColor = normalColor;
			Console.WriteLine(error);
		}

		static void Main(string[] args)
		{
			var knownOptions = new CommandLineOption[]
			{
				new CommandLineOption("help", "Display this information."),
				new CommandLineOption("version", "Display version information."),
				new CommandLineOption("il", "Compile to intermediate code"),
				new CommandLineOption("output", "Specify the output file", false, "output_file")
			};

			var knownOptionlessArgs = new CommandLineArgument[]
			{
				new CommandLineArgument("input_file", "Input source file")
			};

			CommandLineParser parser = new CommandLineParser(args, knownOptions, knownOptionlessArgs, 1);

			var usageStr = parser.GenerateUsageString(asmName!);
			var helpStr = parser.GenerateHelpString(25);

			string? outputFile = null;

			var argIl = false;

			while (parser.GetNextArg(out var option, out var arg))
			{
				switch (option)
				{
					case "help":
						Console.WriteLine(usageStr);
						Console.WriteLine();
						Console.WriteLine(helpStr);
						Environment.Exit(0);
						break;
					case "version":
						Console.WriteLine($"{name} {version}");
						Console.WriteLine(copyright);
						Environment.Exit(0);
						break;
					case "output":
						if (arg == null)
							FatalError($"option requires an argument: {option}");
						outputFile = arg;
						break;
					case "il":
						argIl = true;
						break;
					case "":
						FatalError("no name provided for argument");
						Environment.Exit(-1);
						break;
					default:
						FatalError($"unknown option: {option}");
						Environment.Exit(-1);
						break;
				}
			}

			var opts = parser.Args;

			if (opts.Count > 1)
				FatalError("too many input files");
			else if (opts.Count < 1)
				FatalError("no input file");

			var inputFile = opts[0];

			if (Directory.Exists(inputFile))
			{
				Error($"{inputFile}: is a directory");
			}

			if (!File.Exists(inputFile))
			{
				Error($"{inputFile}: no such file or directory");
				FatalError("no input file");
			}

			if (outputFile == null)
			{
				FatalError("no output file");
				Console.WriteLine("compilation terminated.");
				Environment.Exit(-1);
			}

			if (!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(outputFile))))
			{
				FatalError("target directory does not exist");
			}

			var lines = File.ReadAllLines(inputFile);

			var il = LengthCompiler.LengthToIntermediate(lines);

			if (argIl)
			{
				File.WriteAllLines(outputFile, il); 
				Environment.Exit(0);
			}

			var byteCode = LengthCompiler.IntermediateToByteCode(il);

			string runtimeBin = "";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				runtimeBin = Path.Join(asmDir, "lengthrt.exe");
			else
				FatalError("compilation to native binary not supported on this platform");

			if (!File.Exists(runtimeBin))
				FatalError("runtime binary missing");

			File.Copy(runtimeBin, outputFile, true);

			using var fs = new FileStream(outputFile, FileMode.Append);

			foreach (var b in byteCode)
				fs.Write(byteCode, 0, byteCode.Length);

			Span<byte> lengthBytes = stackalloc byte[4];

			BinaryPrimitives.WriteInt32BigEndian(lengthBytes, byteCode.Length);
			fs.Write(lengthBytes);
			fs.WriteByte(0xAB);
		}
	}
}