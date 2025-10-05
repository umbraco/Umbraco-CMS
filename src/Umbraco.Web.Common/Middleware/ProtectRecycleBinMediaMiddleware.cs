using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.Extensions;

namespace Umbraco.Cms.Web.Common.Middleware;

/// <summary>
/// Ensures that requests to the media in the recycle bin are authorized and only authenticated back-office users
/// with permissions for the media have access.
/// </summary>
public class ProtectRecycleBinMediaMiddleware : IMiddleware
{
    private ImagingSettings _imagingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtectRecycleBinMediaMiddleware"/> class.
    /// </summary>
    public ProtectRecycleBinMediaMiddleware(
        IOptionsMonitor<ImagingSettings> imagingSettings)
    {
        _imagingSettings = imagingSettings.CurrentValue;
        imagingSettings.OnChange(x => _imagingSettings = x);
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_imagingSettings.EnableMediaRecycleBinProtection is false)
        {
            await next(context);
            return;
        }

        string? requestPath = context.Request.Path.Value;

        if (string.IsNullOrEmpty(requestPath) ||
            requestPath.StartsWithNormalizedPath($"/media/", StringComparison.OrdinalIgnoreCase) is false ||
            requestPath.Contains(Core.Constants.Conventions.Media.TrashedMediaSuffix + ".") is false)
        {
            await next(context);
            return;
        }

        AuthenticateResult authenticateResult = await context.AuthenticateAsync(Core.Constants.Security.BackOfficeExposedAuthenticationType);
        if (authenticateResult.Succeeded is false)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        Claim? mediaSectionClaim = authenticateResult.Principal.Claims
            .FirstOrDefault(x => x.Type == Core.Constants.Security.AllowedApplicationsClaimType && x.Value == Core.Constants.Applications.Media);

        if (mediaSectionClaim is null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next(context);
    }
}
