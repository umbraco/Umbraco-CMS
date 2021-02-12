using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Cms.Core.Compose
{
    // TODO: This should just exist in the content service/repo!
    public sealed class RelateOnCopyComponent : IComponent
    {
        private readonly IRelationService _relationService;
        private readonly IAuditService _auditService;

        public RelateOnCopyComponent(IRelationService relationService, IAuditService auditService)
        {
            _relationService = relationService;
            _auditService = auditService;
        }

        public void Initialize()
        {
            ContentService.Copied += ContentServiceCopied;
        }

        public void Terminate()
        {
            ContentService.Copied -= ContentServiceCopied;
        }

        private void ContentServiceCopied(IContentService sender, CopyEventArgs<IContent> e)
        {
            if (e.RelateToOriginal == false) return;


            var relationType = _relationService.GetRelationTypeByAlias(Cms.Core.Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias);

            if (relationType == null)
            {
                relationType = new RelationType(Cms.Core.Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias,
                    Cms.Core.Constants.Conventions.RelationTypes.RelateDocumentOnCopyName,
                    true,
                    Cms.Core.Constants.ObjectTypes.Document,
                    Cms.Core.Constants.ObjectTypes.Document);

                _relationService.Save(relationType);
            }

            var relation = new Relation(e.Original.Id, e.Copy.Id, relationType);
            _relationService.Save(relation);

            _auditService.Add(
                AuditType.Copy,
                e.Copy.WriterId,
                e.Copy.Id, ObjectTypes.GetName(UmbracoObjectTypes.Document),
                $"Copied content with Id: '{e.Copy.Id}' related to original content with Id: '{e.Original.Id}'");
        }
    }
}
