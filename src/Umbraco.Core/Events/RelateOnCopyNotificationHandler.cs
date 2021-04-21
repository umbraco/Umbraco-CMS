// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Notifications;

namespace Umbraco.Cms.Core.Events
{
    public class RelateOnCopyNotificationHandler : INotificationHandler<ContentCopiedNotification>
    {
        private readonly IRelationService _relationService;
        private readonly IAuditService _auditService;

        public RelateOnCopyNotificationHandler(IRelationService relationService, IAuditService auditService)
        {
            _relationService = relationService;
            _auditService = auditService;
        }

        public void Handle(ContentCopiedNotification notification)
        {
            if (notification.RelateToOriginal == false)
            {
                return;
            }

            var relationType = _relationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias);

            if (relationType == null)
            {
                relationType = new RelationType(Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias,
                    Constants.Conventions.RelationTypes.RelateDocumentOnCopyName,
                    true,
                    Constants.ObjectTypes.Document,
                    Constants.ObjectTypes.Document);

                _relationService.Save(relationType);
            }

            var relation = new Relation(notification.Original.Id, notification.Copy.Id, relationType);
            _relationService.Save(relation);

            _auditService.Add(
                AuditType.Copy,
                notification.Copy.WriterId,
                notification.Copy.Id, ObjectTypes.GetName(UmbracoObjectTypes.Document),
                $"Copied content with Id: '{notification.Copy.Id}' related to original content with Id: '{notification.Original.Id}'");
        }
    }
}
