using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorkflowToCSharp.Assistants;
using WorkflowToCSharp.Converter;

namespace WorkflowToCSharp.ConsoleApplication
{
	public interface Startup
	{
		void Execute(string[] args);
	}

	public class StartupImpl : Startup
	{
		private readonly ConsoleAssistant consoleAssistant;
		private readonly WorkflowToCSharpConverter workflowToCSharpConverter;

		public StartupImpl(ConsoleAssistant consoleAssistant,
			WorkflowToCSharpConverter workflowToCSharpConverter)
		{
			this.consoleAssistant = consoleAssistant;
			this.workflowToCSharpConverter = workflowToCSharpConverter;
		}

		public void Execute(string[] args)
		{
			string source = args.FirstOrDefault();
			if (source is null)
			{
				source = consoleAssistant.ReadLine("Source (xaml or xaml file path or folder with xaml files path) = ");
			}
			try
			{
				ConvertSource(source);
			}
			catch (DirectoryNotFoundException ex)
			{
				consoleAssistant.WriteLineInColor(ex.Message, ConsoleColor.Red);
			}
			catch (ArgumentException ex)
			{
				consoleAssistant.WriteLineInColor(ex.Message, ConsoleColor.Red);
			}
			catch (FormatException ex)
			{
				consoleAssistant.WriteLineInColor(ex.Message, ConsoleColor.Red);
			}
			catch (FileNotFoundException ex)
			{
				consoleAssistant.WriteLineInColor(ex.Message, ConsoleColor.Red);
			}
			consoleAssistant.Wait();
		}

		private void ConvertSource(string source)
		{
			if (string.IsNullOrWhiteSpace(source))
			{
				throw new ArgumentException("Source is empty");
			}
			List<string> files = new List<string>();
			if (File.Exists(source))
			{
				consoleAssistant.WriteLineInColor("File: " + source, ConsoleColor.Green);
				ConvertFile(source);
			}
			else if (Directory.Exists(source))
			{
				string[] filePaths = Directory.GetFiles(source, "*.xaml", SearchOption.AllDirectories);
				for (var i = 0; i < filePaths.Length; i++)
				{
					consoleAssistant.WriteLineInColor("File: " + filePaths[i], ConsoleColor.Green);
					ConvertFile(filePaths[i]);
				}
			}
			else
			{
				string convertedContent = workflowToCSharpConverter.Convert(source);
				consoleAssistant.WriteLineInColor(convertedContent);
			}
		}

		private void ConvertFile(string path, string outputPath = "Output")
		{
			string outputFolder = Path.Combine(Path.GetDirectoryName(path), outputPath);
			if (!Directory.Exists(outputFolder))
			{
				Directory.CreateDirectory(outputFolder);
			}
			using (var xamlFile = new StreamReader(File.Open(path, FileMode.Open)))
			{
				string xaml = xamlFile.ReadToEnd();
				string csharpCode = workflowToCSharpConverter.Convert(xaml);
				string outputFilePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(path) + ".cs");
				using (var convertedFile = new StreamWriter(File.Open(outputFilePath, FileMode.Create)))
				{
					convertedFile.Write(csharpCode);
				}
			}
		}
	}
}
