using WorkflowToCSharp.Converter.Data;

namespace WorkflowToCSharp.Converter
{
	public class WorkflowToCSharpConverterFromXaml : WorkflowToCSharpConverter
	{
		private readonly XamlParser xamlParser;
		private readonly ClassCodeToCSharp classCodeToCSharp;
		private readonly ClassGenerator classGenerator;

		public WorkflowToCSharpConverterFromXaml(XamlParser xamlParser,
			ClassCodeToCSharp classCodeToCSharp,
			ClassGenerator classGenerator)
		{
			this.xamlParser = xamlParser;
			this.classCodeToCSharp = classCodeToCSharp;
			this.classGenerator = classGenerator;
		}

		public string Convert(string workflowXaml)
		{
			XamlDocument document = xamlParser.Parse(workflowXaml);
			ClassCode classCode = classGenerator.Generate(document);
			return classCodeToCSharp.Convert(classCode);
		}
	}
}
