using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class ApiAccessService : RequestHeaderHandler, IApiAccessService
{
    private DeliveryApiSettings _deliveryApiSettings;

    public ApiAccessService(IHttpContextAccessor httpContextAccessor, IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
        : base(httpContextAccessor)
    {
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    /// <inheritdoc />
    public bool HasPublicAccess() => IfEnabled(() => _deliveryApiSettings.PublicAccess || HasValidApiKey());

    /// <inheritdoc />
    public bool HasPreviewAccess() => IfEnabled(HasValidApiKey);

    /// <inheritdoc />
    public bool HasMediaAccess() => IfMediaEnabled(() => _deliveryApiSettings is { PublicAccess: true, Media.PublicAccess: true } || HasValidApiKey());

    private bool IfEnabled(Func<bool> condition) => _deliveryApiSettings.Enabled && condition();

    private bool HasValidApiKey() => _deliveryApiSettings.ApiKey.IsNullOrWhiteSpace() == false
                                     && _deliveryApiSettings.ApiKey.Equals(GetHeaderValue("Api-Key"));

    private bool IfMediaEnabled(Func<bool> condition) => _deliveryApiSettings is { Enabled: true, Media.Enabled: true } && condition();
}
