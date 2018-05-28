using System;
using Umbraco.Core.Auditing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Strategies
{
    //TODO: This should just exist in the content service/repo! 
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
                string[] tokens = new string[] { e.Copy.Id.ToString(), e.Original.Id.ToString() };

                ApplicationContext.Current.Services.AuditService.Add(
                    AuditType.Copy,
                    ApplicationContext.Current.Services.TextService.Localize("auditTrails/copyWithId", tokens), e.Copy.WriterId, e.Copy.Id);
            }
        }
    }
}
