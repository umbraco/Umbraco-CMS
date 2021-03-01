using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Infrastructure.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Compose
{
    // TODO: insert these notification handlers in core composition
    // TODO: lots of duplicate code in this one, refactor
    public sealed class RelateOnTrashHandler :
        INotificationHandler<MovedNotification<IContent>>,
        INotificationHandler<TrashedNotification<IContent>>
    {
        private readonly IRelationService _relationService;
        private readonly IEntityService _entityService;
        private readonly ILocalizedTextService _textService;
        private readonly IAuditService _auditService;
        private readonly IScopeProvider _scopeProvider;

        public RelateOnTrashHandler(
            IRelationService relationService,
            IEntityService entityService,
            ILocalizedTextService textService,
            IAuditService auditService,
            IScopeProvider scopeProvider)
        {
            _relationService = relationService;
            _entityService = entityService;
            _textService = textService;
            _auditService = auditService;
            _scopeProvider = scopeProvider;
        }

        public void Handle(MovedNotification<IContent> notification)
        {
            foreach (var item in notification.MoveInfoCollection.Where(x => x.OriginalPath.Contains(Cms.Core.Constants.System.RecycleBinContentString)))
            {
                const string relationTypeAlias = Cms.Core.Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;
                var relations = _relationService.GetByChildId(item.Entity.Id);

                foreach (var relation in relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)))
                {
                    _relationService.Delete(relation);
                }
            }
        }

        public void Handle(TrashedNotification<IContent> notification)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                const string relationTypeAlias = Cms.Core.Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;
                var relationType = _relationService.GetRelationTypeByAlias(relationTypeAlias);

                // check that the relation-type exists, if not, then recreate it
                if (relationType == null)
                {
                    var documentObjectType = Cms.Core.Constants.ObjectTypes.Document;
                    const string relationTypeName =
                        Cms.Core.Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName;

                    relationType = new RelationType(relationTypeName, relationTypeAlias, false, documentObjectType,
                        documentObjectType);
                    _relationService.Save(relationType);
                }

                foreach (var item in notification.MoveInfoCollection)
                {
                    var originalPath = item.OriginalPath.ToDelimitedList();
                    var originalParentId = originalPath.Count > 2
                        ? int.Parse(originalPath[originalPath.Count - 2])
                        : Cms.Core.Constants.System.Root;

                    //before we can create this relation, we need to ensure that the original parent still exists which
                    //may not be the case if the encompassing transaction also deleted it when this item was moved to the bin

                    if (_entityService.Exists(originalParentId))
                    {
                        // Add a relation for the item being deleted, so that we can know the original parent for if we need to restore later
                        var relation = _relationService.GetByParentAndChildId(originalParentId, item.Entity.Id, relationType) ??
                                       new Relation(originalParentId, item.Entity.Id, relationType);
                        _relationService.Save(relation);

                        _auditService.Add(AuditType.Delete,
                            item.Entity.WriterId,
                            item.Entity.Id,
                            ObjectTypes.GetName(UmbracoObjectTypes.Document),
                            string.Format(_textService.Localize(
                                    "recycleBin/contentTrashed"),
                                item.Entity.Id, originalParentId));
                    }
                }

                scope.Complete();
            }
        }
    }


    public sealed class RelateOnTrashComponent : IComponent
    {
        private readonly IRelationService _relationService;
        private readonly IEntityService _entityService;
        private readonly ILocalizedTextService _textService;
        private readonly IAuditService _auditService;
        private readonly IScopeProvider _scopeProvider;

        public RelateOnTrashComponent(
            IRelationService relationService,
            IEntityService entityService,
            ILocalizedTextService textService,
            IAuditService auditService,
            IScopeProvider scopeProvider)
        {
            _relationService = relationService;
            _entityService = entityService;
            _textService = textService;
            _auditService = auditService;
            _scopeProvider = scopeProvider;
        }

        public void Initialize()
        {
            MediaService.Moved += MediaService_Moved;
            MediaService.Trashed += MediaService_Trashed;
        }

        public void Terminate()
        {
            MediaService.Moved -= MediaService_Moved;
            MediaService.Trashed -= MediaService_Trashed;
        }

        private void MediaService_Moved(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            foreach (var item in e.MoveInfoCollection.Where(x => x.OriginalPath.Contains(Cms.Core.Constants.System.RecycleBinMediaString)))
            {
                const string relationTypeAlias = Cms.Core.Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
                var relations = _relationService.GetByChildId(item.Entity.Id);
                foreach (var relation in relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)))
                {
                    _relationService.Delete(relation);
                }
            }
        }

        public void MediaService_Trashed(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                const string relationTypeAlias =
                    Cms.Core.Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
                var relationType = _relationService.GetRelationTypeByAlias(relationTypeAlias);
                // check that the relation-type exists, if not, then recreate it
                if (relationType == null)
                {
                    var documentObjectType = Cms.Core.Constants.ObjectTypes.Document;
                    const string relationTypeName =
                        Cms.Core.Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName;
                    relationType = new RelationType(relationTypeName, relationTypeAlias, false, documentObjectType,
                        documentObjectType);
                    _relationService.Save(relationType);
                }

                foreach (var item in e.MoveInfoCollection)
                {
                    var originalPath = item.OriginalPath.ToDelimitedList();
                    var originalParentId = originalPath.Count > 2
                        ? int.Parse(originalPath[originalPath.Count - 2])
                        : Cms.Core.Constants.System.Root;
                    //before we can create this relation, we need to ensure that the original parent still exists which
                    //may not be the case if the encompassing transaction also deleted it when this item was moved to the bin
                    if (_entityService.Exists(originalParentId))
                    {
                        // Add a relation for the item being deleted, so that we can know the original parent for if we need to restore later
                        var relation =
                            _relationService.GetByParentAndChildId(originalParentId, item.Entity.Id, relationType) ??
                            new Relation(originalParentId, item.Entity.Id, relationType);
                        _relationService.Save(relation);
                        _auditService.Add(AuditType.Delete,
                            item.Entity.CreatorId,
                            item.Entity.Id,
                            ObjectTypes.GetName(UmbracoObjectTypes.Media),
                            string.Format(_textService.Localize(
                                    "recycleBin/mediaTrashed"),
                                item.Entity.Id, originalParentId));
                    }
                }

                scope.Complete();
            }
        }
    }
}
