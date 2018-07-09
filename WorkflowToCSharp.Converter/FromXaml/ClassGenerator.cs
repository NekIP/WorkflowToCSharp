using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WorkflowToCSharp.Converter.Data;
using WorkflowToCSharp.Converter.Extensions;

namespace WorkflowToCSharp.Converter
{
	public interface ClassGenerator
	{
		ClassCode Generate(XamlDocument document);
	}

	public class ClassGeneratorImpl : ClassGenerator
	{
		private readonly MethodGenerator methodGenerator;
		private readonly CustomMethodAlocator customMethodAlocator;

		public ClassGeneratorImpl(MethodGenerator methodGenerator,
			CustomMethodAlocator customMethodAlocator)
		{
			this.methodGenerator = methodGenerator;
			this.customMethodAlocator = customMethodAlocator;
		}

		public ClassCode Generate(XamlDocument document)
		{
			var result = new ClassCode
			{
				Name = document.Name,
				CodeBlocks = new List<Code>(),
				UsingNamespaces = new List<string>(),
				Namespace = document.Namespace
			};
			IEnumerable<XElement> children = document.Elements;
			foreach (var child in children)
			{
				string name = child.Name.LocalName;
				switch (name)
				{
					case "Members":
						result.CodeBlocks.AddRange(GetProperties(child));
						break;
					case "TextExpression.NamespacesForImplementation":
						result.UsingNamespaces.AddRange(GetNamespaces(child));
						break;
					case "Sequence":
						result.CodeBlocks.Add(methodGenerator.Generate(child));
						break;
				}
			}
			customMethodAlocator.Allocate(result.CodeBlocks);
			return result;
		}

		private List<PropertyCode> GetProperties(XElement element)
		{
			var result = new List<PropertyCode>();
			IEnumerable<XElement> children = element.Elements();
			foreach (var child in children)
			{
				var name = child.Attribute(XName.Get("Name")).Value;
				var type = child.Attribute(XName.Get("Type")).Value;
				type = type.Replace("InOutArgument(", "").Replace("OutArgument(", "").Replace("InArgument(", "").Replace(")", "");
				if (type.Contains(":"))
				{
					type = type.Split(':').Last();
				}
				result.Add(new PropertyCode
				{
					Name = name,
					Type = type
				});
			}
			return result;
		}

		private List<string> GetNamespaces(XElement element)
		{
			var result = new List<string>();
			XElement collection = element.Child("Collection");
			IEnumerable<XElement> namespaces = collection.Children("String");
			foreach (var name in namespaces)
			{
				result.Add(name.Value);
			}
			return result;
		}
	}
}
