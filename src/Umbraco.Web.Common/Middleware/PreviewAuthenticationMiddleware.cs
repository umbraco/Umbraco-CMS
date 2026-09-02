using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Middleware;

/// <summary>
///     Ensures that preview pages (front-end routed) are authenticated with the back office identity appended to the
///     principal alongside any default authentication that takes place
/// </summary>
public class PreviewAuthenticationMiddleware : IMiddleware
{
    private readonly ILogger<PreviewAuthenticationMiddleware> _logger;
    private readonly IPreviewSessionService _previewSessionService;

    public PreviewAuthenticationMiddleware(
        ILogger<PreviewAuthenticationMiddleware> logger,
        IPreviewSessionService previewSessionService)
    {
        _logger = logger;
        _previewSessionService = previewSessionService;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        HttpRequest request = context.Request;

        // do not process if client-side request
        if (request.IsClientSideRequest())
        {
            await next(context);
            return;
        }

        try
        {
            var isPreview = request.HasPreviewCookie()
                            && !request.IsBackOfficeRequest();

            if (isPreview)
            {
                AuthenticateResult authenticateResult = await context.AuthenticateAsync(Core.Constants.Security.BackOfficeAuthenticationType);
                if (authenticateResult.Succeeded)
                {
                    ClaimsIdentity? umbracoIdentity = authenticateResult.Principal.GetUmbracoIdentity();
                    if (umbracoIdentity is not null)
                    {
                        // Ok, we've got a real ticket, now we can add this ticket's identity to the current
                        // Principal, this means we'll have 2 identities assigned to the principal which we can
                        // use to authorize the preview and allow for a back office User.
                        context.User.AddIdentity(umbracoIdentity);
                        _previewSessionService.Start();
                    }
                    else
                    {
                        _logger.LogDebug("Could not get the current Umbraco user for preview.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // log any errors and continue the request without preview
            _logger.LogError("Unable to perform preview authentication: {message}", ex.Message);
        }
        finally
        {
            await next(context);
        }
    }
}
