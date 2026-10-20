namespace Umbraco.Cms.Core.Media;

/// <summary>
/// Refreshes the HMAC signature on a generated image URL using the secret key
/// that is currently configured on the imaging middleware.
/// </summary>
/// <remarks>
/// Rich text editors persist image URLs (with the HMAC token baked in) into stored markup.
/// When the secret key is rotated, the persisted token no longer validates and the image
/// fails to render. Render-time pipelines call <see cref="RefreshSignature"/> to strip any
/// stale token from the URL and re-sign with the current key.
/// </remarks>
public interface IImageUrlTokenGenerator
{
    /// <summary>
    /// Strips any existing HMAC token from <paramref name="url"/> and returns the URL
    /// signed with the currently configured secret key. If no key is configured, or
    /// the input is empty, the URL is returned unchanged.
    /// </summary>
    /// <param name="url">The image URL, optionally containing a stale HMAC token in its query string.</param>
    /// <returns>The image URL with a freshly computed HMAC token (or unchanged if no key is configured).</returns>
    string RefreshSignature(string url);
}
