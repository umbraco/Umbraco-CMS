using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Compose
{
    public sealed class RelateOnTrashComponent : IComponent
    {
        public void Initialize()
        {
            ContentService.Moved += ContentService_Moved;
            ContentService.Trashed += ContentService_Trashed;
            MediaService.Moved += MediaService_Moved;
            MediaService.Trashed += MediaService_Trashed;
        }

        public void Terminate()
        {
            ContentService.Moved -= ContentService_Moved;
            ContentService.Trashed -= ContentService_Trashed;
            MediaService.Moved -= MediaService_Moved;
            MediaService.Trashed -= MediaService_Trashed;
        }

        private static void ContentService_Moved(IContentService sender, MoveEventArgs<IContent> e)
        {
            foreach (var item in e.MoveInfoCollection.Where(x => x.OriginalPath.Contains(Constants.System.RecycleBinContentString)))
            {
                var relationService = Current.Services.RelationService;
                const string relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;
                var relations = relationService.GetByChildId(item.Entity.Id);

                foreach (var relation in relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)))
                {
                    relationService.Delete(relation);
                }
            }
        }

        private static void MediaService_Moved(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            foreach (var item in e.MoveInfoCollection.Where(x => x.OriginalPath.Contains(Constants.System.RecycleBinMediaString)))
            {
                var relationService = Current.Services.RelationService;
                const string relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
                var relations = relationService.GetByChildId(item.Entity.Id);
                foreach (var relation in relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)))
                {
                    relationService.Delete(relation);
                }
            }
        }

        private static void ContentService_Trashed(IContentService sender, MoveEventArgs<IContent> e)
        {
            var relationService = Current.Services.RelationService;
            var entityService = Current.Services.EntityService;
            var textService = Current.Services.TextService;
            const string relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;
            var relationType = relationService.GetRelationTypeByAlias(relationTypeAlias);

            // check that the relation-type exists, if not, then recreate it
            if (relationType == null)
            {
                var documentObjectType = Constants.ObjectTypes.Document;
                const string relationTypeName = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName;

                relationType = new RelationType(relationTypeName, relationTypeAlias, false, documentObjectType, documentObjectType);
                relationService.Save(relationType);
            }

            foreach (var item in e.MoveInfoCollection)
            {
                var originalPath = item.OriginalPath.ToDelimitedList();
                var originalParentId = originalPath.Count > 2
                    ? int.Parse(originalPath[originalPath.Count - 2])
                    : Constants.System.Root;

                //before we can create this relation, we need to ensure that the original parent still exists which
                //may not be the case if the encompassing transaction also deleted it when this item was moved to the bin

                if (entityService.Exists(originalParentId))
                {
                    // Add a relation for the item being deleted, so that we can know the original parent for if we need to restore later
                    var relation = new Relation(originalParentId, item.Entity.Id, relationType);
                    relationService.Save(relation);

                    Current.Services.AuditService.Add(AuditType.Delete,
                        item.Entity.WriterId,
                        item.Entity.Id,
                        ObjectTypes.GetName(UmbracoObjectTypes.Document),
                        string.Format(textService.Localize(
                                "recycleBin","contentTrashed"),

                            item.Entity.Id, originalParentId));
                }
            }
        }

        private static void MediaService_Trashed(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            var relationService = Current.Services.RelationService;
            var entityService = Current.Services.EntityService;
            var textService = Current.Services.TextService;
            const string relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
            var relationType = relationService.GetRelationTypeByAlias(relationTypeAlias);
            // check that the relation-type exists, if not, then recreate it
            if (relationType == null)
            {
                var documentObjectType = Constants.ObjectTypes.Document;
                const string relationTypeName = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName;
                relationType = new RelationType(relationTypeName, relationTypeAlias, false, documentObjectType, documentObjectType);
                relationService.Save(relationType);
            }
            foreach (var item in e.MoveInfoCollection)
            {
                var originalPath = item.OriginalPath.ToDelimitedList();
                var originalParentId = originalPath.Count > 2
                    ? int.Parse(originalPath[originalPath.Count - 2])
                    : Constants.System.Root;
                //before we can create this relation, we need to ensure that the original parent still exists which
                //may not be the case if the encompassing transaction also deleted it when this item was moved to the bin
                if (entityService.Exists(originalParentId))
                {
                    // Add a relation for the item being deleted, so that we can know the original parent for if we need to restore later
                    var relation = new Relation(originalParentId, item.Entity.Id, relationType);
                    relationService.Save(relation);
                    Current.Services.AuditService.Add(AuditType.Delete,
                        item.Entity.CreatorId,
                        item.Entity.Id,
                        ObjectTypes.GetName(UmbracoObjectTypes.Media),
                        string.Format(textService.Localize(
                               "recycleBin", "mediaTrashed"),
                            item.Entity.Id, originalParentId));
                }
            }
        }
    }
}
