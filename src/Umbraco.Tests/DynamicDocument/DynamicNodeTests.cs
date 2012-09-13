using System.IO;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using umbraco.BusinessLogic;
using umbraco.IO;
using umbraco.MacroEngines;
using umbraco.NodeFactory;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.template;
using System.Linq;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Tests.DynamicDocument
{
	[TestFixture]
	public class DynamicNodeTests : DynamicDocumentTestsBase<DynamicNode, DynamicNodeList>
	{
		protected override bool RequiresDbSetup
		{
			get { return true; }
		}

		public override void Initialize()
		{
			base.Initialize();
			//copy the umbraco settings file over
			var currDir = new DirectoryInfo(TestHelper.CurrentAssemblyDirectory);
			File.Copy(
				currDir.Parent.Parent.Parent.GetDirectories("Umbraco.Web.UI")
					.First()
					.GetDirectories("config").First()
					.GetFiles("umbracoSettings.Release.config").First().FullName,
				Path.Combine(currDir.Parent.Parent.FullName, "config", "umbracoSettings.config"), 
				true);

			UmbracoSettings.SettingsFilePath = IOHelper.MapPath(SystemDirectories.Config, false);

			//for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
			PluginManager.Current.AssembliesToScan = new[]
				{
					typeof(DynamicNode).Assembly
				};

		}

		public override void TearDown()
		{
			base.TearDown();

			PluginManager.Current.AssembliesToScan = null;
		}

		protected override dynamic GetDynamicNode(int id)
		{
			//var template = Template.MakeNew("test", new User(0));
			//var ctx = GetUmbracoContext("/test", template.Id);
			var ctx = GetUmbracoContext("/test", 1234);
			var contentStore = new DefaultPublishedContentStore();
			var node = new DynamicNode(
				new DynamicBackingItem(
					new Node(ctx.GetXml().SelectSingleNode("//*[@id='" + id + "' and @isDoc]"))));
			Assert.IsNotNull(node);
			return (dynamic) node;
		}
	}
}