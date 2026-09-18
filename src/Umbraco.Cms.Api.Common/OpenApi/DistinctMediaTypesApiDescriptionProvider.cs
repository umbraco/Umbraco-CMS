using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Collapses repeated media types in API descriptions so each request or response media type is listed once.
/// </summary>
/// <remarks>
/// The API explorer records one format per formatter able to handle a media type. Registering several formatters
/// for the same media type, as happens when a formatter is added per named JSON options, therefore lists that
/// media type several times, and OpenAPI document generation would render each entry as an alternative schema.
/// </remarks>
internal sealed class DistinctMediaTypesApiDescriptionProvider : IApiDescriptionProvider
{
    /// <summary>
    /// Providers run <see cref="OnProvidersExecuted"/> in descending order, so the lowest order runs last, after every
    /// provider that adds or clones descriptions.
    /// </summary>
    public int Order => int.MinValue;

    /// <inheritdoc />
    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {
    }

    /// <inheritdoc />
    public void OnProvidersExecuted(ApiDescriptionProviderContext context)
    {
        foreach (ApiDescription description in context.Results)
        {
            RemoveRepeatedMediaTypes(description.SupportedRequestFormats, format => format.MediaType);

            foreach (ApiResponseType responseType in description.SupportedResponseTypes)
            {
                RemoveRepeatedMediaTypes(responseType.ApiResponseFormats, format => format.MediaType);
            }
        }
    }

    private static void RemoveRepeatedMediaTypes<TFormat>(IList<TFormat> formats, Func<TFormat, string?> mediaType)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < formats.Count; i++)
        {
            if (mediaType(formats[i]) is { } type && seen.Add(type) is false)
            {
                formats.RemoveAt(i--);
            }
        }
    }
}
