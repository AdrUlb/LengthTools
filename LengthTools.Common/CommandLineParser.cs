using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LengthTools.Common
{
    public class CommandLineOption
    {
        public string Option { get; }
        public string HelpDescription { get; }
        public bool Optional { get; }
        public string? ArgumentName { get; }

        public CommandLineOption(string option, string helpDescription, bool optional = true, string? argumentName = null)
        {
            Option = option;
            HelpDescription = helpDescription;
            Optional = optional;
            ArgumentName = argumentName;
        }
    }

    public class CommandLineArgument
    {
        public string ArgumentName { get; }
        public string HelpDescription { get; }

        public CommandLineArgument(string argumentName, string helpDescription)
        {
            ArgumentName = argumentName;
            HelpDescription = helpDescription;
        }
    }

    public class CommandLineParser
    {
        private List<string> args;
        public IReadOnlyList<string> Args => args;

        private List<CommandLineOption> optionArgs = new List<CommandLineOption>();
        private CommandLineArgument[] optionlessArgs;
        private int requiredOptionlessArgs;

        const string prefix = "--";

        public CommandLineParser(string[] args, CommandLineOption[] knownOptions, CommandLineArgument[] knownOptionlessArgs, int requiredOptionlessArgs)
        {
            this.args = args.ToList();

            foreach (var arg in knownOptions)
            {
                switch (arg.Option.Length)
                {
                    case 0:
                        throw new ArgumentOutOfRangeException("Possible command line argument values have to be at least one character long.");
                    default:
                        optionArgs.Add(arg);
                        break;
                }
            }

            optionlessArgs = knownOptionlessArgs;
            this.requiredOptionlessArgs = requiredOptionlessArgs;
        }

        public string GenerateUsageString(string asmName)
        {
            var sb = new StringBuilder("usage: ");
            sb.Append(asmName);

            foreach (var arg in optionArgs)
            {
                sb.Append(' ');
                if (arg.Optional)
                    sb.Append('[');
                sb.Append(prefix);
                sb.Append(arg.Option);

                if (arg.ArgumentName != null)
                {
                    sb.Append(' ');
                    sb.Append(arg.ArgumentName);
                }

                if (arg.Optional)
                    sb.Append(']');
            }

            for (var i = 0; i < optionlessArgs.Length; i++)
            {
                sb.Append(' ');

                if (i >= requiredOptionlessArgs)
                    sb.Append('[');

                sb.Append(optionlessArgs[i].ArgumentName);

                if (i >= requiredOptionlessArgs)
                    sb.Append(']');
            }

            return sb.ToString();
        }

        public string GenerateHelpString(int descriptionOffset = 20, int nameOffset = 2)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < optionlessArgs.Length; i++)
            {
                var sbLine = new StringBuilder();

                for (var j = 0; j < nameOffset; j++)
                    sbLine.Append(' ');

                sbLine.Append(optionlessArgs[i].ArgumentName);

                sbLine.Append(' ');

                while (sbLine.Length < descriptionOffset)
                    sbLine.Append(' ');

                sbLine.Append(optionlessArgs[i].HelpDescription);
                sbLine.Append('\n');
                sb.Append(sbLine);
            }

            foreach (var arg in optionArgs)
            {
                var sbLine = new StringBuilder();

                for (var j = 0; j < nameOffset; j++)
                    sbLine.Append(' ');

                sbLine.Append(prefix);
                sbLine.Append(arg.Option);

                if (arg.ArgumentName != null)
                {
                    sbLine.Append(' ');
                    sbLine.Append(arg.ArgumentName);
                }

                sbLine.Append(' ');

                while (sbLine.Length < descriptionOffset)
                    sbLine.Append(' ');

                sbLine.Append(arg.HelpDescription);
                sbLine.Append('\n');

                sb.Append(sbLine);
            }

            return sb.ToString();
        }

        public bool GetNextArg(out string option, out string? argument)
        {
            var argi = 0;
        Try:
            if (argi == args.Count)
            {
                option = "";
                argument = null;
                return false;
            }

            var arg = args[argi];

            if (arg.StartsWith(prefix))
            {
                args.RemoveAt(argi);

                var argName = arg.Substring(prefix.Length);
                if (argName.Contains('='))
                {
                    var arr = argName.Split('=');
                    option = arr[0];
                    argument = arr[1];
                    return true;
                }

                var argProps = optionArgs.FirstOrDefault(o => o.Option == argName);

                option = argName;
                argument = null;

                if (argProps?.ArgumentName != null && args.Count > argi && !args[argi].StartsWith(prefix))
                {
                    argument = args[argi];
                    args.RemoveAt(argi);
                }

                return true;
            }
            else
            {
                argi++;
                goto Try;
            }
        }
    }
}