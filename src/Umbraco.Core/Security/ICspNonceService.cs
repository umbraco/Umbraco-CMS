namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Provides CSP nonce generation and retrieval for the current HTTP request.
/// </summary>
public interface ICspNonceService
{
    /// <summary>
    /// Gets the CSP nonce for the current request (generates once, caches per-request).
    /// Returns null if no HTTP context is available.
    /// </summary>
    /// <returns>The CSP nonce value, or null if no HTTP context is available.</returns>
    string? GetNonce();
}
