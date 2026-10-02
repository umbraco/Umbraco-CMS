using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Server;
using OpenIddict.Validation;
using Umbraco.Cms.Core.DependencyInjection;
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
    [Obsolete("Please use the constructor that accepts all parameters. Scheduled for removal in Umbraco 19.")]
    public ProcessRequestContextHandler(IHttpContextAccessor httpContextAccessor)
        : this(
              httpContextAccessor,
              StaticServiceProvider.Instance.GetRequiredService<IOpenIddictPathsToHandleProvider>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProcessRequestContextHandler"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="pathsToHandleProvider">The provider for the request paths that OpenIddict should handle.</param>
    public ProcessRequestContextHandler(
        IHttpContextAccessor httpContextAccessor,
        IOpenIddictPathsToHandleProvider pathsToHandleProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _pathsToHandle = pathsToHandleProvider.GetPathsToHandle().ToArray();
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
