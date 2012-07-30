using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Tests;
using Umbraco.Tests.PartialTrust;
using Umbraco.Tests.TestHelpers;
using umbraco;
using umbraco.DataLayer;
using umbraco.MacroEngines;
using umbraco.MacroEngines.Iron;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.uicontrols;

[assembly: TypeFinderTests.AssemblyContainsPluginsAttribute]

namespace Umbraco.Tests
{
	/// <summary>
	/// Full Trust benchmark tests for typefinder and the old typefinder
	/// </summary>
	[TestFixture]	
	public class TypeFinderTests
	{
		/// <summary>
		/// List of assemblies to scan
		/// </summary>
		private Assembly[] _assemblies;

		[SetUp]
		public void Initialize()
		{
			_assemblies = new[]
			    {
			        this.GetType().Assembly, 
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

		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		public class MyTestAttribute : Attribute
		{
			
		}

		public abstract class TestEditor
		{

		}

		[MyTestAttribute]
		public class BenchmarkTestEditor : TestEditor
		{

		}

		[MyTestAttribute]
		public class MyOtherTestEditor : TestEditor
		{

		}

		[Test]
		public void Get_Type_With_Attribute()
		{

			var finder2 = new Umbraco.Core.TypeFinder2();

			var typesFound = finder2.FindClassesOfTypeWithAttribute<TestEditor, MyTestAttribute>(_assemblies);

			Assert.AreEqual(2, typesFound.Count());

		}
		
		[Test]
		[Ignore("This is a benchark test")]
		public void Benchmark_Old_TypeFinder_vs_New_TypeFinder_FindClassesWithAttribute()
		{
			var timer = new Stopwatch();

			var finder2 = new Umbraco.Core.TypeFinder2();

			timer.Start();
			var found1 = finder2.FindClassesWithAttribute<XsltExtensionAttribute>(_assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find class with XsltExtensionAttribute (" + found1.Count() + ") in " + _assemblies.Count() + " assemblies using new TypeFinder: " + timer.ElapsedMilliseconds);

			timer.Start();
			var found2 = umbraco.BusinessLogic.Utils.TypeFinder.FindClassesMarkedWithAttribute(typeof(XsltExtensionAttribute), _assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find class with XsltExtensionAttribute (" + found2.Count() + ") in " + _assemblies.Count() + " assemblies using old TypeFinder: " + timer.ElapsedMilliseconds);

			Assert.AreEqual(found1.Count(), found2.Count());
		}

		[Test]
		[Ignore("This is a benchark test")]
		public void Benchmark_Old_TypeFinder_vs_New_TypeFinder_FindClassesOfType()
		{
			var timer = new Stopwatch();			

			var finder2 = new Umbraco.Core.TypeFinder2();

			timer.Start();
			var found1 = finder2.FindClassesOfType<TestEditor>(_assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find TestEditor (" + found1.Count() + ") in " + _assemblies.Count() + " assemblies using new TypeFinder: " + timer.ElapsedMilliseconds);

			timer.Start();
			var found2 = umbraco.BusinessLogic.Utils.TypeFinder.FindClassesOfType<TestEditor>(true, true, _assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find TestEditor (" + found2.Count() + ") in " + _assemblies.Count() + " assemblies using old TypeFinder: " + timer.ElapsedMilliseconds);

			Assert.AreEqual(found1.Count(), found2.Count());
		}

		/// <summary>
		/// To indicate why we had the AssemblyContainsPluginsAttribute attribute in v5, now just need to get the .hash and assembly 
		/// cache files created instead since this is clearly a TON faster.
		/// </summary>
		[Test]
		[Ignore("This is a benchark test")]
		public void Benchmark_Finding_First_Type_In_Assemblies()
		{
			var timer = new Stopwatch();			

			var finder = new Umbraco.Core.TypeFinder2();

			timer.Start();
			var found1 = finder.FindClassesOfType<TestEditor, AssemblyContainsPluginsAttribute>(_assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find TestEditor (" + found1.Count() + ") in " + _assemblies.Count() + " assemblies using AssemblyContainsPluginsAttribute: " + timer.ElapsedMilliseconds);

			timer.Start();
			var found2 = finder.FindClassesOfType<TestEditor>(_assemblies);
			timer.Stop();

			Console.WriteLine("Total time to find TestEditor (" + found2.Count() + ") in " + _assemblies.Count() + " assemblies without AssemblyContainsPluginsAttribute: " + timer.ElapsedMilliseconds);

			Assert.AreEqual(found1.Count(), found2.Count());
		}

		
	}
}