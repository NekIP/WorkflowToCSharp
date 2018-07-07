using System.Collections.Generic;
using System.Linq;
using WorkflowToCSharp.Converter.Data;
using WorkflowToCSharp.Converter.Extensions;

namespace WorkflowToCSharp.Converter
{
	public interface CodeRefactor
	{
		void AllocateCustomMethods(List<Code> codeBlock);
	}

	public class CodeRefactorImpl : CodeRefactor
	{
		private readonly FieldManager fieldManager;

		public CodeRefactorImpl(FieldManager fieldManager)
		{
			this.fieldManager = fieldManager;
		}

		public void AllocateCustomMethods(List<Code> codeBlocks)
		{
			fieldManager.ParseAndInitialize(codeBlocks);
			var customActivityMethods = new List<Method>();
			foreach (Code codeBlock in codeBlocks)
			{
				if (codeBlock is Method method)
				{
					customActivityMethods.AddRange(CreateCustomActivityMethods(method.Sequence));
				}
			}
			codeBlocks.AddRange(customActivityMethods);
		}

		private List<Method> CreateCustomActivityMethods(Sequence sequence)
		{
			var result = new List<Method>();
			foreach (Construction connstruction in sequence.Values)
			{
				switch (connstruction)
				{
					case IfCode ifCode:
						result.AddRange(CreateCustomActivityMethods(ifCode.Then));
						if (ifCode.Else != null)
						{
							result.AddRange(CreateCustomActivityMethods(ifCode.Else));
						}
						break;
					case Sequence subSequence:
						result.AddRange(CreateCustomActivityMethods(subSequence));
						break;
					case TryCatchCode tryCatchCode:
						result.AddRange(CreateCustomActivityMethods(tryCatchCode.Try));
						foreach (CatchCode catchCode in tryCatchCode.Catches)
						{
							result.AddRange(CreateCustomActivityMethods(catchCode.Body));
						}
						break;
					case CustomActivityCode customActivityCode:
						result.Add(CreateMethodFromCustomActivity(customActivityCode));
						break;
				}
			}
			return result;
		}

		private Method CreateMethodFromCustomActivity(CustomActivityCode entity)
		{
			var result = new Method
			{
				AccessModify = "private",
				Name = entity.Name,
				ReturnType = entity.ReturnType,
				InArguments = new List<VariableCode>(),
				Sequence = new Sequence()
			};
			result.Sequence.Values.Add(new AssignCode
			{
				To = $"var {entity.Name.FirstLetterLower()}",
				Value = $"new {entity.Name}()"
			});
			foreach (AssignCode assign in entity.Assigns)
			{
				result.InArguments.AddRange(fieldManager.GetUsingVariables(assign.Value));
				result.Sequence.Values.Add(new AssignCode
				{
					To = $"{entity.Name.FirstLetterLower()}.{assign.To}",
					Value = assign.Value
				});
			}
			for (var i = result.InArguments.Count - 1; i >= 0; i--)
			{
				VariableCode inArgument = result.InArguments[i];
				if (result.InArguments.Where(x => x.Name == inArgument.Name).Count() > 1)
				{
					result.InArguments.RemoveAt(i);
				}
			}
			if (entity.ReturnType == "void")
			{
				result.Sequence.Values.Add(new StringCode { Value = $"{entity.Name.FirstLetterLower()}.Execute()" });
			}
			else
			{
				result.Sequence.Values.Add(new StringCode { Value = $"return {entity.Name.FirstLetterLower()}.Execute()" });
			}
			entity.Method = result;
			return result;
		}
	}
}
