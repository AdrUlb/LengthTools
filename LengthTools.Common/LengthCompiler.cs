using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LengthTools.Common
{
	public static class LengthCompiler
	{
		static readonly IReadOnlyDictionary<int, (string, int)> instructionSet = new Dictionary<int, (string, int)>()
		{
			{ 9, ("inp", 0) },
			{ 10, ("add", 0) },
			{ 11, ("sub", 0) },
			{ 12, ("dup", 0) },
			{ 13, ("cond", 0) },
			{ 14, ("gotou", 1) },
			{ 15, ("outn", 0) },
			{ 16, ("outa", 0) },
			{ 20, ("mul", 0) },
			{ 21, ("div", 0) },
			{ 25, ("push", 1) },
		};

		static readonly IReadOnlyDictionary<string, int> byteCode = new Dictionary<string, int>()
		{
			{ "inp", 0 },
			{ "add", 1 },
			{ "sub", 2 },
			{ "dup", 3 },
			{ "cond", 4 },
			{ "gotou", 5 },
			{ "outn", 6 },
			{ "outa", 7 },
			{ "mul", 8 },
			{ "div", 9 },
			{ "push", 10 },
		};

		public static string[] LengthToIntermediate(string[] code)
		{
			Span<int> lengths = stackalloc int[code.Length];

			for (var i = 0; i < code.Length; i++)
				lengths[i] = code[i].Length;

			List<string> il = new List<string>();

			for (var i = 0; i < lengths.Length; i++)
			{
				if (!instructionSet.ContainsKey(lengths[i]))
					throw new Exception();

				(string instruction, int argCount) = instructionSet[lengths[i]];

				if (argCount > 0)
				{
					var instArgs = new int[argCount];

					for (var j = 0; j < argCount; j++)
					{
						i++;
						instArgs[j] = lengths[i];
					}

					instruction += " " + instArgs[0];

					for (var j = 1; j < argCount; j++)
					{
						instruction += ", " + instArgs[j];
					}
				}

				il.Add(instruction);
			}

			return il.ToArray();
		}

		public static byte[] IntermediateToByteCode(string[] code)
		{
			List<byte> bytes = new List<byte>();

			List<string> usedInstructions = new List<string>();

			foreach (var line in code)
			{
				bool noArgs = !line.Contains(' ');
				var inst = noArgs ? line : line.Substring(0, line.IndexOf(' '));
				var args = noArgs ? Array.Empty<string>() : line[line.IndexOf(' ')..].Split(',');

				Console.WriteLine(inst);
			}

			return bytes.ToArray();
		}
	}
}
