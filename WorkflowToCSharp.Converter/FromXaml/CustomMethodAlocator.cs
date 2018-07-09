using System.Collections.Generic;
using System.Linq;
using WorkflowToCSharp.Converter.Data;
using WorkflowToCSharp.Converter.Extensions;

namespace WorkflowToCSharp.Converter
{
	public interface CustomMethodAlocator
	{
		void Allocate(List<Code> codeBlock);
	}

	public class CustomMethodAlocatorImpl : CustomMethodAlocator
	{
		private readonly FieldManager fieldManager;

		public CustomMethodAlocatorImpl(FieldManager fieldManager)
		{
			this.fieldManager = fieldManager;
		}

		public void Allocate(List<Code> codeBlocks)
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
				AccessModifier = "private",
				Name = entity.Name,
				ReturnType = entity.ReturnType,
				Parameters = new List<MethodParameter>(),
				Sequence = new Sequence()
			};
			var objectName = "entity";
			result.Sequence.Values.Add(new AssignCode
			{
				To = $"var {objectName}",
				Value = $"new {entity.Name}()"
			});
			var finallyBlock = new Sequence();
			foreach (CustomMethodAssignCode assign in entity.Assigns)
			{
				result.Parameters.AddRange(fieldManager.GetUsingVariables(assign.Value)
					.Select(x => new MethodParameter
					{
						Variable = x,
						Direction = assign.Direction
					}));
				result.Sequence.Values.Add(new AssignCode
				{
					To = $"{objectName}.{assign.To}",
					Value = assign.Value
				});
				if (assign.Direction == ParameterDirection.InOut ||
					assign.Direction == ParameterDirection.Out)
				{
					finallyBlock.Values.Add(new AssignCode
					{
						To = assign.Value,
						Value = $"{objectName}.{assign.To}"
					});
				}
			}
			for (var i = result.Parameters.Count - 1; i >= 0; i--)
			{
				MethodParameter parameter = result.Parameters[i];
				if (result.Parameters.Where(x => x.Variable.Name == parameter.Variable.Name).Count() > 1)
				{
					result.Parameters.RemoveAt(i);
				}
			}
			var executeLine = $"{objectName}.Execute()";
			if (entity.ReturnType != "void")
			{
				executeLine = "return " + executeLine;
			}
			var executeStringCode = new StringCode
			{
				Value = executeLine
			};
			if (finallyBlock.Values.Count > 0)
			{
				var tryFinally = new TryCatchCode
				{
					Try = new StringCode { Value = executeLine }.WrapInSequence(),
					Catches = new List<CatchCode>(),
					Finally = finallyBlock
				};
				result.Sequence.Values.Add(tryFinally);
			}
			else
			{
				result.Sequence.Values.Add(executeStringCode);
			}
			entity.Method = result;
			return result;
		}
	}
}
