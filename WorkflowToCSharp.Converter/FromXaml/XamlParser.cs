using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WorkflowToCSharp.Converter.Data;
using WorkflowToCSharp.Converter.Extensions;

namespace WorkflowToCSharp.Converter
{
	public interface XamlParser
	{
		XamlDocument Parse(string xaml);
	}

	public class XamlParserImpl : XamlParser
	{
		public XamlDocument Parse(string xaml)
		{
			var document = XDocument.Parse(xaml);
			XElement mainElement = document.Elements().First();
			string className = mainElement.GetAttribute("Class")?.Value;
			string[] splitedClassName = className.Split('.');
			className = splitedClassName.LastOrDefault();
			string classNamespace = string.Join(".", 
				splitedClassName.Take(splitedClassName.Length - 1));
			IEnumerable<XElement> children = mainElement.Elements();
			List<XamlNamespace> usingNamespaces = ParseNamespaces(mainElement);
			return new XamlDocument
			{
				Elements = children,
				UsingNamespaces = usingNamespaces,
				Name = className,
				Namespace = classNamespace
			};
		}

		private List<XamlNamespace> ParseNamespaces(XElement element)
		{
			var result = new List<XamlNamespace>();
			IEnumerable<XAttribute> attributes = element.Attributes();
			foreach (var attribute in attributes)
			{
				if (attribute.Value.StartsWith("clr-namespace:"))
				{
					string name = attribute.Name.LocalName;
					string value = string.Join(":", attribute.Value.Split(':').Skip(1).ToArray());
					result.Add(new XamlNamespace
					{
						Name = name,
						Value = value
					});
				}
			}
			return result;
		}
	}
}
