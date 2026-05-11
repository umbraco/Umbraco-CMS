using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Middleware;

/// <summary>
/// Ensures that requests to the media in the recycle bin are authorized and only authenticated back-office users
/// with permissions for the media have access.
/// </summary>
public class ProtectRecycleBinMediaMiddleware : IMiddleware
{
    private readonly IUserService _userService;
    private ContentSettings _contentSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtectRecycleBinMediaMiddleware"/> class.
    /// </summary>
    public ProtectRecycleBinMediaMiddleware(
        IUserService userService,
        IOptionsMonitor<ContentSettings> contentSettings)
    {
        _userService = userService;
        _contentSettings = contentSettings.CurrentValue;
        contentSettings.OnChange(x => _contentSettings = x);
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_contentSettings.EnableMediaRecycleBinProtection is false)
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

        Guid? userKey = authenticateResult.Principal?.Identity?.GetUserKey();
        if (userKey is null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        IUser? user = await _userService.GetAsync(userKey.Value);
        if (user is null || user.AllowedSections.Contains(Core.Constants.Applications.Media) is false)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next(context);
    }
}
