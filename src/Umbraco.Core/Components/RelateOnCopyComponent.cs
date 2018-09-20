﻿using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Components
{
    //TODO: This should just exist in the content service/repo!
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class RelateOnCopyComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public void Initialize()
        {
            ContentService.Copied += ContentServiceCopied;
        }

        private static void ContentServiceCopied(IContentService sender, Events.CopyEventArgs<IContent> e)
        {
            if (e.RelateToOriginal == false) return;

            var relationService = Current.Services.RelationService;

            var relationType = relationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias);

            if (relationType == null)
            {
                relationType = new RelationType(Constants.ObjectTypes.Document,
                    Constants.ObjectTypes.Document,
                    Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias,
                    Constants.Conventions.RelationTypes.RelateDocumentOnCopyName) { IsBidirectional = true };

                relationService.Save(relationType);
            }

            var relation = new Relation(e.Original.Id, e.Copy.Id, relationType);
            relationService.Save(relation);

            Current.Services.AuditService.Add(
                AuditType.Copy,
                $"Copied content with Id: '{e.Copy.Id}' related to original content with Id: '{e.Original.Id}'",
                e.Copy.WriterId, e.Copy.Id);
        }
    }
}
