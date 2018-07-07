using System.Collections.Generic;

namespace WorkflowToCSharp.Converter.Data
{
	public class ClassCode : Code
	{
		public string Name { get; set; }
		public string Namespace { get; set; }
		public List<string> UsingNamespaces { get; set; }
		public List<Code> CodeBlocks { get; set;}
	}
}
