namespace WorkflowToCSharp.ConsoleApplication
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var settings = new Setting();
			settings.InitializeDependences();
			settings.RunApplication(args);
		}
	}
}
