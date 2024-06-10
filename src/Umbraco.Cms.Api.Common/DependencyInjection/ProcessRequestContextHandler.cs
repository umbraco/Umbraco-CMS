using Microsoft.AspNetCore.Http;
using OpenIddict.Server;
using OpenIddict.Validation;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

public class ProcessRequestContextHandler
    : IOpenIddictServerHandler<OpenIddictServerEvents.ProcessRequestContext>, IOpenIddictValidationHandler<OpenIddictValidationEvents.ProcessRequestContext>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _backOfficePathSegment;

    public ProcessRequestContextHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _backOfficePathSegment = Constants.System.DefaultUmbracoPath.TrimStart(Constants.CharArrays.Tilde)
            .EnsureStartsWith('/')
            .EnsureEndsWith('/');
    }

    public ValueTask HandleAsync(OpenIddictServerEvents.ProcessRequestContext context)
    {
        if (SkipOpenIddictHandlingForRequest())
        {
            context.SkipRequest();
        }

        return ValueTask.CompletedTask;
    }

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

        return requestPath.StartsWith(_backOfficePathSegment) is false;
    }
}
