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
        protected void SetupDomainServiceMock(IEnumerable<IDomain> allDomains)
        {
            var domainService = Mock.Get(ServiceContext.DomainService);
            //setup mock domain service
            domainService.Setup(service => service.GetAll(It.IsAny<bool>()))
                .Returns((bool incWildcards) => incWildcards ? allDomains : allDomains.Where(d => d.IsWildcard == false));
            domainService.Setup(service => service.GetAssignedDomains(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns((int id, bool incWildcards) => allDomains.Where(d => d.RootContent.Id == id && (incWildcards || d.IsWildcard == false)));
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
                    new UmbracoDomain("domain1.com/"){Id = 1, Language = new Language("de-DE"), RootContent = new Content("test1", -1, new ContentType(-1)){ Id = 1001}}, 
                    new UmbracoDomain("domain1.com/en"){Id = 1, Language = new Language("en-US"), RootContent = new Content("test2", -1, new ContentType(-1)){ Id = 10011}}, 
                    new UmbracoDomain("domain1.com/fr"){Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test3", -1, new ContentType(-1)){ Id = 10012}}
                });
            return svcCtx;
        }

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