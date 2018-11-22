using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models;
using Umbraco.Tests.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Routing;
using Language = umbraco.cms.businesslogic.language.Language;

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

            foreach (var l in ServiceContext.LocalizationService.GetAllLanguages().Where(x => x.CultureName != "en-US").ToArray())
                ServiceContext.LocalizationService.Delete(l);

            var l0 = ServiceContext.LocalizationService.GetLanguageByIsoCode("en-US");
            var l1 = new Core.Models.Language("fr-FR");
            var l2 = new Core.Models.Language("de-DE");
            ServiceContext.LocalizationService.Save(l1);
            ServiceContext.LocalizationService.Save(l2);

            foreach (var d in ServiceContext.DomainService.GetAll(true).ToArray())
                ServiceContext.DomainService.Delete(d);

            ServiceContext.DomainService.Save(new UmbracoDomain("domain1.com") {DomainName="domain1.com", RootContentId = c1.Id, LanguageId = l0.Id});
            ServiceContext.DomainService.Save(new UmbracoDomain("domain1.fr") { DomainName = "domain1.fr", RootContentId = c1.Id, LanguageId = l1.Id });
            ServiceContext.DomainService.Save(new UmbracoDomain("*100112") { DomainName = "*100112", RootContentId = c3.Id, LanguageId = l2.Id });

            var content = c2;
            var culture = global::Umbraco.Web.Models.ContentExtensions.GetCulture(null,
                ServiceContext.DomainService, ServiceContext.LocalizationService, ServiceContext.ContentService,
                content.Id, content.Path, new Uri("http://domain1.com/"));
            Assert.AreEqual("en-US", culture.Name);

            content = c2;
            culture = global::Umbraco.Web.Models.ContentExtensions.GetCulture(null,
                ServiceContext.DomainService, ServiceContext.LocalizationService, ServiceContext.ContentService, 
                content.Id, content.Path, new Uri("http://domain1.fr/"));
            Assert.AreEqual("fr-FR", culture.Name);

            content = c4;
            culture = global::Umbraco.Web.Models.ContentExtensions.GetCulture(null,
                ServiceContext.DomainService, ServiceContext.LocalizationService, ServiceContext.ContentService, 
                content.Id, content.Path, new Uri("http://domain1.fr/"));
            Assert.AreEqual("de-DE", culture.Name);
        }

        [Test]
        public void GetCultureWithWildcard()
        {
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbBlah", "test Doc Type");
            contentTypeService.Save(contentType);
            var contentService = ServiceContext.ContentService;

            var c1 = contentService.CreateContentWithIdentity("content", -1, "umbBlah");
            var c2 = contentService.CreateContentWithIdentity("content", c1, "umbBlah");
            var c3 = contentService.CreateContentWithIdentity("content", c1, "umbBlah");
            var c4 = contentService.CreateContentWithIdentity("content", c3, "umbBlah");

            foreach (var l in ServiceContext.LocalizationService.GetAllLanguages().Where(x => x.CultureName != "en-US").ToArray())
                ServiceContext.LocalizationService.Delete(l);

            var l0 = ServiceContext.LocalizationService.GetLanguageByIsoCode("en-US");
            var l1 = new Core.Models.Language("fr-FR");
            var l2 = new Core.Models.Language("de-DE");
            ServiceContext.LocalizationService.Save(l1);
            ServiceContext.LocalizationService.Save(l2);

            foreach (var d in ServiceContext.DomainService.GetAll(true).ToArray())
                ServiceContext.DomainService.Delete(d);

            ServiceContext.DomainService.Save(new UmbracoDomain("*0000") { DomainName = "*0000", RootContentId = c1.Id, LanguageId = l2.Id });
            ServiceContext.DomainService.Save(new UmbracoDomain("*0001") { DomainName = "*0001", RootContentId = c3.Id, LanguageId = l1.Id });
            //ServiceContext.DomainService.Save(new UmbracoDomain("*100112") { DomainName = "*100112", RootContentId = c3.Id, LanguageId = l2.Id });

            var content = c2;
            var culture = Umbraco.Web.Models.ContentExtensions.GetCulture(null,
                ServiceContext.DomainService, ServiceContext.LocalizationService, ServiceContext.ContentService,
                content.Id, content.Path, new Uri("http://domain1.com/"));
            Assert.AreEqual("de-DE", culture.Name);

            content = c4;
            culture = Umbraco.Web.Models.ContentExtensions.GetCulture(null,
                ServiceContext.DomainService, ServiceContext.LocalizationService, ServiceContext.ContentService,
                content.Id, content.Path, new Uri("http://domain1.fr/"));
            Assert.AreEqual("fr-FR", culture.Name);
        }
    }
}