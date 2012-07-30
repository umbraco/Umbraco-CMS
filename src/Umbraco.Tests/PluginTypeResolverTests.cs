using System.Linq;
using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using umbraco;
using umbraco.DataLayer;
using umbraco.MacroEngines;
using umbraco.MacroEngines.Iron;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.editorControls;
using umbraco.presentation.umbracobase;
using umbraco.uicontrols;
using umbraco.cms;

namespace Umbraco.Tests
{

	[TestFixture]
	public class PluginTypeResolverTests
	{

		[SetUp]
		public void Initialize()
		{
			TestHelper.SetupLog4NetForTests();

			//this ensures its reset
			PluginTypeResolver.Current = new PluginTypeResolver();

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
					typeof(BaseDataType).Assembly
			    };
		}

		[Test]
		public void Ensure_Only_One_Type_List_Created()
		{
			var foundTypes1 = PluginTypeResolver.Current.ResolveFindMeTypes();
			var foundTypes2 = PluginTypeResolver.Current.ResolveFindMeTypes();
			Assert.AreEqual(1,
			                PluginTypeResolver.Current.GetTypeLists()
			                	.Count(x => x.GetListType() == typeof (IFindMe)));
		}

		[Test]
		public void Resolves_Types()
		{
			var foundTypes1 = PluginTypeResolver.Current.ResolveFindMeTypes();			
			Assert.AreEqual(2, foundTypes1.Count());
		}

		[Test]
		public void Resolves_Attributed_Trees()
		{
			var trees = PluginTypeResolver.Current.ResolveAttributedTrees();
			Assert.AreEqual(26, trees.Count());
		}

		[Test]
		public void Resolves_Actions()
		{
			var actions = PluginTypeResolver.Current.ResolveActions();
			Assert.AreEqual(36, actions.Count());
		}

		[Test]
		public void Resolves_Trees()
		{
			var trees = PluginTypeResolver.Current.ResolveTrees();
			Assert.AreEqual(36, trees.Count());
		}

		[Test]
		public void Resolves_Applications()
		{
			var apps = PluginTypeResolver.Current.ResolveApplications();
			Assert.AreEqual(7, apps.Count());
		}

		[Test]
		public void Resolves_Action_Handlers()
		{
			var types = PluginTypeResolver.Current.ResolveActionHandlers();
			Assert.AreEqual(1, types.Count());
		}

		[Test]
		public void Resolves_DataTypes()
		{
			var types = PluginTypeResolver.Current.ResolveDataTypes();
			Assert.AreEqual(36, types.Count());
		}

		[Test]
		public void Resolves_RazorDataTypeModels()
		{
			var types = PluginTypeResolver.Current.ResolveRazorDataTypeModels();
			Assert.AreEqual(1, types.Count());
		}

		[Test]
		public void Resolves_RestExtensions()
		{
			var types = PluginTypeResolver.Current.ResolveRestExtensions();
			Assert.AreEqual(1, types.Count());
		}

		[Test]
		public void Resolves_XsltExtensions()
		{
			var types = PluginTypeResolver.Current.ResolveXsltExtensions();
			Assert.AreEqual(1, types.Count());
		}

		[XsltExtension("Blah.Blah")]
		public class MyXsltExtension
		{

		}

		[RestExtension("Blah")]
		public class MyRestExtension
		{
			
		}

		public interface IFindMe
		{

		}

		public class FindMe1 : IFindMe
		{

		}

		public class FindMe2 : IFindMe
		{

		}

	}
}