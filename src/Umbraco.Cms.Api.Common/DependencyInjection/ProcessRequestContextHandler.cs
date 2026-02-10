using Microsoft.AspNetCore.Http;
using OpenIddict.Server;
using OpenIddict.Validation;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

/// <summary>
///     Handles OpenIddict request processing to skip handling for non-authentication requests.
/// </summary>
/// <remarks>
///     This handler prevents OpenIddict from processing every request to the server,
///     limiting its scope to back-office and well-known OpenID Connect endpoints.
/// </remarks>
public class ProcessRequestContextHandler
    : IOpenIddictServerHandler<OpenIddictServerEvents.ProcessRequestContext>, IOpenIddictValidationHandler<OpenIddictValidationEvents.ProcessRequestContext>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string[] _pathsToHandle;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProcessRequestContextHandler"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public ProcessRequestContextHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        var backOfficePathSegment = Constants.System.DefaultUmbracoPath.TrimStart(Constants.CharArrays.Tilde)
            .EnsureStartsWith('/')
            .EnsureEndsWith('/');
        _pathsToHandle = [backOfficePathSegment, "/.well-known/openid-configuration", "/.well-known/jwks"];
    }

    /// <summary>
    ///     Handles the server process request context event.
    /// </summary>
    /// <param name="context">The process request context.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public ValueTask HandleAsync(OpenIddictServerEvents.ProcessRequestContext context)
    {
        if (SkipOpenIddictHandlingForRequest())
        {
            context.SkipRequest();
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Handles the validation process request context event.
    /// </summary>
    /// <param name="context">The process request context.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public ValueTask HandleAsync(OpenIddictValidationEvents.ProcessRequestContext context)
    {
        if (SkipOpenIddictHandlingForRequest())
        {
            context.SkipRequest();
        }

        return ValueTask.CompletedTask;
    }

    private bool SkipOpenIddictHandlingForRequest()
    {
        var requestPath = _httpContextAccessor.HttpContext?.Request.Path.Value;
        if (requestPath.IsNullOrWhiteSpace())
        {
            return false;
        }

        foreach (var path in _pathsToHandle)
        {
            if (requestPath.StartsWith(path))
            {
                return false;
            }
        }

        return true;
    }
}
