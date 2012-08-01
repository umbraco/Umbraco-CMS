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
			TestHelper.SetupLog4NetForTests();

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
			        typeof(TypeFinder).Assembly,
			        typeof(ISqlHelper).Assembly,
			        typeof(DLRScriptingEngine).Assembly,
			        typeof(ICultureDictionary).Assembly
			    };

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

			var typesFound = TypeFinder.FindClassesOfTypeWithAttribute<TestEditor, MyTestAttribute>(_assemblies);

			Assert.AreEqual(2, typesFound.Count());

		}
	
		
	}
}