using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Content.Services;

public class RequestCultureService : RequestHeaderService, IRequestCultureService
{
    private readonly IVariationContextAccessor _variationContextAccessor;

    public RequestCultureService(IHttpContextAccessor httpContextAccessor, IVariationContextAccessor variationContextAccessor)
        : base(httpContextAccessor) =>
        _variationContextAccessor = variationContextAccessor;

    protected override string HeaderName => HeaderNames.AcceptLanguage;

    public string? GetRequestedCulture() => GetHeaderValue();

    public void SetRequestCulture(string culture)
    {
        if (_variationContextAccessor.VariationContext?.Culture == culture)
        {
            return;
        }

        _variationContextAccessor.VariationContext = new VariationContext(culture);
    }
}
