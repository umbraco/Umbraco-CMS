using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Moq;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Current = Umbraco.Web.Current;

namespace Umbraco.Tests
{
    [TestFixture]
    public class MockTests
    {
        [SetUp]
        public void Setup()
        {
            Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }

        [Test]
        public void Can_Create_Empty_App_Context()
        {
            //var appCtx = new ApplicationContext(
            //   CacheHelper.CreateDisabledCacheHelper(),
            //   new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            //Assert.Pass();
        }

        [Test]
        public void Can_Create_Service_Context()
        {
            var svcCtx = TestObjects.GetServiceContextMock();
            Assert.Pass();
        }

        [Test]
        public void Can_Create_Db_Context()
        {
            var databaseFactory = TestObjects.GetIDatabaseFactoryMock();
            var logger = Mock.Of<ILogger>();
            var dbCtx = new DatabaseContext(databaseFactory, logger, Mock.Of<IRuntimeState>(), Mock.Of<IMigrationEntryService>());
            Assert.Pass();
        }

        [Test]
        public void Can_Get_Umbraco_Context()
        {
            //var appCtx = new ApplicationContext(
            //   CacheHelper.CreateDisabledCacheHelper(),
            //   new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(),
                Mock.Of<IFacadeService>(),
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            Assert.AreEqual(umbCtx, UmbracoContext.Current);
        }

        [Test]
        public void Can_Mock_Umbraco_Helper()
        {
            //var appCtx = new ApplicationContext(
            //   CacheHelper.CreateDisabledCacheHelper(),
            //   new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var facade = new Mock<IFacade>();
            facade.Setup(x => x.MemberCache).Returns(Mock.Of<IPublishedMemberCache>());
            var facadeService = new Mock<IFacadeService>();
            facadeService.Setup(x => x.CreateFacade(It.IsAny<string>())).Returns(facade.Object);

            var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(),
                facadeService.Object,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var helper = new UmbracoHelper(umbCtx,
                Mock.Of<IPublishedContent>(),
                Mock.Of<IPublishedContentQuery>(),
                Mock.Of<ITagQuery>(),
                Mock.Of<IDataTypeService>(),
                new UrlProvider(umbCtx, new[] {Mock.Of<IUrlProvider>()}, UrlProviderMode.Auto), Mock.Of<ICultureDictionary>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                new MembershipHelper(umbCtx, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>()),
                new ServiceContext(),
                CacheHelper.CreateDisabledCacheHelper());

            Assert.Pass();
        }

        [Test]
        public void Can_Mock_Umbraco_Helper_Get_Url()
        {
            //var appCtx = new ApplicationContext(
            //   CacheHelper.CreateDisabledCacheHelper(),
            //   new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var facade = new Mock<IFacade>();
            facade.Setup(x => x.MemberCache).Returns(Mock.Of<IPublishedMemberCache>());
            var facadeService = new Mock<IFacadeService>();
            facadeService.Setup(x => x.CreateFacade(It.IsAny<string>())).Returns(facade.Object);

            var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(),
                facadeService.Object,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var urlHelper = new Mock<IUrlProvider>();
            urlHelper.Setup(provider => provider.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<int>(), It.IsAny<Uri>(), It.IsAny<UrlProviderMode>()))
                .Returns("/hello/world/1234");

            var helper = new UmbracoHelper(umbCtx,
                Mock.Of<IPublishedContent>(),
                Mock.Of<IPublishedContentQuery>(),
                Mock.Of<ITagQuery>(),
                Mock.Of<IDataTypeService>(),
                new UrlProvider(umbCtx, new[]
                {
                    urlHelper.Object
                }, UrlProviderMode.Auto), Mock.Of<ICultureDictionary>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                new MembershipHelper(umbCtx, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>()),
                new ServiceContext(),
                CacheHelper.CreateDisabledCacheHelper());

            Assert.AreEqual("/hello/world/1234", helper.Url(1234));
        }
    }
}
