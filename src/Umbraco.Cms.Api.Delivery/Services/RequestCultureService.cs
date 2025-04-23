using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed partial class RequestCultureService : RequestHeaderHandler, IRequestCultureService
{
    public RequestCultureService(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

    /// <inheritdoc />
    public string? GetRequestedCulture()
    {
        var acceptLanguage = GetHeaderValue(HeaderNames.AcceptLanguage) ?? string.Empty;
        return ValidLanguageHeaderRegex().IsMatch(acceptLanguage) ? acceptLanguage : null;
    }

    [Obsolete("Use IVariationContextAccessor to manipulate the variation context. Scheduled for removal in V17.")]
    public void SetRequestCulture(string culture)
    {
        // no-op
    }

    // at the time of writing we're introducing this to get rid of accept-language header values like "en-GB,en-US;q=0.9,en;q=0.8",
    // so we don't want to be too restrictive in this regex - keep it simple for now.
    [GeneratedRegex(@"^[\w-]*$")]
    private static partial Regex ValidLanguageHeaderRegex();
}
