using System.Collections.Generic;

namespace WorkflowToCSharp.Converter.Data
{
	public class Method : Construction
	{
		public string Name { get; set; }
		public string ReturnType { get; set; }
		public string AccessModifier { get; set; }
		public List<MethodParameter> Parameters { get; set; }
		public Sequence Sequence { get; set; }
	}

	public class MethodParameter
	{
		public VariableCode Variable { get; set; }
		public ParameterDirection Direction { get; set; }
	}

	public enum ParameterDirection
	{
		In,
		Out,
		InOut
	}
}
