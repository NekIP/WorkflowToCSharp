using System.Collections.Generic;

namespace WorkflowToCSharp.Converter.Data
{
	public class CustomActivityCode : Construction
	{
		public string Name { get; set; }
		public string ResultTo { get; set; }
		public string ReturnType { get; set; }
		public List<AssignCode> Assigns { get; set; }
		public Method Method { get; set; }
	}
}
