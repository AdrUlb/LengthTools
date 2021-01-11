using LengthTools.Common;
using System;
using System.Diagnostics;
using System.IO;

namespace LengthTools
{
	class Program
	{
		public static readonly string? AsmPath = Process.GetCurrentProcess().MainModule?.FileName;
		public static readonly string? AsmName = Path.GetFileName(AsmPath ?? "{assembly path here}");

		public static readonly FileVersionInfo? info = AsmPath == null ? null : FileVersionInfo.GetVersionInfo(AsmPath);
		public static readonly string Name = info?.ProductName ?? "{Product name here}";
		public static readonly string Version = info?.ProductVersion ?? "{Product version here}";
		public static readonly string Copyright = info?.LegalCopyright ?? "{Copyright string here}";

		public static readonly ConsoleColor errorColor = ConsoleColor.Red;

		static void FatalError(string error)
		{
			Console.Write($"{AsmName}: ");
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
			Console.Write($"{AsmName}: ");
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

			var usageStr = parser.GenerateUsageString(AsmName!);
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
						Console.WriteLine($"{Name} {Version}");
						Console.WriteLine(Copyright);
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
						FatalError("unknown option: {option}");
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
		}
	}
}