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
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Tests
{
    [TestFixture]
    public class MockTests
    {

        [Test]
        public void Can_Create_Empty_App_Context()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            Assert.Pass();
        }

        [Test]
        public void Can_Create_Service_Context()
        {
            var svcCtx = MockHelper.GetMockedServiceContext();
            Assert.Pass();
        }

        [Test]
        public void Can_Create_Db_Context()
        {
            var dbCtx = new DatabaseContext(new Mock<IDatabaseFactory>().Object, Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test");
            Assert.Pass();
        }

        [Test]
        public void Can_Create_App_Context_With_Services()
        {
            var appCtx = new ApplicationContext(
                new DatabaseContext(new Mock<IDatabaseFactory>().Object, Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test"),
                MockHelper.GetMockedServiceContext(),
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            
            Assert.Pass();
        }
        
        [Test]
        public void Can_Assign_App_Context_Singleton()
        {
            var appCtx = new ApplicationContext(
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var result = ApplicationContext.EnsureContext(appCtx, true);
            Assert.AreEqual(appCtx, result);
        }

        [Test]
        public void Does_Not_Overwrite_App_Context_Singleton()
        {
            ApplicationContext.EnsureContext(
                new ApplicationContext(
                    CacheHelper.CreateDisabledCacheHelper(), 
                    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())), true);

            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var result = ApplicationContext.EnsureContext(appCtx, false);
            Assert.AreNotEqual(appCtx, result);
        }

        [Test]
        public void Can_Get_Umbraco_Context()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            
            var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);
            
            Assert.AreEqual(umbCtx, UmbracoContext.Current);
        }

        [Test]
        public void Can_Mock_Umbraco_Helper()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var helper = new UmbracoHelper(umbCtx,
                Mock.Of<IPublishedContent>(),
                Mock.Of<ITypedPublishedContentQuery>(),
                Mock.Of<IDynamicPublishedContentQuery>(),
                Mock.Of<ITagQuery>(),
                Mock.Of<IDataTypeService>(),
                new UrlProvider(umbCtx, new[] {Mock.Of<IUrlProvider>()}, UrlProviderMode.Auto), Mock.Of<ICultureDictionary>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                new MembershipHelper(umbCtx, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>()));

            Assert.Pass();
        }

        [Test]
        public void Can_Mock_Umbraco_Helper_Get_Url()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            
            var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var urlHelper = new Mock<IUrlProvider>();
            urlHelper.Setup(provider => provider.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<int>(), It.IsAny<Uri>(), It.IsAny<UrlProviderMode>()))
                .Returns("/hello/world/1234");

            var helper = new UmbracoHelper(umbCtx,
                Mock.Of<IPublishedContent>(),
                Mock.Of<ITypedPublishedContentQuery>(),
                Mock.Of<IDynamicPublishedContentQuery>(),
                Mock.Of<ITagQuery>(),
                Mock.Of<IDataTypeService>(),
                new UrlProvider(umbCtx, new[]
                {
                    urlHelper.Object
                }, UrlProviderMode.Auto), Mock.Of<ICultureDictionary>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                new MembershipHelper(umbCtx, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>()));

            Assert.AreEqual("/hello/world/1234", helper.Url(1234));
        }
    }
}
