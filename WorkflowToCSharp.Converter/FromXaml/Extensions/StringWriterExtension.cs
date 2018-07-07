using System.IO;

namespace WorkflowToCSharp.Converter.Extensions
{
	public static class StringWriterExtension
	{
		public static void WriteLineTabs(this StringWriter stringWriter, 
			string text, int tabs)
		{
			stringWriter.WriteLine(new string('\t', tabs) + text);
		}
	}
}
