namespace WorkflowToCSharp.Converter.Data
{
	public class CatchCode : Construction
	{
		public Sequence Body { get; set; }
		public string Exception { get; set; }
	}
}
