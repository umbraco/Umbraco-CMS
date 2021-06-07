using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Middleware
{
    /// <summary>
    /// Ensures that preview pages (front-end routed) are authenticated with the back office identity appended to the principal alongside any default authentication that takes place
    /// </summary>
    public class PreviewAuthenticationMiddleware : IMiddleware
    {
        private readonly ILogger<PreviewAuthenticationMiddleware> _logger;

        public PreviewAuthenticationMiddleware(ILogger<PreviewAuthenticationMiddleware> logger) => _logger = logger;

        /// <inheritdoc/>
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
                                && context.User != null
                                && !request.IsBackOfficeRequest();

                // If we've gotten this far it means a preview cookie has been set and a front-end umbraco document request is executing.
                // In this case, authentication will not have occurred for an Umbraco back office User, however we need to perform the authentication
                // for the user here so that the preview capability can be authorized otherwise only the non-preview page will be rendered.
                if (isPreview)
                {
                    context.AddBackOfficeIdentityFromAuthenticationCookie();
                }
            }
            catch (Exception ex)
            {
                // log any errors and continue the request without preview
                _logger.LogError($"Unable to perform preview authentication: {ex.Message}");
            }
            finally
            {
                await next(context);
            }
        }
    }
}
