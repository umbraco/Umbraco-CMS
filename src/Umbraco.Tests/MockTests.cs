using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Moq;
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
            var svcCtx = new ServiceContext(
                new Mock<IContentService>().Object,
                new Mock<IMediaService>().Object,
                new Mock<IContentTypeService>().Object,
                new Mock<IDataTypeService>().Object,
                new Mock<IFileService>().Object,
                new Mock<ILocalizationService>().Object,
                new PackagingService(
                    new Mock<IContentService>().Object,
                    new Mock<IContentTypeService>().Object,
                    new Mock<IMediaService>().Object,
                    new Mock<IDataTypeService>().Object,
                    new Mock<IFileService>().Object,
                    new Mock<ILocalizationService>().Object,
                    new RepositoryFactory(true),
                    new Mock<IDatabaseUnitOfWorkProvider>().Object),
                new Mock<IEntityService>().Object,
                new RelationService(
                    new Mock<IDatabaseUnitOfWorkProvider>().Object,
                    new RepositoryFactory(true),
                    new Mock<IEntityService>().Object),
                new Mock<ISectionService>().Object,
                new Mock<IApplicationTreeService>().Object);
            Assert.Pass();
        }

        [Test]
        public void Can_Create_Db_Context()
        {
            var dbCtx = new DatabaseContext(new Mock<IDatabaseFactory>().Object);
            Assert.Pass();
        }

        [Test]
        public void Can_Create_App_Context_With_Services()
        {
            var appCtx = new ApplicationContext(
                new DatabaseContext(new Mock<IDatabaseFactory>().Object),
                new ServiceContext(
                    new Mock<IContentService>().Object,
                    new Mock<IMediaService>().Object,
                    new Mock<IContentTypeService>().Object,
                    new Mock<IDataTypeService>().Object,
                    new Mock<IFileService>().Object,
                    new Mock<ILocalizationService>().Object,
                    new PackagingService(
                        new Mock<IContentService>().Object,
                        new Mock<IContentTypeService>().Object,
                        new Mock<IMediaService>().Object,
                        new Mock<IDataTypeService>().Object,
                        new Mock<IFileService>().Object,
                        new Mock<ILocalizationService>().Object,
                        new RepositoryFactory(true),
                        new Mock<IDatabaseUnitOfWorkProvider>().Object),
                    new Mock<IEntityService>().Object,
                    new RelationService(
                        new Mock<IDatabaseUnitOfWorkProvider>().Object,
                        new RepositoryFactory(true),
                        new Mock<IEntityService>().Object),
                    new Mock<ISectionService>().Object,
                    new Mock<IApplicationTreeService>().Object),
                CacheHelper.CreateDisabledCacheHelper());
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

        [NUnit.Framework.Ignore("Need to fix more stuff up, this is ignore because an exception occurs because it wants to ensure we have a resolver initialized - need to make that process better for testability")]
        [Test]
        public void Can_Get_Umbraco_Context()
        {
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
            ApplicationContext.EnsureContext(appCtx, true);
            
            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                true);
            
            Assert.AreEqual(umbCtx, UmbracoContext.Current);
        }

    }
}
