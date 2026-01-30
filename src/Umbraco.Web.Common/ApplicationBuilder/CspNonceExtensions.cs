using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
/// Extension methods for integrating CSP nonces with third-party CSP middleware.
/// </summary>
public static class CspNonceExtensions
{
    /// <summary>
    /// Adds middleware that injects Umbraco's CSP nonce into an existing Content-Security-Policy header.
    /// Use this AFTER your CSP middleware (e.g., NWebsec) to add the nonce to the script-src directive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This middleware modifies the CSP header set by other middleware (like NWebsec) to include
    /// Umbraco's nonce value. Place this middleware AFTER your CSP middleware in the pipeline.
    /// </para>
    /// <para>
    /// Example usage with NWebsec:
    /// <code>
    /// app.UseCsp(options => options
    ///     .DefaultSources(s => s.Self())
    ///     .ScriptSources(s => s.Self())
    ///     .StyleSources(s => s.Self().UnsafeInline()));
    ///
    /// app.UseUmbracoCspNonceInjection(); // Add nonce to NWebsec's CSP header
    ///
    /// app.UseUmbraco()...
    /// </code>
    /// </para>
    /// </remarks>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseUmbracoCspNonceInjection(this IApplicationBuilder app)
        => app.UseUmbracoCspNonceInjection(_ => { });

    /// <summary>
    /// Adds middleware that injects Umbraco's CSP nonce into an existing Content-Security-Policy header.
    /// Use this AFTER your CSP middleware (e.g., NWebsec) to add the nonce to the script-src directive.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configure">Action to configure the nonce injection options.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseUmbracoCspNonceInjection(
        this IApplicationBuilder app,
        Action<CspNonceInjectionOptions> configure)
    {
        var options = new CspNonceInjectionOptions();
        configure(options);

        return app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                InjectNonceIntoHeader(context, options);
                return Task.CompletedTask;
            });

            await next();
        });
    }

    private static void InjectNonceIntoHeader(HttpContext context, CspNonceInjectionOptions options)
    {
        // Skip if the request path doesn't match
        if (!options.ShouldApplyToRequest(context))
        {
            return;
        }

        ICspNonceService? cspNonceService = context.RequestServices.GetService<ICspNonceService>();
        if (cspNonceService is null)
        {
            return;
        }

        // Get the nonce that was already generated for this request.
        var nonce = cspNonceService.GetNonce();
        if (string.IsNullOrEmpty(nonce))
        {
            return;
        }

        foreach (var headerName in options.HeaderNames)
        {
            if (context.Response.Headers.TryGetValue(headerName, out StringValues headerValue))
            {
                var csp = headerValue.ToString();
                var modifiedCsp = InjectNonceIntoDirective(csp, options.Directive, nonce);
                context.Response.Headers[headerName] = modifiedCsp;
            }
        }
    }

    private static string InjectNonceIntoDirective(string csp, string directive, string nonce)
    {
        var nonceValue = $"'nonce-{nonce}'";
        var directivePrefix = $"{directive} ";

        // Check if directive exists in the CSP
        var directiveIndex = csp.IndexOf(directivePrefix, StringComparison.OrdinalIgnoreCase);
        if (directiveIndex == -1)
        {
            // Directive not found - append it with the nonce
            return csp.TrimEnd(';', ' ') + $"; {directive} {nonceValue}";
        }

        // Insert nonce after the directive name
        var insertPosition = directiveIndex + directivePrefix.Length;
        return csp.Insert(insertPosition, nonceValue + " ");
    }
}

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
