using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using umbraco;
using umbraco.MacroEngines;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Security;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class RootNodeTests : PublishedContentTestBase
    {
        [Test]
        public void PublishedContentHasNoRootNode()
        {
            var ctx = GetUmbracoContext("/test", 1234);

            // there is no content node with ID -1
            var content = ctx.ContentCache.GetById(-1);
            Assert.IsNull(content);

            // content at root has null parent
            content = ctx.ContentCache.GetById(1046);
            Assert.IsNotNull(content);
            Assert.AreEqual(1, content.Level);
            Assert.IsNull(content.Parent);

            // and yet is has siblings, etc.
            var siblings = content.Siblings();
            Assert.AreEqual(2, siblings.Count());

            // non-existing content is null
            content = ctx.ContentCache.GetById(666);
            Assert.IsNull(content);
        }

        [Test]
        public void LegacyDynamicNodeSortOfHasRootNode()
        {
            // there is a node with ID -1
            var node = new DynamicNode(-1);
            Assert.IsNotNull(node);
            Assert.AreEqual(-1, node.Id);

            // content at root
            node = new DynamicNode(1046);
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.Level);

            // has no parent
            // (confirmed in 4.7 and 6.1)
            Assert.IsNull(node.Parent);

            // has siblings etc - no idea how we're supposed to get them?
            //var siblings = node.Parent.Children; 
            //Assert.AreEqual(2, siblings.Count());

            // non-existing content is "zero node"
            node = new DynamicNode(666, DynamicBackingItemType.Content); // set type to avoid Examine in tests
            Assert.IsNotNull(node);
            Assert.AreEqual(0, node.Id);
        }

        [Test]
        public void Fix_U4_4374()
        {
            var node = new DynamicNode(-1);
            var id = node.DescendantsOrSelf().First().Id;
            Assert.AreEqual(-1, id);
        }
    }
}
