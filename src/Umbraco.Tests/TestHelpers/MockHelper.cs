using Moq;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

namespace Umbraco.Tests.TestHelpers
{
    public static class MockHelper
    {
        public static ServiceContext GetMockedServiceContext(IUmbracoSettingsSection settings = null, ILogger logger = null, CacheHelper cacheHelper = null, ISqlSyntaxProvider sqlSyntax = null)
        {
            if (settings == null) settings = SettingsForTests.GetDefault();
            if (logger == null) logger = Mock.Of<ILogger>();
            if (cacheHelper == null) cacheHelper = CacheHelper.CreateDisabledCacheHelper();
            if (sqlSyntax == null) sqlSyntax = Mock.Of<ISqlSyntaxProvider>();

            return new ServiceContext(
                new Mock<IContentService>().Object,
                new Mock<IMediaService>().Object,
                new Mock<IContentTypeService>().Object,
                new Mock<IDataTypeService>().Object,
                new Mock<IFileService>().Object,
                new Mock<ILocalizationService>().Object,
                new PackagingService(
                    new Mock<ILogger>().Object,
                    new Mock<IContentService>().Object,
                    new Mock<IContentTypeService>().Object,
                    new Mock<IMediaService>().Object,
                    new Mock<IMacroService>().Object,
                    new Mock<IDataTypeService>().Object,
                    new Mock<IFileService>().Object,
                    new Mock<ILocalizationService>().Object,
                    new Mock<IUserService>().Object,
                    new RepositoryFactory(cacheHelper, logger, sqlSyntax, settings),
                    new Mock<IDatabaseUnitOfWorkProvider>().Object),
                new Mock<IEntityService>().Object,
                new RelationService(
                    new Mock<IDatabaseUnitOfWorkProvider>().Object,
                    new RepositoryFactory(cacheHelper, logger, sqlSyntax, settings),
                    logger,
                    new Mock<IEntityService>().Object),
                new Mock<IMemberGroupService>().Object,
                new Mock<IMemberTypeService>().Object,
                new Mock<IMemberService>().Object,
                new Mock<IUserService>().Object,
                new Mock<ISectionService>().Object,
                new Mock<IApplicationTreeService>().Object,
                new Mock<ITagService>().Object,
                new Mock<INotificationService>().Object,
                new Mock<ILocalizedTextService>().Object,
                new Mock<IAuditService>().Object,
                new Mock<IDomainService>().Object);
        }
    }
}