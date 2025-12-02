using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

public sealed class ConfigureMemberCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
{
    private readonly IRuntimeState _runtimeState;
    private readonly UmbracoRequestPaths _umbracoRequestPaths;

    public ConfigureMemberCookieOptions(IRuntimeState runtimeState, UmbracoRequestPaths umbracoRequestPaths)
    {
        _runtimeState = runtimeState;
        _umbracoRequestPaths = umbracoRequestPaths;
    }

    public void Configure(string? name, CookieAuthenticationOptions options)
    {
        if (name == IdentityConstants.ApplicationScheme || name == IdentityConstants.ExternalScheme)
        {
            Configure(options);
        }
    }

    public void Configure(CookieAuthenticationOptions options)
    {
        // TODO: We may want/need to configure these further
        options.LoginPath = null;
        options.AccessDeniedPath = null;
        options.LogoutPath = null;

        options.CookieManager = new MemberCookieManager(_runtimeState, _umbracoRequestPaths);

        options.Events = new CookieAuthenticationEvents
        {
            OnSignedIn = ctx =>
            {
                // occurs when sign in is successful and after the ticket is written to the outbound cookie

                // When we are signed in with the cookie, assign the principal to the current HttpContext
                ctx.HttpContext.SetPrincipalForRequest(ctx.Principal);

                return Task.CompletedTask;
            },
            OnValidatePrincipal = async ctx =>
            {
                // We need to resolve the BackOfficeSecurityStampValidator per request as a requirement (even in aspnetcore they do this)
                MemberSecurityStampValidator securityStampValidator =
                    ctx.HttpContext.RequestServices.GetRequiredService<MemberSecurityStampValidator>();

                await securityStampValidator.ValidateAsync(ctx);
            },
            // retain the login redirect behavior in .NET 10
            // - see https://learn.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/10/cookie-authentication-api-endpoints
            OnRedirectToLogin = context =>
            {
                if (IsXhr(context.Request))
                {
                    context.Response.Headers.Location = context.RedirectUri;
                    context.Response.StatusCode = 401;
                }
                else
                {
                    context.Response.Redirect(context.RedirectUri);
                }

                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                // TODO: rewrite this to match OnRedirectToLogin (with a 403 status code) when UmbracoApiController is removed
                // When the controller is an UmbracoAPIController, or if the request is an XHR, we want to return a
                // StatusCode instead of a redirect.
                // All other cases should use the default Redirect of the CookieAuthenticationEvent.
                if (IsXhr(context.Request) is false && IsUmbracoApiControllerRequest(context.HttpContext) is false)
                {
                    new CookieAuthenticationEvents().OnRedirectToAccessDenied(context);
                }

                return Task.CompletedTask;
            },
        };
        return;

        bool IsUmbracoApiControllerRequest(HttpContext context)
            => context.GetEndpoint()
                ?.Metadata
                .OfType<ControllerActionDescriptor>()
                .FirstOrDefault()
                ?.ControllerTypeInfo
                .IsSubclassOf(typeof(UmbracoApiController)) is true;

        bool IsXhr(HttpRequest request) =>
            string.Equals(request.Query[HeaderNames.XRequestedWith], "XMLHttpRequest", StringComparison.Ordinal) ||
            string.Equals(request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.Ordinal);
    }
}
