using System.Linq;
using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Web;
using umbraco.DataLayer;
using umbraco.MacroEngines;
using umbraco.MacroEngines.Iron;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.uicontrols;

namespace Umbraco.Tests
{
	public class PluginTypeResolverTests
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
					typeof(UmbracoContext).Assembly

			    };
		}

		[Test]
		public void Ensure_Only_One_Type_List_Created()
		{
			var foundTypes1 = PluginTypeResolver.Current.ResolveFindMeTypes();
			var foundTypes2 = PluginTypeResolver.Current.ResolveFindMeTypes();
			Assert.AreEqual(1, PluginTypeResolver.Current.GetTypeLists().Count);
		}

		[Test]
		public void Resolves_Types()
		{
			var foundTypes1 = PluginTypeResolver.Current.ResolveFindMeTypes();			
			Assert.AreEqual(2, foundTypes1.Count());
		}

		[Test]
		public void Resolves_Trees()
		{
			var trees = PluginTypeResolver.Current.ResolveTrees();
			Assert.AreEqual(26, trees.Count());
		}

		[Test]
		public void Resolves_Applications()
		{
			var apps = PluginTypeResolver.Current.ResolveApplications();
			Assert.AreEqual(7, apps.Count());
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