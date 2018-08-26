using System;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Strategies
{
    public sealed class RelateOnTrashHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Moved += ContentService_Moved;
            ContentService.Trashed += ContentService_Trashed;

            MediaService.Moved += MediaService_Moved;
            MediaService.Trashed += MediaService_Trashed;
        }

        private void ContentService_Moved(IContentService sender, MoveEventArgs<IContent> e)
        {
            foreach (var item in e.MoveInfoCollection.Where(x => x.OriginalPath.Contains(Constants.System.RecycleBinContent.ToInvariantString())))
            {
                var relationService = ApplicationContext.Current.Services.RelationService;
                var relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;
                var relations = relationService.GetByChildId(item.Entity.Id);

                foreach (var relation in relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)))
                {
                    relationService.Delete(relation);
                }
            }
        }

        private void MediaService_Moved(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            foreach (var item in e.MoveInfoCollection.Where(x => x.OriginalPath.Contains(Constants.System.RecycleBinMedia.ToInvariantString())))
            {
                var relationService = ApplicationContext.Current.Services.RelationService;
                var relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
                var relations = relationService.GetByChildId(item.Entity.Id);

                foreach (var relation in relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)))
                {
                    relationService.Delete(relation);
                }
            }
        }

        private void ContentService_Trashed(IContentService sender, MoveEventArgs<IContent> e)
        {
            var relationService = ApplicationContext.Current.Services.RelationService;
            var entityService = ApplicationContext.Current.Services.EntityService;
            var textService = ApplicationContext.Current.Services.TextService;
            var relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;
            var relationType = relationService.GetRelationTypeByAlias(relationTypeAlias);

            // check that the relation-type exists, if not, then recreate it
            if (relationType == null)
            {
                var documentObjectType = new Guid(Constants.ObjectTypes.Document);
                var relationTypeName = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName;

                relationType = new RelationType(documentObjectType, documentObjectType, relationTypeAlias, relationTypeName);
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

                    ApplicationContext.Current.Services.AuditService.Add(AuditType.Delete,
                        string.Format(textService.Localize(
                                "recycleBin/contentTrashed"),
                            item.Entity.Id, originalParentId),
                        item.Entity.WriterId,
                        item.Entity.Id);
                }

                
            }
        }

        private void MediaService_Trashed(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            var relationService = ApplicationContext.Current.Services.RelationService;
            var entityService = ApplicationContext.Current.Services.EntityService;
            var textService = ApplicationContext.Current.Services.TextService;
            var relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
            var relationType = relationService.GetRelationTypeByAlias(relationTypeAlias);

            // check that the relation-type exists, if not, then recreate it
            if (relationType == null)
            {
                var documentObjectType = new Guid(Constants.ObjectTypes.Document);
                var relationTypeName = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName;

                relationType = new RelationType(documentObjectType, documentObjectType, relationTypeAlias, relationTypeName);
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

                    ApplicationContext.Current.Services.AuditService.Add(AuditType.Delete,
                        string.Format(textService.Localize(
                                "recycleBin/mediaTrashed"),
                            item.Entity.Id, originalParentId),
                        item.Entity.CreatorId,
                        item.Entity.Id);
                }
            }
        }
    }
}