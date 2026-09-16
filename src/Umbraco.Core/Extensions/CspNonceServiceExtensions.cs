using System.Net;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="ICspNonceService" />.
/// </summary>
public static class CspNonceServiceExtensions
{
    /// <summary>
    /// Gets a <c>nonce</c> attribute for the current request, ready to be written into a tag.
    /// </summary>
    /// <param name="cspNonceService">The CSP nonce service.</param>
    /// <returns>
    /// The attribute including a leading space (for example <c> nonce="abc123"</c>), or an empty string
    /// when no nonce is available for the current request.
    /// </returns>
    /// <remarks>
    /// The nonce is HTML encoded, as <see cref="ICspNonceService" /> can be replaced and the returned
    /// attribute is written into markup without further encoding.
    /// </remarks>
    public static string GetNonceAttribute(this ICspNonceService cspNonceService)
    {
        var nonce = cspNonceService.GetNonce();

        return string.IsNullOrEmpty(nonce) ? string.Empty : $" nonce=\"{WebUtility.HtmlEncode(nonce)}\"";
    }
}
