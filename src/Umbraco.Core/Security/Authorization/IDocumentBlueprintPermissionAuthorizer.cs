using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     Authorizes document blueprint access.
/// </summary>
public interface IDocumentBlueprintPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified document blueprint.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="documentBlueprintKey">The key of the document blueprint to check for.</param>
    /// <returns><c>true</c> if the current user is denied access; otherwise, <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, Guid documentBlueprintKey)
        => IsDeniedAsync(currentUser, documentBlueprintKey.Yield());

    /// <summary>
    ///     Authorizes whether the current user has access to the specified document blueprints.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="documentBlueprintKeys">The keys of the document blueprints to check for.</param>
    /// <returns><c>true</c> if the current user is denied access; otherwise, <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> documentBlueprintKeys);

    /// <summary>
    ///     Authorizes whether the current user has access to the document blueprint root.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <returns><c>true</c> if the current user is denied access; otherwise, <c>false</c>.</returns>
    Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser);
}
