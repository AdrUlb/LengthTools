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
			Span<byte> lengthBytes = stackalloc byte[4];
			fs.Read(lengthBytes);
			var length = BinaryPrimitives.ReadInt32BigEndian(lengthBytes);

			Span<int> ops = stackalloc int[length];

			fs.Position = fs.Length - 5 - length;
			for (var i = 0; i < length; i++)
				ops[i] = fs.ReadByte();

			var stack = new Stack<int>();

			for (var i = 0; i < ops.Length; i++)
			{
				switch (ops[i])
				{
					case 0: // inp - put byte from stdin on the stack
						Console.WriteLine("inp");
						break;
					case 1: // add - add top two values on the stack and push the result
						{
							if (stack.Count < 2)
								throw new Exception("Stack underflow");

							var val1 = stack.Pop();
							var val2 = stack.Pop();
							stack.Push(val1 + val2);
						}
						break;
					case 2: // sub - subtract the top two values on the stack and push the result
						{
							if (stack.Count < 2)
								throw new Exception("Stack underflow");

							var val1 = stack.Pop();
							var val2 = stack.Pop();
							stack.Push(val2 - val1);
						}
						break;
					case 3: // dup - duplicate top value on the stack
						{
							if (stack.Count < 1)
								throw new Exception("Stack underflow");

							stack.Push(stack.Peek());
						}
						break;
					case 4: // cond - skip next instruction if the top value on the stack is 0 and pop that value
						Console.WriteLine("cond");
						break;
					case 5: // gotou - set program counter to next byte
						Console.WriteLine("gotou");
						break;
					case 6: // outn - pop stack output as number
						Console.WriteLine("outn");
						break;
					case 7: // outa - pop stack output as ascii
						{
							Console.Write((char)stack.Pop());
						}
						break;
					case 8: // mul - multiplay the top two values on the stack and push the result
						{
							if (stack.Count < 2)
								throw new Exception("Stack underflow");

							var val1 = stack.Pop();
							var val2 = stack.Pop();
							stack.Push(val1 * val2);
						}
						break;
					case 9: // div - divide the top two values on the stack and push the result
						{
							if (stack.Count < 2)
								throw new Exception("Stack underflow");

							var val1 = stack.Pop();
							var val2 = stack.Pop();
							stack.Push(val2 / val1);
						}
						break;
					case 10: // push - push value onto the stack
						i++;
						stack.Push(ops[i]);
						break;
				}
			}
		}
	}
}
