using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WorkflowToCSharp.Assistants;
using WorkflowToCSharp.Converter.Data;
using WorkflowToCSharp.Converter.Extensions;

namespace WorkflowToCSharp.Converter
{
	public interface MethodGenerator
	{
		Method Generate(XElement root);
	}

	public class MethodGeneratorImpl : MethodGenerator
	{
		private readonly Log log;

		public MethodGeneratorImpl(Log log)
		{
			this.log = log;
		}

		public Method Generate(XElement root)
		{
			var result = new Method
			{
				Name = "Execute",
				AccessModify = "public",
				ReturnType = "void",
				Sequence = WrapInSequence(Parse(root))
			};
			return result;
		}

		private Construction Parse(XElement element)
		{
			string name = element.Name.LocalName;
			switch (name)
			{
				case "Sequence":
					return GetSequence(element);
				case "If":
					return GetIfCode(element);
				case "Assign":
					return GetAssign(element);
				case "TryCatch":
					return GetTryCatch(element);
			}
			if (element.Name.NamespaceName.Contains("SegPaySecure"))
			{
				return GetCustomActivity(element);
			}
			log.Info($"Unrecognized element {element.Name.LocalName}");
			return null;
		}

		private Sequence GetSequence(XElement element)
		{
			var result = new Sequence();
			IEnumerable<XElement> children = element.Elements();
			foreach (var child in children)
			{
				string name = child.Name.LocalName;
				if (name == "Sequence.Variables")
				{
					var variables = new List<VariableCode>();
					IEnumerable<XElement> elementVariables = child.Children("Variable");
					foreach (XElement item in elementVariables)
					{
						string variableType = item.GetAttribute("TypeArguments").Value;
						if (variableType.Contains(":"))
						{
							variableType = variableType.Split(':').Last();
						}
						string variableName = item.GetAttribute("Name").Value;
						variables.Add(new VariableCode
						{
							Type = variableType,
							Name = variableName,
							WasInitialized = false
						});
					}
					result.Values.AddRange(variables);
				}
				else
				{
					result.Values.Add(Parse(child));
				}
			}
			return result;
		}

		private IfCode GetIfCode(XElement element)
		{
			var result = new IfCode();
			XElement elementCondition = element.Child("If.Condition");
			result.Condition = GetArgument(elementCondition.Elements().FirstOrDefault());
			XElement elementThen = element.Child("If.Then");
			if (elementThen != null)
			{
				result.Then = WrapInSequence(Parse(elementThen.Elements().FirstOrDefault()));
			}
			else
			{
				result.Then = new Sequence();
			}
			XElement elementElse = element.Child("If.Else");
			if (elementElse != null)
			{
				result.Else = WrapInSequence(Parse(elementElse.Elements().FirstOrDefault()));
			}
			// TODO: elseif
			return result;
		}

		private AssignCode GetAssign(XElement element)
		{
			var result = new AssignCode();
			XElement elementTo = element.Child("Assign.To");
			result.To = GetArgument(elementTo.Elements().FirstOrDefault());
			XElement elementValue = element.Child("Assign.Value");
			result.Value = GetArgument(elementValue.Elements().FirstOrDefault());
			return result;
		}

		private TryCatchCode GetTryCatch(XElement element)
		{
			var result = new TryCatchCode();
			XElement elementTry = element.Child("TryCatch.Try");
			result.Try = WrapInSequence(Parse(elementTry.Elements().FirstOrDefault()));
			result.Catches = new List<CatchCode>();
			IEnumerable<XElement> catchElements = element.Child("TryCatch.Catches").Children("Catch");
			foreach (var catchElement in catchElements)
			{
				XElement activityAction = catchElement.Child("ActivityAction");
				Sequence body = WrapInSequence(Parse(activityAction.Elements().Last()));
				XElement argument = activityAction.Child("ActivityAction.Argument").Child("DelegateInArgument");
				string exceptionType = argument.GetAttribute("TypeArguments").Value;
				if (exceptionType.Contains(":"))
				{
					exceptionType = exceptionType.Split(':').Last();
				}
				string exceptionVariableName = argument.GetAttribute("Name").Value;
				result.Catches.Add(new CatchCode
				{
					Body = body,
					Exception = $"{exceptionType} {exceptionVariableName}"
				});
			}
			return result;
		}

		private string GetArgument(XElement element)
		{
			string result = element.Value;
			if (result == "True" || result == "False")
			{
				result = result.FirstLetterLower();
			}
			return result;
		}

		private CustomActivityCode GetCustomActivity(XElement element)
		{
			var result = new CustomActivityCode
			{
				Name = element.Name.LocalName,
				Assigns = new List<AssignCode>(),
				ReturnType = "void"
			};
			IEnumerable<XElement> children = element.Elements();
			foreach (var child in children)
			{
				string name = child.Name.LocalName;
				if (name.StartsWith(element.Name.LocalName))
				{
					string[] names = name.Split('.');
					string variableName = names.Last();
					if (variableName == "Result")
					{
						XElement argumentElement = child.Elements().FirstOrDefault();
						result.ResultTo = GetArgument(argumentElement);
						string returnType = argumentElement.GetAttribute("TypeArguments").Value;
						if (returnType.Contains(":"))
						{
							returnType = returnType.Split(':').Last();
						}
						result.ReturnType = returnType;
						continue;
					}
					var assign = new AssignCode
					{
						To = variableName,
						Value = GetArgument(child.Elements().FirstOrDefault())
					};
					result.Assigns.Add(assign);
				}
			}
			return result;
		}

		private Sequence WrapInSequence(Construction construction)
		{
			if (construction is Sequence sequence)
			{
				return sequence;
			}
			return new Sequence
			{
				Values = new List<Construction>
				{
					construction
				}
			};
		}
	}
}
