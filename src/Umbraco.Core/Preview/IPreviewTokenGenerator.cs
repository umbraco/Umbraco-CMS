namespace Umbraco.Cms.Core.Preview;

/// <summary>
/// Generates and verifies preview tokens for content preview functionality.
/// </summary>
/// <remarks>
/// Preview tokens allow users to preview unpublished content by generating
/// secure, time-limited tokens that can be verified on subsequent requests.
/// </remarks>
public interface IPreviewTokenGenerator
{
    /// <summary>
    /// Generates a preview token for the specified user.
    /// </summary>
    /// <param name="userKey">The unique key of the user requesting the preview token.</param>
    /// <returns>An attempt containing the generated token string on success, or a failure result.</returns>
    Task<Attempt<string?>> GenerateTokenAsync(Guid userKey);

    /// <summary>
    /// Verifies a preview token and returns the associated user key.
    /// </summary>
    /// <param name="token">The preview token to verify.</param>
    /// <returns>An attempt containing the user key on success, or a failure result if the token is invalid or expired.</returns>
    Task<Attempt<Guid?>> VerifyAsync(string token);
}
