using Microsoft.AspNetCore.HttpOverrides;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Web.UI.Composers;

/// <summary>
/// Extends the forwarded headers middleware to also process X-Forwarded-Host.
/// </summary>
/// <remarks>
/// <para>
/// <c>ASPNETCORE_FORWARDEDHEADERS_ENABLED=true</c> (set in <c>.devcontainer/devcontainer.json</c>)
/// processes <c>X-Forwarded-For</c> and <c>X-Forwarded-Proto</c> but intentionally excludes
/// <c>X-Forwarded-Host</c> for security reasons.
/// </para>
/// <para>
/// GitHub Codespaces' port-forwarding proxy sets <c>Host: localhost:&lt;port&gt;</c> on requests
/// forwarded to the container and puts the real external hostname in <c>X-Forwarded-Host</c>.
/// Without this composer, <c>Request.Host</c> stays as <c>localhost</c>, causing ASP.NET Core's
/// authentication middleware to generate login redirect URLs pointing at localhost rather than
/// the forwarded Codespaces hostname.
/// </para>
/// <para>
/// This Composer is not shipped by the Umbraco.Templates package.
/// </para>
/// </remarks>
public class ReverseProxyComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder) =>
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
            options.ForwardedHeaders |= ForwardedHeaders.XForwardedHost);
}
