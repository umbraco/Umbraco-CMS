using System;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Current = Umbraco.Web.Current;

namespace Umbraco.Tests.Testing
{
    [TestFixture]
    public class MockTests : UmbracoTestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
        }

        [Test]
        public void Can_Mock_Service_Context()
        {
            // ReSharper disable once UnusedVariable
            var serviceContext = TestObjects.GetServiceContextMock();
            Assert.Pass();
        }

        [Test]
        public void Can_Mock_Database_Context()
        {
            var databaseFactory = TestObjects.GetDatabaseFactoryMock();
            var logger = Mock.Of<ILogger>();
            var runtimeState = Mock.Of<IRuntimeState>();
            var migrationEntryService = Mock.Of<IMigrationEntryService>();

            // ReSharper disable once UnusedVariable
            var databaseContext = new DatabaseContext(databaseFactory, logger, runtimeState, migrationEntryService);
            Assert.Pass();
        }

        [Test]
        public void Can_Mock_Umbraco_Context()
        {
            var umbracoContext = TestObjects.GetUmbracoContextMock(Current.UmbracoContextAccessor);
            Assert.AreEqual(umbracoContext, UmbracoContext.Current);
        }

        [Test]
        public void Can_Mock_Umbraco_Helper()
        {
            var umbracoContext = TestObjects.GetUmbracoContextMock();

            // unless we can inject them in MembershipHelper, we need need this
            Container.Register(_ => Mock.Of<IMemberService>());
            Container.Register(_ => Mock.Of<IMemberTypeService>());
            Container.Register(_ => CacheHelper.CreateDisabledCacheHelper());
            Container.Register<ServiceContext>();

            // ReSharper disable once UnusedVariable
            var helper = new UmbracoHelper(umbracoContext,
                Mock.Of<IPublishedContent>(),
                Mock.Of<IPublishedContentQuery>(),
                Mock.Of<ITagQuery>(),
                Mock.Of<IDataTypeService>(),
                Mock.Of<ICultureDictionary>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                new MembershipHelper(umbracoContext, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>()),
                new ServiceContext(),
                CacheHelper.CreateDisabledCacheHelper());
            Assert.Pass();
        }

        [Test]
        public void Can_Mock_UrlProvider()
        {
            var umbracoContext = TestObjects.GetUmbracoContextMock();

            var urlProviderMock = new Mock<IUrlProvider>();
            urlProviderMock.Setup(provider => provider.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<int>(), It.IsAny<Uri>(), It.IsAny<UrlProviderMode>()))
                .Returns("/hello/world/1234");
            var urlProvider = urlProviderMock.Object;

            var theUrlProvider = new UrlProvider(umbracoContext, new [] { urlProvider });

            Assert.AreEqual("/hello/world/1234", theUrlProvider.GetUrl(1234));
        }
    }
}
