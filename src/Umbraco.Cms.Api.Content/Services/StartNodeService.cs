using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Services;

public class StartNodeService : IStartNodeService
{
    private const string StartNodeHeaderName = "start-node";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StartNodeService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public string? GetStartNode()
    {
        HttpContext? context = _httpContextAccessor.HttpContext;

        if (context is not null && context.Request.Headers.TryGetValue(StartNodeHeaderName, out StringValues headerValue))
        {
            var startNodeHeader = headerValue.ToString();
            return WebUtility.UrlDecode(startNodeHeader).TrimStart(Constants.CharArrays.ForwardSlash);
        }

        return null;
    }
}
