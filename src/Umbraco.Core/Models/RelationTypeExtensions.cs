namespace Umbraco.Core.Models
{
    internal static class RelationTypeExtensions
    {
        internal static bool IsSystemRelationType(this IRelationType relationType) =>
            relationType.Alias == Constants.Conventions.RelationTypes.RelatedDocumentAlias
            || relationType.Alias == Constants.Conventions.RelationTypes.RelatedMediaAlias
            || relationType.Alias == Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias
            || relationType.Alias == Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias
            || relationType.Alias == Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
    }
}
