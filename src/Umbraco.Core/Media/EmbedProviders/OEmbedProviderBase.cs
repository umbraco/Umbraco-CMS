using System.Net;
using System.Text;
using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Abstract base class for OEmbed providers that implements the <see cref="IEmbedProvider"/> interface.
/// </summary>
public abstract class OEmbedProviderBase : IEmbedProvider
{
    private static HttpClient? _httpClient;
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OEmbedProviderBase"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    protected OEmbedProviderBase(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public abstract string ApiEndpoint { get; }

    /// <inheritdoc />
    public abstract string[] UrlSchemeRegex { get; }

    /// <inheritdoc />
    public abstract Dictionary<string, string> RequestParams { get; }

    /// <inheritdoc />
    public abstract Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the full OEmbed provider URL including the URL to embed and dimension constraints.
    /// </summary>
    /// <param name="url">The URL of the media to embed.</param>
    /// <param name="maxWidth">The maximum width, or <c>null</c> for no constraint.</param>
    /// <param name="maxHeight">The maximum height, or <c>null</c> for no constraint.</param>
    /// <returns>The full OEmbed provider URL with query parameters.</returns>
    public virtual string GetEmbedProviderUrl(string url, int? maxWidth, int? maxHeight) => GetEmbedProviderUrl(url, maxWidth ?? 0, maxHeight ?? 0);

    /// <summary>
    ///     Gets the full OEmbed provider URL including the URL to embed and dimension constraints.
    /// </summary>
    /// <param name="url">The URL of the media to embed.</param>
    /// <param name="maxWidth">The maximum width, or 0 for no constraint.</param>
    /// <param name="maxHeight">The maximum height, or 0 for no constraint.</param>
    /// <returns>The full OEmbed provider URL with query parameters.</returns>
    public virtual string GetEmbedProviderUrl(string url, int maxWidth, int maxHeight)
    {
        if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) == false)
        {
            throw new ArgumentException("Not a valid URL.", nameof(url));
        }

        var fullUrl = new StringBuilder();

        fullUrl.Append(ApiEndpoint);
        fullUrl.Append("?url=" + WebUtility.UrlEncode(url));

        foreach (KeyValuePair<string, string> param in RequestParams)
        {
            fullUrl.Append($"&{param.Key}={param.Value}");
        }

        if (maxWidth > 0)
        {
            fullUrl.Append("&maxwidth=" + maxWidth);
        }

        if (maxHeight > 0)
        {
            fullUrl.Append("&maxheight=" + maxHeight);
        }

        return fullUrl.ToString();
    }

    /// <summary>
    ///     Downloads the response content from the specified URL.
    /// </summary>
    /// <param name="url">The URL to download from.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the response content as a string.</returns>
    public virtual async Task<string> DownloadResponseAsync(string url, CancellationToken cancellationToken)
    {
        if (_httpClient == null)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(Constants.HttpClients.Headers.UserAgentProductName);
        }

        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }

    /// <summary>
    ///     Gets the JSON response from the specified URL and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="url">The URL to download from.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the deserialized response or <c>null</c>.</returns>
    public virtual async Task<T?> GetJsonResponseAsync<T>(string url, CancellationToken cancellationToken)
        where T : class
    {
        var response = await DownloadResponseAsync(url, cancellationToken);
        return _jsonSerializer.Deserialize<T>(response);
    }

    /// <summary>
    ///     Gets the XML response from the specified URL.
    /// </summary>
    /// <param name="url">The URL to download from.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the response as an <see cref="XmlDocument"/>.</returns>
    public virtual async Task<XmlDocument> GetXmlResponseAsync(string url, CancellationToken cancellationToken)
    {
        var response =  await DownloadResponseAsync(url, cancellationToken);
        var doc = new XmlDocument();
        doc.LoadXml(response);

        return doc;
    }

    /// <summary>
    ///     Gets a property value from an XML document using an XPath expression.
    /// </summary>
    /// <param name="doc">The XML document.</param>
    /// <param name="property">The XPath expression to select the property.</param>
    /// <returns>The inner text of the selected node, or an empty string if not found.</returns>
    public virtual string GetXmlProperty(XmlDocument doc, string property)
    {
        XmlNode? selectSingleNode = doc.SelectSingleNode(property);
        return selectSingleNode != null ? selectSingleNode.InnerText : string.Empty;
    }

    /// <summary>
    ///     Gets the HTML markup for embedding media using a JSON-based OEmbed response.
    /// </summary>
    /// <param name="url">The URL of the media to embed.</param>
    /// <param name="maxWidth">The maximum width, or <c>null</c> for no constraint.</param>
    /// <param name="maxHeight">The maximum height, or <c>null</c> for no constraint.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the HTML markup or <c>null</c>.</returns>
    public virtual async Task<string?> GetJsonBasedMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        var requestUrl = GetEmbedProviderUrl(url, maxWidth, maxHeight);
        OEmbedResponse? oembed = await GetJsonResponseAsync<OEmbedResponse>(requestUrl, cancellationToken);

        return oembed?.GetHtml();
    }

    /// <summary>
    ///     Gets the HTML markup for embedding media using an XML-based OEmbed response.
    /// </summary>
    /// <param name="url">The URL of the media to embed.</param>
    /// <param name="maxWidth">The maximum width, or <c>null</c> for no constraint.</param>
    /// <param name="maxHeight">The maximum height, or <c>null</c> for no constraint.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <param name="property">The XPath expression to select the HTML property. Defaults to "/oembed/html".</param>
    /// <returns>A task that represents the asynchronous operation, containing the HTML markup or <c>null</c>.</returns>
    public virtual async Task<string?> GetXmlBasedMarkupAsync(
        string url,
        int? maxWidth,
        int? maxHeight,
        CancellationToken cancellationToken,
        string property = "/oembed/html")
    {
        var requestUrl = GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = await GetXmlResponseAsync(requestUrl, cancellationToken);

        return GetXmlProperty(xmlDocument, property);
    }
}
