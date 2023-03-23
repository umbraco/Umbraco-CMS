using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Services;

public class RequestStartNodeService : RequestHeaderService, IRequestStartNodeService
{
    private readonly GlobalSettings _globalSettings;

    public RequestStartNodeService(IHttpContextAccessor httpContextAccessor, IOptions<GlobalSettings> globalSettings)
        : base(httpContextAccessor) =>
        _globalSettings = globalSettings.Value;

    protected override string HeaderName => "start-node";

    /// <inheritdoc/>
    public string? GetRequestedStartNodePath()
        => _globalSettings.HideTopLevelNodeFromPath
            ? null
            : GetHeaderValue()?.Trim(Constants.CharArrays.ForwardSlash);
}
