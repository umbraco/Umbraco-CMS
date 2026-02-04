using System.Security.Claims;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing content preview mode.
/// </summary>
/// <remarks>
///     Preview mode allows backoffice users to view unpublished content changes
///     as they would appear on the front-end website.
/// </remarks>
public interface IPreviewService
{
    /// <summary>
    ///     Enters preview mode for a given user.
    /// </summary>
    /// <param name="user">The user entering preview mode.</param>
    /// <returns><c>true</c> if preview mode was entered successfully; otherwise, <c>false</c>.</returns>
    Task<bool> TryEnterPreviewAsync(IUser user);

    /// <summary>
    ///     Exits preview mode for the current request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EndPreviewAsync();

    /// <summary>
    ///     Determines whether the current request is in preview mode.
    /// </summary>
    /// <returns><c>true</c> if the current request is in preview mode; otherwise, <c>false</c>.</returns>
    bool IsInPreview();

    /// <summary>
    ///     Attempts to get the claims identity for the current preview session.
    /// </summary>
    /// <returns>An attempt containing the claims identity if in preview mode; otherwise, a failed attempt.</returns>
    Task<Attempt<ClaimsIdentity>> TryGetPreviewClaimsIdentityAsync();
}
