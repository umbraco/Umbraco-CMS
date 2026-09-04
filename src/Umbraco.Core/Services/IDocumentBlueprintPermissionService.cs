using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for document blueprint access.
/// </summary>
/// <remarks>
///     Access is granted as a whole: a user who can reach a document blueprint may edit, move and delete it.
///     There are no per-action verbs to check, so no method takes any.
/// </remarks>
public interface IDocumentBlueprintPermissionService
{
    /// <summary>
    ///     Authorize that a user has access to a document blueprint.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="documentBlueprintKey">The identifier of the document blueprint to check for access.</param>
    /// <returns>A task resolving into a <see cref="DocumentBlueprintAuthorizationStatus"/>.</returns>
    Task<DocumentBlueprintAuthorizationStatus> AuthorizeAccessAsync(IUser user, Guid documentBlueprintKey)
        => AuthorizeAccessAsync(user, documentBlueprintKey.Yield());

    /// <summary>
    ///     Authorize that a user has access to document blueprints.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="documentBlueprintKeys">
    ///     The identifiers to check for access. Each may identify either a document blueprint or one of the
    ///     containers holding them.
    /// </param>
    /// <returns>A task resolving into a <see cref="DocumentBlueprintAuthorizationStatus"/>.</returns>
    Task<DocumentBlueprintAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> documentBlueprintKeys);

    /// <summary>
    ///     Authorize that a user has access to the document blueprint root.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <returns>A task resolving into a <see cref="DocumentBlueprintAuthorizationStatus"/>.</returns>
    Task<DocumentBlueprintAuthorizationStatus> AuthorizeRootAccessAsync(IUser user);
}
