using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WorkflowToCSharp.Converter.Extensions
{
	public static class XElementExtensions
	{
		public static XElement Child(this XElement self, string name)
		{
			return self.Elements().FirstOrDefault(x => x.Name.LocalName == name);
		}

		public static XAttribute GetAttribute(this XElement self, string name)
		{
			return self.Attributes().FirstOrDefault(x => x.Name.LocalName == name);
		}

		public static IEnumerable<XElement> Children(this XElement self, string name)
		{
			return self.Elements().Where(x => x.Name.LocalName == name);
		}
	}
}
