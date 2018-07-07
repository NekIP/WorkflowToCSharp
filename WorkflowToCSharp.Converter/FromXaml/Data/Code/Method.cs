using System.Collections.Generic;

namespace WorkflowToCSharp.Converter.Data
{
	public class Method : Construction
	{
		public string Name { get; set; }
		public string ReturnType { get; set; }
		public string AccessModify { get; set; }
		public List<VariableCode> InArguments { get; set; }
		public Sequence Sequence { get; set; }
	}
}
