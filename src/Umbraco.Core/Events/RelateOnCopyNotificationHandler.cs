// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Events;

public class RelateOnCopyNotificationHandler :
    INotificationHandler<ContentCopiedNotification>,
    INotificationAsyncHandler<ContentCopiedNotification>
{
    private readonly IAuditService _auditService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IRelationService _relationService;

    public RelateOnCopyNotificationHandler(
        IRelationService relationService,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver)
    {
        _relationService = relationService;
        _auditService = auditService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in V19.")]
    public RelateOnCopyNotificationHandler(
        IRelationService relationService,
        IAuditService auditService)
        : this(
            relationService,
            auditService,
            StaticServiceProvider.Instance.GetRequiredService<IUserIdKeyResolver>())
    {
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

        Guid writerKey = await _userIdKeyResolver.GetAsync(notification.Copy.WriterId);
        await _auditService.AddAsync(
            AuditType.Copy,
            writerKey,
            notification.Copy.Id,
            UmbracoObjectTypes.Document.GetName() ?? string.Empty,
            $"Copied content with Id: '{notification.Copy.Id}' related to original content with Id: '{notification.Original.Id}'");
    }

    [Obsolete("Use the INotificationAsyncHandler.HandleAsync implementation instead. Scheduled for removal in V19.")]
    public void Handle(ContentCopiedNotification notification) =>
        HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();
}
