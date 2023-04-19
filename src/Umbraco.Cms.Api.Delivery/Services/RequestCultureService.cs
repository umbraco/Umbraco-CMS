using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class RequestCultureService : RequestHeaderHandler, IRequestCultureService
{
    private readonly IVariationContextAccessor _variationContextAccessor;

    public RequestCultureService(IHttpContextAccessor httpContextAccessor, IVariationContextAccessor variationContextAccessor)
        : base(httpContextAccessor) =>
        _variationContextAccessor = variationContextAccessor;

    /// <inheritdoc />
    public string? GetRequestedCulture() => GetHeaderValue(HeaderNames.AcceptLanguage);

    /// <inheritdoc />
    public void SetRequestCulture(string culture)
    {
        if (_variationContextAccessor.VariationContext?.Culture == culture)
        {
            return;
        }

        _variationContextAccessor.VariationContext = new VariationContext(culture);
    }
}
