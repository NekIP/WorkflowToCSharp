using System.Collections.Generic;

namespace WorkflowToCSharp.Converter.Data
{
	public class TryCatchCode : Construction
	{
		public Sequence Try { get; set; }
		public List<CatchCode> Catches { get; set; }
		public Sequence Finaly { get; set; }
	}
}
