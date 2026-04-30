using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
/// Options for CSP nonce injection middleware.
/// </summary>
public class CspNonceInjectionOptions
{
    /// <summary>
    /// Gets or sets the CSP header names to modify.
    /// Defaults to both "Content-Security-Policy" and "Content-Security-Policy-Report-Only".
    /// </summary>
    public string[] HeaderNames { get; set; } =
    [
        "Content-Security-Policy",
        "Content-Security-Policy-Report-Only"
    ];

    /// <summary>
    /// Gets or sets the CSP directive to inject the nonce into.
    /// Defaults to "script-src".
    /// </summary>
    public string Directive { get; set; } = "script-src";

    /// <summary>
    /// Gets or sets a function that determines whether the nonce should be injected for a given request.
    /// Defaults to injecting for all requests.
    /// </summary>
    public Func<HttpContext, bool> ShouldApplyToRequest { get; set; } = _ => true;
}
