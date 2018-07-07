using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorkflowToCSharp.Converter.Data;
using WorkflowToCSharp.Converter.Extensions;

namespace WorkflowToCSharp.Converter
{
	public interface ClassCodeToCSharp
	{
		string Convert(ClassCode classCode);
	}

	public class ClassCodeToCSharpImpl : ClassCodeToCSharp
	{
		private readonly FieldManager fieldManager;

		public ClassCodeToCSharpImpl(FieldManager fieldManager)
		{
			this.fieldManager = fieldManager;
		}

		public string Convert(ClassCode classCode)
		{
			var writer = new StringWriter();
			fieldManager.ParseAndInitialize(classCode.CodeBlocks);
			WriteUsingNamespaces(writer, classCode.UsingNamespaces);
			WriteClass(writer, classCode);
			return writer.ToString();
		}

		private void WriteUsingNamespaces(StringWriter writer, List<string> usingNamespaces)
		{
			foreach (string usingNamespace in usingNamespaces)
			{
				writer.WriteLine($"using {usingNamespace};");
			}
		}

		private void WriteClass(StringWriter writer, ClassCode classCode)
		{
			writer.WriteLine();
			writer.WriteLine("namespace " + classCode.Namespace);
			writer.WriteLine("{");
			writer.WriteLineTabs("public class " + classCode.Name, 1);
			writer.WriteLineTabs("{", 1);
			WriteCodeBlocks(writer, classCode.CodeBlocks, 2);
			writer.WriteLineTabs("}", 1);
			writer.WriteLine("}");
		}

		private void WriteCodeBlocks(StringWriter writer, List<Code> codeBlocks, int tabs)
		{
			foreach (Code code in codeBlocks)
			{
				switch (code)
				{
					case PropertyCode property:
						WriteProperty(writer, property, tabs);
						break;
					case Method method:
						WriteMethod(writer, method, tabs);
						break;
				}
			}
		}

		private void WriteProperty(StringWriter writer, PropertyCode property, int tabs)
		{
			writer.WriteLineTabs($"public {property.Type} {property.Name} " + "{ get; set; }", tabs);
		}

		private void WriteMethod(StringWriter writer, Method method, int tabs)
		{
			writer.WriteLine();
			writer.WriteLineTabs($"{method.AccessModify} {method.ReturnType} {method.Name}({ConvertParameters(method.InArguments, x => x.Type + " " + x.Name)})", tabs);
			WriteConstruction(writer, method.Sequence, tabs + 1);
		}

		private void WriteConstruction(StringWriter writer, Construction construction, int tabs)
		{
			switch (construction)
			{
				case Sequence sequence:
					WriteSequence(writer, sequence, tabs);
					break;
				case IfCode ifCode:
					WriteIfCode(writer, ifCode, tabs);
					break;
				/*case VariableCode variable:
					writer.WriteLineTabs($"{variable.Type} {variable.Name};", tabs);
					break;*/
				case TryCatchCode tryCatchCode:
					WriteTryCatch(writer, tryCatchCode, tabs);
					break;
				case AssignCode assignCode:
					WriteAssign(writer, assignCode, tabs);
					break;
				case CustomActivityCode customActivityCode:
					WriteCustomActivity(writer, customActivityCode, tabs);
					break;
				case StringCode stringCode:
					writer.WriteLineTabs(stringCode.Value + ";", tabs);
					break;
			}
		}

		private void WriteSequence(StringWriter writer, Sequence sequence, int tabs)
		{
			writer.WriteLineTabs("{", tabs - 1);
			foreach (Construction construction in sequence.Values)
			{
				WriteConstruction(writer, construction, tabs);
			}
			writer.WriteLineTabs("}", tabs - 1);
		}

		private void WriteIfCode(StringWriter writer, IfCode ifCode, int tabs)
		{
			writer.WriteLineTabs($"if ({ifCode.Condition})", tabs);
			WriteConstruction(writer, ifCode.Then, tabs + 1);
			if (ifCode.Else != null)
			{
				writer.WriteLineTabs("else", tabs);
				WriteConstruction(writer, ifCode.Else, tabs + 1);
			}
		}

		private void WriteTryCatch(StringWriter writer, TryCatchCode tryCatchCode, int tabs)
		{
			writer.WriteLineTabs("try", tabs);
			WriteConstruction(writer, tryCatchCode.Try, tabs + 1);
			foreach (CatchCode catchCode in tryCatchCode.Catches)
			{
				writer.WriteLineTabs($"catch ({catchCode.Exception})", tabs);
				WriteConstruction(writer, catchCode.Body, tabs + 1);
			}
		}

		private void WriteAssign(StringWriter writer, AssignCode assign, int tabs)
		{
			VariableCode variable = fieldManager.GetVariable(assign.To);
			string text = $"{assign.To} = {assign.Value};";
			if (variable != null)
			{
				if (!variable.WasInitialized)
				{
					text = $"{variable.Type} {assign.To} = {assign.Value};";
					variable.WasInitialized = true;
				}
			}
			writer.WriteLineTabs(text, tabs);
		}

		private void WriteCustomActivity(StringWriter writer, CustomActivityCode customActivityCode, int tabs)
		{
			if (customActivityCode.Method != null)
			{
				foreach (VariableCode variable in customActivityCode.Method.InArguments)
				{
					if (!variable.WasInitialized)
					{
						writer.WriteLineTabs($"{variable.Type} {variable.Name};", tabs);
						variable.WasInitialized = true;
					}
				}
				if (string.IsNullOrWhiteSpace(customActivityCode.ResultTo))
				{
					writer.WriteLineTabs($"{customActivityCode.Method.Name}({string.Join(", ", ConvertParameters(customActivityCode.Method.InArguments, x => x.Name))});", tabs);
				}
				else
				{
					WriteAssign(writer, new AssignCode
					{
						To = customActivityCode.ResultTo,
						Value = $"{customActivityCode.Method.Name}({ConvertParameters(customActivityCode.Method.InArguments, x => x.Name)})"
					}, tabs);
				}
			}
			else
			{
				writer.WriteLineTabs($"var {customActivityCode.Name} = new {customActivityCode.Name}();", tabs);
				foreach (AssignCode assignCode in customActivityCode.Assigns)
				{
					if (assignCode.Value != null)
					{
						writer.WriteLineTabs($"{customActivityCode.Name}.{assignCode.To} = {assignCode.Value};", tabs);
					}
					else
					{
						writer.WriteLineTabs($"// {customActivityCode.Name}.{assignCode.To} = {assignCode.Value};", tabs);
					}
				}
				writer.WriteLineTabs($"{customActivityCode.Name}.Execute();", tabs);
			}
		}

		private string ConvertParameters(List<VariableCode> parameters, Func<VariableCode, string> convertor)
		{
			if (parameters is null)
			{
				return "";
			}
			return string.Join(", ", parameters.Select(convertor));
		}
	}
}
