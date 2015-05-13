using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.web;
using Umbraco.Tests.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Integration
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class GetCultureTests : BaseServiceTest
    {
        protected override void FreezeResolution()
        {
            SiteDomainHelperResolver.Current = new SiteDomainHelperResolver(new SiteDomainHelper());

            base.FreezeResolution();
        }

        [Test]
        public void GetCulture()
        {
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbBlah", "test Doc Type");
            contentTypeService.Save(contentType);
            var contentService = ServiceContext.ContentService;

            var c1 = contentService.CreateContentWithIdentity("content", -1, "umbBlah");
            var c2 = contentService.CreateContentWithIdentity("content", c1, "umbBlah");
            var c3 = contentService.CreateContentWithIdentity("content", c1, "umbBlah");
            var c4 = contentService.CreateContentWithIdentity("content", c3, "umbBlah");

            var langs = Language.GetAllAsList();
            foreach (var l in langs.Skip(1))
                l.Delete();

            Language.MakeNew("fr-FR");
            Language.MakeNew("de-DE");

            var langEn = Language.GetByCultureCode("en-US");
            var langFr = Language.GetByCultureCode("fr-FR");
            var langDe = Language.GetByCultureCode("de-DE");

            var domains = Domain.GetDomains(true); // we want wildcards too here
            foreach (var d in domains)
                d.Delete();

            Domain.MakeNew("domain1.com/", c1.Id, langEn.id);
            Domain.MakeNew("domain1.fr/", c1.Id, langFr.id);
            Domain.MakeNew("*100112", c3.Id, langDe.id);

            var content = c2;
            var culture = Web.Models.ContentExtensions.GetCulture(null, ServiceContext.LocalizationService, ServiceContext.ContentService, content.Id, content.Path, 
                new Uri("http://domain1.com/"));
            Assert.AreEqual("en-US", culture.Name);

            content = c2;
            culture = Web.Models.ContentExtensions.GetCulture(null, ServiceContext.LocalizationService, ServiceContext.ContentService, content.Id, content.Path,
                new Uri("http://domain1.fr/"));
            Assert.AreEqual("fr-FR", culture.Name);

            content = c4;
            culture = Web.Models.ContentExtensions.GetCulture(null, ServiceContext.LocalizationService, ServiceContext.ContentService, content.Id, content.Path,
                new Uri("http://domain1.fr/"));
            Assert.AreEqual("de-DE", culture.Name);
        }
    }
}
