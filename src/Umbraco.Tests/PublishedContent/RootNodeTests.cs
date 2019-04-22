using System.Linq;
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
            var ctx = GetUmbracoContext("/test");

            // there is no content node with ID -1
            var content = ctx.Content.GetById(-1);
            Assert.IsNull(content);

            // content at root has null parent
            content = ctx.Content.GetById(1046);
            Assert.IsNotNull(content);
            Assert.AreEqual(1, content.Level);
            Assert.IsNull(content.Parent());

            // non-existing content is null
            content = ctx.Content.GetById(666);
            Assert.IsNull(content);
        }

    }
}
