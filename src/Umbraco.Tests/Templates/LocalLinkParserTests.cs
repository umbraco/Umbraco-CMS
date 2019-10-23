using NUnit.Framework;
using System.Linq;
using Umbraco.Core;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web.Templates;

namespace Umbraco.Tests.Templates
{
    [TestFixture]
    public class LocalLinkParserTests
    {
        [Test]
        public void Returns_Udis_From_LocalLinks()
        {
            var input = @"<p>
    <div>
        <img src='/media/12312.jpg' data-udi='umb://media/D4B18427A1544721B09AC7692F35C264' />
        <a href=""{locallink:umb://document/C093961595094900AAF9170DDE6AD442}"">hello</a>
    </div>
</p><p><img src='/media/234234.jpg' data-udi=""umb://media-type/B726D735E4C446D58F703F3FBCFC97A5"" />
<a href=""{locallink:umb://document-type/2D692FCB070B4CDA92FB6883FDBFD6E2}"">hello</a>
</p>";

            var umbracoContextAccessor = new TestUmbracoContextAccessor();
            var parser = new HtmlLocalLinkParser(umbracoContextAccessor);

            var result = parser.FindUdisFromLocalLinks(input).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Udi.Parse("umb://document/C093961595094900AAF9170DDE6AD442"), result[0]);
            Assert.AreEqual(Udi.Parse("umb://document-type/2D692FCB070B4CDA92FB6883FDBFD6E2"), result[1]);
        }
    }
}
