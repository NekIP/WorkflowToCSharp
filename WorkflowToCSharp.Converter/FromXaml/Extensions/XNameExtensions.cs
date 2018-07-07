using System.Linq;
using System.Xml.Linq;

namespace WorkflowToCSharp.Converter.Extensions
{
	public static class XNameExtensions
	{
		public static string GetMain(this XName name)
		{
			string clearName = name.LocalName;
			string[] levelingNames = clearName.Split('.');
			return levelingNames.FirstOrDefault();
		}

		public static string GetNamespace(this XName name)
		{
			return name.NamespaceName;
		}
	}
}
