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
        public static ServiceContext GetMockedServiceContext()
        {
            return new ServiceContext(
                new Mock<IContentService>().Object,
                new Mock<IMediaService>().Object,
                new Mock<IContentTypeService>().Object,
                new Mock<IDataTypeService>().Object,
                new Mock<IFileService>().Object,
                new Mock<ILocalizationService>().Object,
                new Mock<IPackagingService>().Object,
                new Mock<IEntityService>().Object,
                new Mock<IRelationService>().Object,
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
                new Mock<IDomainService>().Object,
                new Mock<ITaskService>().Object,
                new Mock<IMacroService>().Object);
        }
    }
}