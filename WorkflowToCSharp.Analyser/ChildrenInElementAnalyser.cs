using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WorkflowToCSharp.Assistants;

namespace WorkflowToCSharp.Analyser
{
	public interface ChildrenInElementAnalyser
	{
		void FindChildren(string xaml, string parentName);
	}

	public class ChildrenInElementAnalyserImpl : ChildrenInElementAnalyser
	{
		private readonly ConsoleAssistant consoleAssistant;
		private readonly XamlAssistant xamlAssistant;
		private List<string> findedChildren;

		public ChildrenInElementAnalyserImpl(ConsoleAssistant consoleAssistant,
			XamlAssistant xamlAssistant)
		{
			this.consoleAssistant = consoleAssistant;
			this.xamlAssistant = xamlAssistant;
			this.findedChildren = new List<string>();
		}

		public void FindChildren(string xaml, string parentName)
		{
			XamlParseResult parseResult = xamlAssistant.Parse(xaml);
			IEnumerable<XElement> elements = parseResult.Elements;
			foreach (var element in elements)
			{
				FindChildren(element, parentName);
			}
			PrintFindedChildren();
		}

		private void FindChildren(XElement element, string parentName, bool findParent = false, string fullName = "")
		{
			IEnumerable<XElement> children = element.Elements();
			string name = element.Name.LocalName;
			if (findParent)
			{
				/*if (!customMethodsAssistant.Exist(name))
				{
					UsingConstructions.Add(parentName + "\t" + name);
				}*/
			}
			else if (!findParent && name == parentName)
			{
				findParent = true;
			}
			foreach (var child in children)
			{
				FindChildren(child, parentName, findParent, 
					findParent 
						? fullName + "\t" + name 
						: fullName);
			}
		}

		private void PrintFindedChildren()
		{
			foreach (string item in findedChildren.OrderBy(x => x))
			{
				consoleAssistant.WriteLineInColor(item);
			}
		}
	}
}
