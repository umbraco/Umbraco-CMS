using System;
using Umbraco.Core.Auditing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Strategies
{
    public sealed class RelateOnCopyHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Copied += ContentServiceCopied;
        }

        private void ContentServiceCopied(IContentService sender, Core.Events.CopyEventArgs<IContent> e)
        {
            if (e.RelateToOriginal)
            {
                var relationService = ApplicationContext.Current.Services.RelationService;

                var relationType = relationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias);

                if (relationType == null)
                {
                    relationType = new RelationType(new Guid(Constants.ObjectTypes.Document),
                        new Guid(Constants.ObjectTypes.Document),
                        Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias,
                        Constants.Conventions.RelationTypes.RelateDocumentOnCopyName) { IsBidirectional = true };

                    relationService.Save(relationType);
                }

                var relation = new Relation(e.Original.Id, e.Copy.Id, relationType);
                relationService.Save(relation);

                ApplicationContext.Current.Services.AuditService.Add(
                    AuditType.Copy,
                    string.Format("Copied content with Id: '{0}' related to original content with Id: '{1}'",
                        e.Copy.Id, e.Original.Id), e.Copy.WriterId, e.Copy.Id);
            }
        }
    }
}
