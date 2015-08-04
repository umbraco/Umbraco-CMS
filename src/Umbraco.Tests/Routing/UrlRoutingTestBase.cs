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
                    new UmbracoDomain("domain1.com/"){Id = 1, LanguageId = 333, RootContentId = 1001}, 
                    new UmbracoDomain("domain1.com/en"){Id = 1, LanguageId = 334, RootContentId = 10011}, 
                    new UmbracoDomain("domain1.com/fr"){Id = 1, LanguageId = 335, RootContentId = 10012}
                });

            var langService = Mock.Get(svcCtx.LocalizationService);
            //setup mock domain service
            langService.Setup(service => service.GetLanguageById(LangDeId)).Returns(new Language("de-DE") {Id = LangDeId });
            langService.Setup(service => service.GetLanguageById(LangEngId)).Returns(new Language("en-US") { Id = LangEngId });
            langService.Setup(service => service.GetLanguageById(LangFrId)).Returns(new Language("fr-FR") { Id = LangFrId });
            langService.Setup(service => service.GetLanguageById(LangCzId)).Returns(new Language("cs-CZ") { Id = LangCzId });
            langService.Setup(service => service.GetLanguageById(LangNlId)).Returns(new Language("nl-NL") { Id = LangNlId });
            langService.Setup(service => service.GetLanguageById(LangDkId)).Returns(new Language("da-DK") { Id = LangDkId });

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