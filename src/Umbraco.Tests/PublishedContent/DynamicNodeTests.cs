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
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture]
    public class DynamicNodeTests : DynamicDocumentTestsBase<DynamicNode, DynamicNodeList>
    {

        public override void Initialize()
        {
            base.Initialize();
           
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