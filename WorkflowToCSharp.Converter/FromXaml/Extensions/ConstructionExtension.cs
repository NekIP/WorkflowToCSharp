using System.Collections.Generic;
using WorkflowToCSharp.Converter.Data;

namespace WorkflowToCSharp.Converter.Extensions
{
	public static class ConstructionExtension
	{
		public static Sequence WrapInSequence(this Construction construction)
		{
			if (construction is Sequence sequence)
			{
				return sequence;
			}
			return new Sequence
			{
				Values = new List<Construction>
				{
					construction
				}
			};
		}
	}
}
