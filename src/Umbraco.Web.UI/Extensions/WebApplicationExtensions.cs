using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.UI.Extensions;

internal static class WebApplicationExtensions
{
    /// <summary>
    /// Adds middleware that sets a Content-Security-Policy header with a nonce for backoffice script execution.
    /// </summary>
    /// <remarks>
    /// This applies the documented CSP rules from https://docs.umbraco.com/umbraco-cms/extending/health-check/guides/contentsecuritypolicy
    /// to the local development site, so any backoffice changes that conflict with these rules are surfaced early.
    /// </remarks>
    public static WebApplication UseDocumentedContentSecurityPolicy(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            ICspNonceService cspNonceService = context.RequestServices.GetRequiredService<ICspNonceService>();
            var nonce = cspNonceService.GetNonce();

            context.Response.Headers.Append("Content-Security-Policy",
                $"default-src 'self'; " +
                $"script-src 'self' 'nonce-{nonce}'; " +
                $"style-src 'self' 'unsafe-inline'; " +
                $"img-src 'self' data: {Constants.NewsDashboard.Url}; " +
                $"connect-src 'self'; " +
                $"font-src 'self'; " +
                $"frame-src 'self' {Constants.Marketplace.Url}");

            await next();
        });

        return app;
    }
}
