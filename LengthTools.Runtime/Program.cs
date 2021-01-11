using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LengthTools.Runtime
{
	class Program
	{
		static readonly string? asmPath = Process.GetCurrentProcess().MainModule?.FileName;

		static void Main(string[] args)
		{
			if (asmPath == null)
				Environment.Exit(-1);

			var fs = File.OpenRead(asmPath);

			fs.Position = fs.Length - 1;
			if (fs.ReadByte() != 0xAB)
				Environment.Exit(0);

			fs.Position = fs.Length - 5;
			Span<byte> intBytes = stackalloc byte[4];
			fs.Read(intBytes);
			var length = BinaryPrimitives.ReadInt32BigEndian(intBytes);

			Span<int> ops = stackalloc int[length];


			fs.Position = fs.Length - 5 - length;
			for (var i = 0; i < length / 4; i++)
			{
				fs.Read(intBytes);
				ops[i] = BinaryPrimitives.ReadInt32BigEndian(intBytes);
			}

			var stack = new Stack<int>();

			for (var i = 0; i < ops.Length; i++)
			{
				switch (ops[i])
				{
					case 9: // inp - put byte from stdin on the stack
						{
							stack.Push(Console.ReadKey(true).KeyChar);
						}
						break;
					case 10: // add - add top two values on the stack and push the result
						{
							if (stack.Count < 2)
								throw new Exception("Stack underflow");

							var val1 = stack.Pop();
							var val2 = stack.Pop();
							stack.Push(val1 + val2);
						}
						break;
					case 11: // sub - subtract the top two values on the stack and push the result
						{
							if (stack.Count < 2)
								throw new Exception("Stack underflow");

							var val1 = stack.Pop();
							var val2 = stack.Pop();
							stack.Push(val2 - val1);
						}
						break;
					case 12: // dup - duplicate top value on the stack
						{
							if (stack.Count < 1)
								throw new Exception("Stack underflow");

							stack.Push(stack.Peek());
						}
						break;
					case 13: // cond - skip next instruction if the top value on the stack is 0 and pop that value
						{
							if (stack.Count < 1)
								throw new Exception("Stack underflow");

							if (stack.Pop() == 0)
							{
								i++;
								i += LengthTools.Common.LengthCompiler.instructionSet[ops[i]].Item2;
							}
						}
						break;
					case 14: // gotou - set program counter to next byte
						{
							i = ops[i + 1] - 1;
						}
						break;
					case 15: // outn - pop stack output as number
						{
							if (stack.Count < 1)
								throw new Exception("Stack underflow");

							Console.Write(stack.Pop());
						}
						break;
					case 16: // outa - pop stack output as ascii
						{
							if (stack.Count < 1)
								throw new Exception("Stack underflow");

							Console.Write((char)stack.Pop());
						}
						break;
					case 20: // mul - multiplay the top two values on the stack and push the result
						{
							if (stack.Count < 2)
								throw new Exception("Stack underflow");

							var val1 = stack.Pop();
							var val2 = stack.Pop();
							stack.Push(val1 * val2);
						}
						break;
					case 21: // div - divide the top two values on the stack and push the result
						{
							if (stack.Count < 2)
								throw new Exception("Stack underflow");

							var val1 = stack.Pop();
							var val2 = stack.Pop();
							stack.Push(val2 / val1);
						}
						break;
					case 25: // push - push value onto the stack
						i++;
						stack.Push(ops[i]);
						break;
				}
			}
		}
	}
}
