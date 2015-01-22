using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Moq;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests
{
    [TestFixture]
    public class MockTests
    {

        [Test]
        public void Can_Create_Empty_App_Context()
        {
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
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
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
            var result = ApplicationContext.EnsureContext(appCtx, true);
            Assert.AreEqual(appCtx, result);
        }

        [Test]
        public void Does_Not_Overwrite_App_Context_Singleton()
        {
            ApplicationContext.EnsureContext(new ApplicationContext(CacheHelper.CreateDisabledCacheHelper()), true);
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
            var result = ApplicationContext.EnsureContext(appCtx, false);
            Assert.AreNotEqual(appCtx, result);
        }

        [Test]
        public void Can_Get_Umbraco_Context()
        {
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
            ApplicationContext.EnsureContext(appCtx, true);
            
            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                Mock.Of<IUmbracoSettingsSection>(),
                true);
            
            Assert.AreEqual(umbCtx, UmbracoContext.Current);
        }

    }
}
