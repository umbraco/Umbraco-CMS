// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Events;

public class RelateOnCopyNotificationHandler :
    INotificationHandler<ContentCopiedNotification>,
    INotificationAsyncHandler<ContentCopiedNotification>
{
    private readonly IAuditService _auditService;
    private readonly IRelationService _relationService;

    public RelateOnCopyNotificationHandler(IRelationService relationService, IAuditService auditService)
    {
        _relationService = relationService;
        _auditService = auditService;
    }

    /// <inheritdoc />
    public async Task HandleAsync(ContentCopiedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.RelateToOriginal == false)
        {
            return;
        }

        IRelationType? relationType =
            _relationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias);

        if (relationType == null)
        {
            relationType = new RelationType(
                Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias,
                Constants.Conventions.RelationTypes.RelateDocumentOnCopyName,
                true,
                Constants.ObjectTypes.Document,
                Constants.ObjectTypes.Document,
                false);

            _relationService.Save(relationType);
        }

        var relation = new Relation(notification.Original.Id, notification.Copy.Id, relationType);
        _relationService.Save(relation);

        await _auditService.AddAsync(
            AuditType.Copy,
            notification.Copy.WriterId,
            notification.Copy.Id,
            UmbracoObjectTypes.Document.GetName() ?? string.Empty,
            $"Copied content with Id: '{notification.Copy.Id}' related to original content with Id: '{notification.Original.Id}'");
    }

    [Obsolete("Use the INotificationAsyncHandler.HandleAsync implementation instead. Scheduled for removal in V19.")]
    public void Handle(ContentCopiedNotification notification) =>
        HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();
}
