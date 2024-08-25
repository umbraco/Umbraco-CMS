using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Locking;

namespace Umbraco.Cms.Core.Services;

internal sealed class ContentTypeContainerService : EntityTypeContainerService<IContentType, IDocumentTypeContainerRepository>, IContentTypeContainerService
{
    public ContentTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentTypeContainerRepository entityContainerRepository,
        IAuditRepository auditRepository,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditRepository, entityRepository, userIdKeyResolver)
    {
    }

    protected override Guid ContainedObjectType => Constants.ObjectTypes.DocumentType;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DocumentTypeContainer;

    protected override int[] ReadLockIds => ContentTypeLocks.ReadLockIds;

    protected override int[] WriteLockIds => ContentTypeLocks.WriteLockIds;
}
