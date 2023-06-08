using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed partial class RequestCultureService : RequestHeaderHandler, IRequestCultureService
{
    private readonly IVariationContextAccessor _variationContextAccessor;

    public RequestCultureService(IHttpContextAccessor httpContextAccessor, IVariationContextAccessor variationContextAccessor)
        : base(httpContextAccessor) =>
        _variationContextAccessor = variationContextAccessor;

    /// <inheritdoc />
    public string? GetRequestedCulture()
    {
        var acceptLanguage = GetHeaderValue(HeaderNames.AcceptLanguage) ?? string.Empty;
        return ValidLanguageHeaderRegex().IsMatch(acceptLanguage) ? acceptLanguage : null;
    }

    /// <inheritdoc />
    public void SetRequestCulture(string culture)
    {
        if (_variationContextAccessor.VariationContext?.Culture == culture)
        {
            return;
        }

        _variationContextAccessor.VariationContext = new VariationContext(culture);
    }

    // at the time of writing we're introducing this to get rid of accept-language header values like "en-GB,en-US;q=0.9,en;q=0.8",
    // so we don't want to be too restrictive in this regex - keep it simple for now.
    [GeneratedRegex(@"^[\w-]*$")]
    private static partial Regex ValidLanguageHeaderRegex();
}
