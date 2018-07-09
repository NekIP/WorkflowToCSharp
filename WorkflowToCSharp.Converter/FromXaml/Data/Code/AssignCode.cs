namespace WorkflowToCSharp.Converter.Data
{
	public class AssignCode : Construction
	{
		public string To { get; set; }
		public string Value { get; set; }
	}

	public class CustomMethodAssignCode : AssignCode
	{
		public ParameterDirection Direction { get; set; }
	}
}
