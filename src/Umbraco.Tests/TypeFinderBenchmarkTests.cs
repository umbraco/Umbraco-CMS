using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Tests;
using Umbraco.Tests.PartialTrust;
using umbraco;
using umbraco.DataLayer;
using umbraco.MacroEngines;
using umbraco.MacroEngines.Iron;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.uicontrols;

[assembly: TypeFinderBenchmarkTests.AssemblyContainsPluginsAttribute]

namespace Umbraco.Tests
{

	/// <summary>
	/// Full Trust benchmark tests for typefinder and the old typefinder
	/// </summary>
	[TestFixture]
	[Ignore("This is a benchark test")]
	public class TypeFinderBenchmarkTests
	{
		[SetUp]
		public void Initialize()
		{
			//we need to copy the umbracoSettings file to the output folder!
			File.Copy(
				Path.Combine(TestHelper.CurrentAssemblyDirectory, "..\\..\\..\\Umbraco.Web.UI\\config\\umbracoSettings.Release.config"),
				Path.Combine(TestHelper.CurrentAssemblyDirectory, "umbracoSettings.config"), 
				true);
			UmbracoSettings.SettingsFilePath = TestHelper.CurrentAssemblyDirectory + "\\";
		}

		/// <summary>
		/// Informs the system that the assembly tagged with this attribute contains plugins and the system should scan and load them
		/// </summary>
		[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
		public class AssemblyContainsPluginsAttribute : Attribute
		{
		}

		public abstract class TestEditor
		{

		}

		public class BenchmarkTestEditor : TestEditor
		{

		}

		[Test]
		public void Benchmark_Old_TypeFinder_vs_New_TypeFinder_FindClassesWithAttribute()
		{
			var timer = new Stopwatch();
			var assemblies = new[]
			    {
			        //both contain the type
			        this.GetType().Assembly, 
			        //these dont contain the type
			        typeof(ApplicationStartupHandler).Assembly,
			        typeof(SqlCEHelper).Assembly,
			        typeof(CMSNode).Assembly,
			        typeof(System.Guid).Assembly,
			        typeof(NUnit.Framework.Assert).Assembly,
			        typeof(Microsoft.CSharp.CSharpCodeProvider).Assembly,
			        typeof(System.Xml.NameTable).Assembly,
			        typeof(System.Configuration.GenericEnumConverter).Assembly,
			        typeof(System.Web.SiteMap).Assembly,
			        typeof(TabPage).Assembly,
			        typeof(System.Web.Mvc.ActionResult).Assembly,
			        typeof(TypeFinder2).Assembly,
			        typeof(ISqlHelper).Assembly,
			        typeof(DLRScriptingEngine).Assembly,
			        typeof(ICultureDictionary).Assembly
			    };

			var finder2 = new Umbraco.Core.TypeFinder2();

			timer.Start();
			var found1 = finder2.FindClassesWithAttribute<XsltExtensionAttribute>(assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find class with XsltExtensionAttribute (" + found1.Count() + ") in " + assemblies.Count() + " assemblies using new TypeFinder: " + timer.ElapsedMilliseconds);

			timer.Start();
			var found2 = umbraco.BusinessLogic.Utils.TypeFinder.FindClassesMarkedWithAttribute(typeof(XsltExtensionAttribute), assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find class with XsltExtensionAttribute (" + found2.Count() + ") in " + assemblies.Count() + " assemblies using old TypeFinder: " + timer.ElapsedMilliseconds);
		}

		[Test]
		public void Benchmark_Old_TypeFinder_vs_New_TypeFinder_FindClassesOfType()
		{
			var timer = new Stopwatch();
			var assemblies = new[]
			    {
			        //both contain the type
			        this.GetType().Assembly, 
			        //these dont contain the type
			        typeof(ApplicationStartupHandler).Assembly,
			        typeof(SqlCEHelper).Assembly,
			        typeof(CMSNode).Assembly,
			        typeof(System.Guid).Assembly,
			        typeof(NUnit.Framework.Assert).Assembly,
			        typeof(Microsoft.CSharp.CSharpCodeProvider).Assembly,
			        typeof(System.Xml.NameTable).Assembly,
			        typeof(System.Configuration.GenericEnumConverter).Assembly,
			        typeof(System.Web.SiteMap).Assembly,
			        typeof(TabPage).Assembly,
			        typeof(System.Web.Mvc.ActionResult).Assembly,
			        typeof(TypeFinder2).Assembly,
			        typeof(ISqlHelper).Assembly,
			        typeof(DLRScriptingEngine).Assembly,
			        typeof(ICultureDictionary).Assembly
			    };

			var finder2 = new Umbraco.Core.TypeFinder2();

			timer.Start();
			var found1 = finder2.FindClassesOfType<TestEditor>(assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find TestEditor (" + found1.Count() + ") in " + assemblies.Count() + " assemblies using new TypeFinder: " + timer.ElapsedMilliseconds);

			timer.Start();
			var found2 = umbraco.BusinessLogic.Utils.TypeFinder.FindClassesOfType<TestEditor>(true, true, assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find TestEditor (" + found2.Count() + ") in " + assemblies.Count() + " assemblies using old TypeFinder: " + timer.ElapsedMilliseconds);
		}


		[Test]
		public void Benchmark_Finding_First_Type_In_Assemblies()
		{
			var timer = new Stopwatch();
			var assemblies = new[]
			    {
			        //both contain the type
			        this.GetType().Assembly, 
			        //these dont contain the type
			        typeof(ApplicationStartupHandler).Assembly,
			        typeof(SqlCEHelper).Assembly,
			        typeof(CMSNode).Assembly,
			        typeof(System.Guid).Assembly,
			        typeof(NUnit.Framework.Assert).Assembly,
			        typeof(Microsoft.CSharp.CSharpCodeProvider).Assembly,
			        typeof(System.Xml.NameTable).Assembly,
			        typeof(System.Configuration.GenericEnumConverter).Assembly,
			        typeof(System.Web.SiteMap).Assembly,
			        typeof(TabPage).Assembly,
			        typeof(System.Web.Mvc.ActionResult).Assembly,
			        typeof(TypeFinder2).Assembly,
			        typeof(ISqlHelper).Assembly,
			        typeof(DLRScriptingEngine).Assembly,
			        typeof(ICultureDictionary).Assembly
			    };

			var finder = new Umbraco.Core.TypeFinder2();

			timer.Start();
			var found1 = finder.FindClassesOfType<TestEditor, AssemblyContainsPluginsAttribute>(assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find TestEditor (" + found1.Count() + ") in " + assemblies.Count() + " assemblies using AssemblyContainsPluginsAttribute: " + timer.ElapsedMilliseconds);

			timer.Start();
			var found2 = finder.FindClassesOfType<TestEditor>(assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find TestEditor (" + found2.Count() + ") in " + assemblies.Count() + " assemblies without AssemblyContainsPluginsAttribute: " + timer.ElapsedMilliseconds);

		}

		
	}
}