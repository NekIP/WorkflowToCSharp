using System.Collections.Generic;

namespace WorkflowToCSharp.Converter.Data
{
	public class Sequence : Construction
	{
		public Sequence()
		{
			Values = new List<Construction>();
		}

		public List<Construction> Values { get; set; }
	}
}
