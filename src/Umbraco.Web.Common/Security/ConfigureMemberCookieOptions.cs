using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            OnRedirectToAccessDenied = ctx =>
            {
                // When the controller is an UmbracoAPIController, we want to return a StatusCode instead of a redirect.
                // All other cases should use the default Redirect of the CookieAuthenticationEvent.
                var controllerDescriptor = ctx.HttpContext.GetEndpoint()?.Metadata
                    .OfType<ControllerActionDescriptor>()
                    .FirstOrDefault();

                if (!controllerDescriptor?.ControllerTypeInfo.IsSubclassOf(typeof(UmbracoApiController)) ?? false)
                {
                    new CookieAuthenticationEvents().OnRedirectToAccessDenied(ctx);
                }

                return Task.CompletedTask;
            },
        };
    }
}
