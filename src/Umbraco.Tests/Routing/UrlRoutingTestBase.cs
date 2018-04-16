using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public abstract class UrlRoutingTestBase : BaseWebTest
    {
        /// <summary>
        /// Sets up the mock domain service
        /// </summary>
        /// <param name="allDomains"></param>
        protected IDomainService SetupDomainServiceMock(IEnumerable<IDomain> allDomains)
        {
            var domainService = Mock.Get(ServiceContext.DomainService);
            //setup mock domain service
            domainService.Setup(service => service.GetAll(It.IsAny<bool>()))
                .Returns((bool incWildcards) => incWildcards ? allDomains : allDomains.Where(d => d.IsWildcard == false));
            domainService.Setup(service => service.GetAssignedDomains(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns((int id, bool incWildcards) => allDomains.Where(d => d.RootContentId == id && (incWildcards || d.IsWildcard == false)));
            return domainService.Object;
        }

        protected override void Compose()
        {
            base.Compose();

            Container.RegisterSingleton(_ => GetServiceContext());
        }

        protected ServiceContext GetServiceContext()
        {
            // get the mocked service context to get the mocked domain service
            var serviceContext = TestObjects.GetServiceContextMock(Container);

            //setup mock domain service
            var domainService = Mock.Get(serviceContext.DomainService);
            domainService.Setup(service => service.GetAll(It.IsAny<bool>()))
                .Returns((bool incWildcards) => new[]
                {
                    new UmbracoDomain("domain1.com/"){Id = 1, LanguageId = LangDeId, RootContentId = 1001, LanguageIsoCode = "de-DE"},
                    new UmbracoDomain("domain1.com/en"){Id = 1, LanguageId = LangEngId, RootContentId = 10011, LanguageIsoCode = "en-US"},
                    new UmbracoDomain("domain1.com/fr"){Id = 1, LanguageId = LangFrId, RootContentId = 10012, LanguageIsoCode = "fr-FR"}
                });

            return serviceContext;
        }

        public const int LangDeId = 333;
        public const int LangEngId = 334;
        public const int LangFrId = 335;
        public const int LangCzId = 336;
        public const int LangNlId = 337;
        public const int LangDkId = 338;
    }
}
