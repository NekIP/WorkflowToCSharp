using System;
using System.Text;

namespace WorkflowToCSharp.Assistants
{
	public interface ConsoleAssistant
	{
		void WriteLineInColor(string text, ConsoleColor color = ConsoleColor.White);
		void WiteLineOffset(string text, int count = 1, char offsetLetter = '\t', ConsoleColor color = ConsoleColor.White);
		string ReadLine(string text);
		int ReadLineNumber(string text);
		void Wait(ConsoleKey? stopKey = null);
	}

	public class ConsoleAssistantImpl : ConsoleAssistant
	{
		public Encoding Encoding { get; set; } = Encoding.UTF8;

		public void WriteLineInColor(string text, ConsoleColor color = ConsoleColor.White)
		{
			ChangeColorEncodingInConsoleAndExecuteAction(color, () => Console.WriteLine(text));
		}

		public void WriteInColor(string text, ConsoleColor color = ConsoleColor.White)
		{
			ChangeColorEncodingInConsoleAndExecuteAction(color, () => Console.Write(text));
		}

		public void WiteLineOffset(string text, int count = 1, char offsetLetter = '\t', 
			ConsoleColor color = ConsoleColor.White)
		{
			WriteLineInColor(new string(offsetLetter, count) + text, color);
		}

		public string ReadLine(string text)
		{
			WriteInColor(text);
			return Console.ReadLine();
		}

		public int ReadLineNumber(string text)
		{
			WriteInColor(text);
			string line = Console.ReadLine();
			if (int.TryParse(line, out var result))
			{
				return result;
			}
			throw new FormatException("Entered value have invalid format. Plese, enter number!");
		}

		public void Wait(ConsoleKey? stopKey = null)
		{
			if (stopKey.HasValue)
			{
				WriteLineInColor($"Press { stopKey.ToString() } for stop...");
				while (Console.ReadKey().Key != stopKey) { }
			}
			else
			{
				WriteLineInColor("Press any key for stop...");
				Console.ReadKey();
			}
		}

		private void ChangeColorEncodingInConsoleAndExecuteAction(ConsoleColor color, Action action)
		{
			Encoding oldOutputEncoding = Console.OutputEncoding;
			Console.OutputEncoding = Encoding;
			ConsoleColor oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			action();
			Console.ForegroundColor = oldColor;
			Console.OutputEncoding = oldOutputEncoding;
		}
	}
}
