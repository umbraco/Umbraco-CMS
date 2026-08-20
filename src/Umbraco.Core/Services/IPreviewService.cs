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
    /// <remarks>
    ///     The preview mode is persistent across sessions (requests) until terminated with <see cref="EndPreviewAsync"/>.
    /// </remarks>
    /// <param name="user">The user entering preview mode.</param>
    /// <returns><c>true</c> if preview mode was entered successfully; otherwise, <c>false</c>.</returns>
    Task<bool> TryEnterPreviewAsync(IUser user);

    /// <summary>
    ///     Terminates preview mode for the current user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EndPreviewAsync();
}
