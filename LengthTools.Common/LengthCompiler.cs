using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace LengthTools.Common
{
	public static class LengthCompiler
	{
		public static readonly IReadOnlyDictionary<int, (string, int)> instructionSet = new Dictionary<int, (string, int)>()
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

		public static string[] LengthToIntermediate(string[] code)
		{
			Span<int> lengths = stackalloc int[code.Length];

			for (var i = 0; i < code.Length; i++)
				lengths[i] = code[i].Length;

			List<string> il = new List<string>();

			for (var i = 0; i < lengths.Length; i++)
			{
				if (lengths[i] == 0)
					continue;

				if (!instructionSet.ContainsKey(lengths[i]))
					throw new Exception($"Unknown length {i}");

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
			var bytes = new List<byte>();

			Span<byte> intBytes = stackalloc byte[4];

			foreach (var line in code)
			{
				bool noArgs = !line.Contains(' ');
				var inst = noArgs ? line : line.Substring(0, line.IndexOf(' '));
				var args = noArgs ? Array.Empty<string>() : line[line.IndexOf(' ')..].Split(',');

				for (var i = 0; i < instructionSet.Count; i++)
				{
					var item = instructionSet.ElementAt(i);

					if (item.Value.Item1 == inst)
					{
						BinaryPrimitives.WriteInt32BigEndian(intBytes, item.Key);

						foreach (var b in intBytes)
							bytes.Add(b);

						break;
					}
				}

				foreach (var arg in args)
				{
					BinaryPrimitives.WriteInt32BigEndian(intBytes, int.Parse(arg));

					foreach (var b in intBytes)
						bytes.Add(b);
				}
			}

			return bytes.ToArray();
		}
	}
}
