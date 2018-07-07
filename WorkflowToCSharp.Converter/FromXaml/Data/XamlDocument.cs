using System.Collections.Generic;
using System.Xml.Linq;

namespace WorkflowToCSharp.Converter.Data
{
	public class XamlDocument
	{
		public string Name { get; set; }
		public string Namespace { get; set; }
		public IEnumerable<XElement> Elements { get; set; }
		public List<XamlNamespace> UsingNamespaces { get; set; }
	}
}
