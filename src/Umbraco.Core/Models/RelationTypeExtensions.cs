// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IRelationType"/>.
/// </summary>
public static class RelationTypeExtensions
{
    /// <summary>
    /// Determines whether the relation type is a built-in system relation type.
    /// </summary>
    /// <param name="relationType">The relation type to check.</param>
    /// <returns><c>true</c> if the relation type is a system relation type; otherwise, <c>false</c>.</returns>
    public static bool IsSystemRelationType(this IRelationType relationType) =>
        relationType.Alias == Constants.Conventions.RelationTypes.RelatedDocumentAlias
        || relationType.Alias == Constants.Conventions.RelationTypes.RelatedMediaAlias
        || relationType.Alias == Constants.Conventions.RelationTypes.RelatedMemberAlias
        || relationType.Alias == Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias
        || relationType.Alias == Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias
        || relationType.Alias == Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;

    /// <summary>
    /// Determines whether the relation type can be deleted.
    /// </summary>
    /// <param name="relationType">The relation type to check.</param>
    /// <returns><c>true</c> if the relation type can be deleted; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// System relation types cannot be deleted.
    /// </remarks>
    public static bool IsDeletableRelationType(this IRelationType relationType)
        => relationType.IsSystemRelationType() is false;
}
