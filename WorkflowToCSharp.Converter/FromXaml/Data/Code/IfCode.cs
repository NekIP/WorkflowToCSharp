namespace WorkflowToCSharp.Converter.Data
{
	public class IfCode : Construction
	{
		public string Condition { get; set; }
		public Sequence Then { get; set; }
		public Sequence Else { get; set; }
	}
}
