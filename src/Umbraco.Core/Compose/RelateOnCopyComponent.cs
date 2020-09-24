using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Compose
{
    // TODO: This should just exist in the content service/repo!
    public sealed class RelateOnCopyComponent : IComponent
    {
        public void Initialize()
        {
            ContentService.Copied += ContentServiceCopied;
        }

        public void Terminate()
        {
            ContentService.Copied -= ContentServiceCopied;
        }

        private static void ContentServiceCopied(IContentService sender, Events.CopyEventArgs<IContent> e)
        {
            if (e.RelateToOriginal == false) return;

            var relationService = Current.Services.RelationService;

            var relationType = relationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias);

            if (relationType == null)
            {
                relationType = new RelationType(Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias,
                    Constants.Conventions.RelationTypes.RelateDocumentOnCopyName,
                    true,
                    Constants.ObjectTypes.Document,
                    Constants.ObjectTypes.Document);

                relationService.Save(relationType);
            }

            var relation = new Relation(e.Original.Id, e.Copy.Id, relationType);
            relationService.Save(relation);

            Current.Services.AuditService.Add(
                AuditType.Copy,
                e.Copy.WriterId,
                e.Copy.Id, ObjectTypes.GetName(UmbracoObjectTypes.Document),
                $"Copied content with Id: '{e.Copy.Id}' related to original content with Id: '{e.Original.Id}'");
        }
    }
}
