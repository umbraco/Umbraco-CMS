using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Services;

internal sealed class ApiAccessService : RequestHeaderHandler, IApiAccessService
{
    private ContentApiSettings _contentApiSettings;

    public ApiAccessService(IHttpContextAccessor httpContextAccessor, IOptionsMonitor<ContentApiSettings> contentApiSettings)
        : base(httpContextAccessor)
    {
        _contentApiSettings = contentApiSettings.CurrentValue;
        contentApiSettings.OnChange(settings => _contentApiSettings = settings);
    }

    /// <inheritdoc />
    public bool HasPublicAccess() => IfEnabled(() => _contentApiSettings.PublicAccess || HasValidApiKey());

    /// <inheritdoc />
    public bool HasPreviewAccess() => IfEnabled(HasValidApiKey);

    private bool IfEnabled(Func<bool> condition) => _contentApiSettings.Enabled && condition();

    private bool HasValidApiKey() => _contentApiSettings.ApiKey.IsNullOrWhiteSpace() == false
                                     && _contentApiSettings.ApiKey.Equals(GetHeaderValue("Api-Key"));
}
