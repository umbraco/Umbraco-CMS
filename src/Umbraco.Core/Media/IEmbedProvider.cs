namespace Umbraco.Cms.Core.Media;

public interface IEmbedProvider
{
    /// <summary>
    ///     The OEmbed API Endpoint
    /// </summary>
    string ApiEndpoint { get; }

    /// <summary>
    ///     A string array of Regex patterns to match against the pasted OEmbed URL
    /// </summary>
    string[] UrlSchemeRegex { get; }

    /// <summary>
    ///     A collection of querystring request parameters to append to the API URL
    /// </summary>
    /// <example>?key=value&amp;key2=value2</example>
    Dictionary<string, string> RequestParams { get; }

    [Obsolete("Use GetMarkupAsync instead. This will be removed in Umbraco 15.")]
    string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0);

    Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken) => Task.FromResult(GetMarkup(url, maxWidth ?? 0, maxHeight ?? 0));
}
