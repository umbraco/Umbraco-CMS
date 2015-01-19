using System;
using System.Linq;
using Umbraco.Core.Auditing;
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

        private void ContentService_Trashed(IContentService sender, MoveEventArgs<IContent> e)
        {
            var relationService = ApplicationContext.Current.Services.RelationService;
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

                // Add a relation for the item being deleted, so that we can know the original parent for if we need to restore later
                var relation = new Relation(originalParentId, item.Entity.Id, relationType);
                relationService.Save(relation);

                ApplicationContext.Current.Services.AuditService.Add(AuditType.Delete,
                    string.Format("Trashed content with Id: '{0}' related to original parent content with Id: '{1}'", item.Entity.Id, originalParentId),
                    item.Entity.WriterId,
                    item.Entity.Id);
            }
        }
    }
}