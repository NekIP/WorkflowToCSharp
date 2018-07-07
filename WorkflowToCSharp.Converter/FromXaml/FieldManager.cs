using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WorkflowToCSharp.Converter.Data;

namespace WorkflowToCSharp.Converter
{
	public interface FieldManager
	{
		void ParseAndInitialize(List<Code> codeBlocks);
		VariableCode GetVariable(string name);
		PropertyCode GetProperty(string name);
		List<VariableCode> GetUsingVariables(string line);
	}

	public class FieldManagerImpl : FieldManager
	{
		private List<VariableCode> allVariables;
		private List<PropertyCode> allProperties;

		public void ParseAndInitialize(List<Code> codeBlocks)
		{
			allVariables = FindAllVariables(codeBlocks);
			allProperties = FindAllProperties(codeBlocks);
		}

		public VariableCode GetVariable(string name)
		{
			return allVariables.FirstOrDefault(x => x.Name == name);
		}

		public List<VariableCode> GetUsingVariables(string line)
		{
			return ExtractVariables(line);
		}

		public PropertyCode GetProperty(string name)
		{
			return allProperties.FirstOrDefault(x => x.Name == name);
		}

		private List<VariableCode> ExtractVariables(string line)
		{
			VariableCode variable = GetVariable(line);
			if (variable != null)
			{
				return new List<VariableCode> { variable };
			}
			return allVariables.Where(x => Regex.IsMatch(" " + line + " ", @"[ ,=><)(]" + x.Name + @"[ ,\.\?=><)(]")).ToList();
		}

		private List<VariableCode> FindAllVariables(List<Code> codeBlocks)
		{
			var result = new List<VariableCode>();
			foreach (Code codeBlock in codeBlocks)
			{
				switch (codeBlock)
				{
					case Method method:
						result.AddRange(FindAllVariables(new List<Code> { method.Sequence }));
						break;
					case Sequence sequence:
						result.AddRange(FindAllVariables(sequence.Values.Select(x => (Code)x).ToList()));
						break;
					case TryCatchCode tryCatchCode:
						result.AddRange(FindAllVariables(new List<Code> { tryCatchCode.Try }));
						foreach (CatchCode catchCode in tryCatchCode.Catches)
						{
							result.AddRange(FindAllVariables(new List<Code> { catchCode.Body }));
						}
						break;
					case IfCode ifCode:
						result.AddRange(FindAllVariables(new List<Code> { ifCode.Then }));
						if (ifCode.Else != null)
						{
							result.AddRange(FindAllVariables(new List<Code> { ifCode.Else }));
						}
						break;
					case VariableCode variable:
						result.Add(variable);
						break;
				}
			}
			return result;
		}

		private List<PropertyCode> FindAllProperties(List<Code> codeBlocks)
		{
			var result = new List<PropertyCode>();
			foreach (Code codeBlock in codeBlocks)
			{
				if (codeBlock is PropertyCode property)
				{
					result.Add(property);
					continue;
				}
			}
			return result;
		}
	}
}
