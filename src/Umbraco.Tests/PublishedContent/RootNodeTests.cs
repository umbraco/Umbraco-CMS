﻿using System.Linq;
using NUnit.Framework;
using Umbraco.Web;

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

    }
}
