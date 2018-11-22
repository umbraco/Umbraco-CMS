using NUnit.Framework;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    
    [TestFixture]
    public class ContentFinderByAliasWithDomainsTests : ContentFinderByAliasTests
    {

        

        [TestCase("http://domain1.com/this/is/my/alias", "de-DE", -1001)] // alias to domain's page fails - no alias on domain's home
        [TestCase("http://domain1.com/page2/alias", "de-DE", 10011)] // alias to sub-page works
        [TestCase("http://domain1.com/en/flux", "en-US", -10011)] // alias to domain's page fails - no alias on domain's home
        [TestCase("http://domain1.com/endanger", "de-DE", 10011)] // alias to sub-page works, even with "en..."
        [TestCase("http://domain1.com/en/endanger", "en-US", -10011)] // no
        [TestCase("http://domain1.com/only/one/alias", "de-DE", 100111)] // ok
        [TestCase("http://domain1.com/entropy", "de-DE", 100111)] // ok
        [TestCase("http://domain1.com/bar/foo", "de-DE", 100111)] // ok
        [TestCase("http://domain1.com/en/bar/foo", "en-US", -100111)] // no, alias must include "en/"
        [TestCase("http://domain1.com/en/bar/nil", "en-US", 100111)] // ok, alias includes "en/"
        public void Lookup_By_Url_Alias_And_Domain(string inputUrl, string expectedCulture, int expectedNode)
        {
            //SetDomains1();

            var routingContext = GetRoutingContext(inputUrl);
            var url = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
            var pcr = new PublishedContentRequest(url, routingContext);
            // must lookup domain
            pcr.Engine.FindDomain();

            if (expectedNode > 0)
                Assert.AreEqual(expectedCulture, pcr.Culture.Name);

            var finder = new ContentFinderByUrlAlias();
            var result = finder.TryFindContent(pcr);

            if (expectedNode > 0)
            {
                Assert.IsTrue(result);
                Assert.AreEqual(pcr.PublishedContent.Id, expectedNode);
            }
            else
            {
                Assert.IsFalse(result);
            }
        }
    }
}