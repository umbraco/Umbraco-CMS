namespace Umbraco.Cms.Core.Media;

/// <summary>
///     Defines a provider for embedding media content from external sources using OEmbed protocol.
/// </summary>
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

    /// <summary>
    ///     Gets the HTML markup for embedding the media content from the specified URL.
    /// </summary>
    /// <param name="url">The URL of the media to embed.</param>
    /// <param name="maxWidth">The maximum width of the embedded content, or <c>null</c> for no constraint.</param>
    /// <param name="maxHeight">The maximum height of the embedded content, or <c>null</c> for no constraint.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the HTML markup or <c>null</c> if unavailable.</returns>
    Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken);
}
