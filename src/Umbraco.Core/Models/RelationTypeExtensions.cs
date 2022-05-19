// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Extensions;

public static class RelationTypeExtensions
{
    public static bool IsSystemRelationType(this IRelationType relationType) =>
        relationType.Alias == Constants.Conventions.RelationTypes.RelatedDocumentAlias
        || relationType.Alias == Constants.Conventions.RelationTypes.RelatedMediaAlias
        || relationType.Alias == Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias
        || relationType.Alias == Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias
        || relationType.Alias == Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
}
