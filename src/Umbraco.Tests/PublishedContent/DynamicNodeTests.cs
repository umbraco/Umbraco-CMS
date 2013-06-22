using System;
using System.IO;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using umbraco.MacroEngines;
using umbraco.NodeFactory;
using System.Linq;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class DynamicNodeTests : DynamicDocumentTestsBase<DynamicNode, DynamicNodeList>
    {
        /// <summary>
        /// We only need a new schema per fixture... speeds up testing
        /// </summary>
        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get { return DatabaseBehavior.NewSchemaPerFixture; }
        }

        public override void Initialize()
        {
            base.Initialize();
            //copy the umbraco settings file over
            var currDir = new DirectoryInfo(TestHelper.CurrentAssemblyDirectory);

            var configPath = Path.Combine(currDir.Parent.Parent.FullName, "config");
            if (Directory.Exists(configPath) == false)
                Directory.CreateDirectory(configPath);

            var umbracoSettingsFile = Path.Combine(currDir.Parent.Parent.FullName, "config", "umbracoSettings.config");
            if (File.Exists(umbracoSettingsFile) == false)
                File.Copy(
                    currDir.Parent.Parent.Parent.GetDirectories("Umbraco.Web.UI")
                        .First()
                        .GetDirectories("config").First()
                        .GetFiles("umbracoSettings.Release.config").First().FullName,
                    Path.Combine(currDir.Parent.Parent.FullName, "config", "umbracoSettings.config"),
                    true);

            UmbracoSettings.SettingsFilePath = IOHelper.MapPath(SystemDirectories.Config + Path.DirectorySeparatorChar, false);

            //need to specify a custom callback for unit tests
            DynamicNode.GetDataTypeCallback = (docTypeAlias, propertyAlias) =>
            {
                if (propertyAlias == "content")
                {
                    //return the rte type id
                    return Guid.Parse(Constants.PropertyEditors.TinyMCEv3);
                }
                return Guid.Empty;
            };

        }

        [Test]
        [Ignore("This test will never work unless DynamicNode is refactored a lot in order to get a list of root nodes since root nodes don't have a parent to look up")]
        public override void Is_First_Root_Nodes()
        {
            base.Is_First_Root_Nodes();
        }

        [Test]
        [Ignore("This test will never work unless DynamicNode is refactored a lot in order to get a list of root nodes since root nodes don't have a parent to look up")]
        public override void Is_Not_First_Root_Nodes()
        {
            base.Is_Not_First_Root_Nodes();
        }

        [Test]
        [Ignore("This test will never work unless DynamicNode is refactored a lot in order to get a list of root nodes since root nodes don't have a parent to look up")]
        public override void Is_Position_Root_Nodes()
        {
            base.Is_Position_Root_Nodes();
        }

        public override void TearDown()
        {
            //TODO: Deleting the umbracoSettings.config file makes a lot of tests fail

            //var currDir = new DirectoryInfo(TestHelper.CurrentAssemblyDirectory);

            //var umbracoSettingsFile = Path.Combine(currDir.Parent.Parent.FullName, "config", "umbracoSettings.config");
            //if (File.Exists(umbracoSettingsFile))
            //    File.Delete(umbracoSettingsFile);

            base.TearDown();
        }

        protected override dynamic GetDynamicNode(int id)
        {
            //var template = Template.MakeNew("test", new User(0));
            //var ctx = GetUmbracoContext("/test", template.Id);
            var ctx = GetUmbracoContext("/test", 1234);

            var cache = ctx.ContentCache.InnerCache as PublishedContentCache;
            if (cache == null) throw new Exception("Unsupported IPublishedContentCache, only the Xml one is supported.");

            var node = new DynamicNode(
                new DynamicBackingItem(
                    new Node(cache.GetXml(ctx, ctx.InPreviewMode).SelectSingleNode("//*[@id='" + id + "' and @isDoc]"))));
            Assert.IsNotNull(node);
            return (dynamic)node;
        }
    }
}