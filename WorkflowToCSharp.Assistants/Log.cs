using System;

namespace WorkflowToCSharp.Assistants
{
	public interface Log
	{
		void Info(string text);
		void Exception(string text);
	}

	public class ConsoleLog : Log
	{
		private readonly ConsoleAssistant consoleAssistant;

		public ConsoleLog(ConsoleAssistant consoleAssistant)
		{
			this.consoleAssistant = consoleAssistant;
		}

		public void Info(string text)
		{
			consoleAssistant.WriteLineInColor(text);
		}

		public void Exception(string text)
		{
			consoleAssistant.WriteLineInColor(text, ConsoleColor.Red);
		}
	}
}
