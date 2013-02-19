using System;
using System.IO;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using umbraco.IO;
using umbraco.MacroEngines;
using umbraco.NodeFactory;
using System.Linq;

namespace Umbraco.Tests.PublishedContent
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

            UmbracoSettings.SettingsFilePath = IOHelper.MapPath(SystemDirectories.Config + Path.DirectorySeparatorChar, false);

            //for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
            PluginManager.Current.AssembliesToScan = new[]
				{
					typeof(DynamicNode).Assembly
				};

            //need to specify a custom callback for unit tests
            DynamicNode.GetDataTypeCallback = (docTypeAlias, propertyAlias) =>
            {
                if (propertyAlias == "content")
                {
                    //return the rte type id
                    return Guid.Parse("5e9b75ae-face-41c8-b47e-5f4b0fd82f83");
                }
                return Guid.Empty;
            };

        }

        public override void TearDown()
        {
            base.TearDown();

            PluginManager.Current.AssembliesToScan = null;

            UmbracoSettings.ResetSetters();
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
            return (dynamic)node;
        }
    }
}