using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Web;
using umbraco.DataLayer;
using umbraco.MacroEngines;
using umbraco.MacroEngines.Iron;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.editorControls;
using umbraco.uicontrols;
using System.Linq;

namespace Umbraco.Tests
{
	[TestFixture]
	public class DataTypeFactoryTests
	{
		[SetUp]
		public void Initialize()
		{
			//for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
			PluginTypeResolver.Current.AssembliesToScan = new[]
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
			        typeof(ICultureDictionary).Assembly,
					typeof(UmbracoContext).Assembly,
					typeof(BaseDataType).Assembly,

			    };
		}

		[Test]
		public void Find_All_DataTypes()
		{
			umbraco.cms.businesslogic.datatype.controls.Factory.Initialize();
			Assert.AreEqual(33, umbraco.cms.businesslogic.datatype.controls.Factory._controls.Count);
		}

		[Test]
		public void Get_All_Instances()
		{
			umbraco.cms.businesslogic.datatype.controls.Factory.Initialize();
			var factory = new umbraco.cms.businesslogic.datatype.controls.Factory();
			Assert.AreEqual(33, factory.GetAll().Count());
		}

	}
}