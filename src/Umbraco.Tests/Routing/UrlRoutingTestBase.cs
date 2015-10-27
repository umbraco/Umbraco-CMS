using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Routing
{
    [DatabaseTestBehavior(DatabaseBehavior.NoDatabasePerFixture)]
    [TestFixture]
    public abstract class UrlRoutingTestBase : BaseRoutingTest
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

        protected ServiceContext GetServiceContext(IUmbracoSettingsSection umbracoSettings, ILogger logger)
        {
            //get the mocked service context to get the mocked domain service
            var svcCtx = MockHelper.GetMockedServiceContext();

            var domainService = Mock.Get(svcCtx.DomainService);
            //setup mock domain service
            domainService.Setup(service => service.GetAll(It.IsAny<bool>()))
                .Returns((bool incWildcards) => new[]
                {
                    new UmbracoDomain("domain1.com/"){Id = 1, LanguageId = LangDeId, RootContentId = 1001, LanguageIsoCode = "de-DE"}, 
                    new UmbracoDomain("domain1.com/en"){Id = 1, LanguageId = LangEngId, RootContentId = 10011, LanguageIsoCode = "en-US"}, 
                    new UmbracoDomain("domain1.com/fr"){Id = 1, LanguageId = LangFrId, RootContentId = 10012, LanguageIsoCode = "fr-FR"}
                });            

            return svcCtx;
        }

        public const int LangDeId = 333;
        public const int LangEngId = 334;
        public const int LangFrId = 335;
        public const int LangCzId = 336;
        public const int LangNlId = 337;
        public const int LangDkId = 338;

        protected override void SetupApplicationContext()
        {
            var settings = SettingsForTests.GetDefault();
            ApplicationContext.Current = new ApplicationContext(
                new DatabaseContext(Mock.Of<IDatabaseFactory>(), Logger, Mock.Of<ISqlSyntaxProvider>(), "test"),
                GetServiceContext(settings, Logger),
                CacheHelper,
                ProfilingLogger)
            {
                IsReady = true
            };
        }
    }
}