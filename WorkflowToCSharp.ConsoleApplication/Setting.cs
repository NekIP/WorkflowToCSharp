using System;
using LightInject;
using WorkflowToCSharp.Assistants;
using WorkflowToCSharp.Converter;

namespace WorkflowToCSharp.ConsoleApplication
{
	public class Setting
	{
		private readonly ServiceContainer container;
		private readonly string startupServiceName = "Startup";

		public Setting()
		{
			container = new ServiceContainer();
		}

		public void InitializeDependences()
		{
			container.Register<Startup, StartupImpl>(startupServiceName);
			container.Register<ConsoleAssistant, ConsoleAssistantImpl>();
			container.Register<XamlParser, XamlParserImpl>();
			container.Register<WorkflowToCSharpConverter, WorkflowToCSharpConverterFromXaml>();
			container.Register<ClassCodeToCSharp, ClassCodeToCSharpImpl>();
			container.Register<ClassGenerator, ClassGeneratorImpl>();
			container.Register<MethodGenerator, MethodGeneratorImpl>();
			container.Register<CodeRefactor, CodeRefactorImpl>();
			container.Register<FieldManager, FieldManagerImpl>();
			container.Register<Log, ConsoleLog>();
		}

		public void RunApplication(string[] args)
		{
			if (!container.CanGetInstance(typeof(Startup), startupServiceName))
			{
				throw new Exception("Dependences wasn't initialised! Please, provide call 'InitializeDependences' before 'RunApplication'.");
			}
			var startup = container.TryGetInstance<Startup>();
			startup.Execute(args);
		}
	}
}
