using System.Linq;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Tests.Routing
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture]
    public class ContentFinderByAliasWithDomainsTests : ContentFinderByAliasTests
    {
        public override void Initialize()
        {
            base.Initialize();

            // ensure we can create them although the content is not in the database
            TestHelper.DropForeignKeys("umbracoDomains");

            InitializeLanguagesAndDomains();
        }

        void InitializeLanguagesAndDomains()
        {
            var domains = Domain.GetDomains();
            foreach (var d in domains)
                d.Delete();

            var langs = Language.GetAllAsList();
            foreach (var l in langs.Skip(1))
                l.Delete();

            // en-US is there by default
            Language.MakeNew("fr-FR");
            Language.MakeNew("de-DE");
        }

        void SetDomains1()
        {
            var langEn = Language.GetByCultureCode("en-US");
            var langFr = Language.GetByCultureCode("fr-FR");
            var langDe = Language.GetByCultureCode("de-DE");

            Domain.MakeNew("domain1.com/", 1001, langDe.id);
            Domain.MakeNew("domain1.com/en", 10011, langEn.id);
            Domain.MakeNew("domain1.com/fr", 10012, langFr.id);
        }
        

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
            SetDomains1();

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