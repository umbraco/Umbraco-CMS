using System.Net;
using System.Text;
using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

public abstract class OEmbedProviderBase : IEmbedProvider
{
    private static HttpClient? _httpClient;
    private readonly IJsonSerializer _jsonSerializer;

    protected OEmbedProviderBase(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    public abstract string ApiEndpoint { get; }

    public abstract string[] UrlSchemeRegex { get; }

    public abstract Dictionary<string, string> RequestParams { get; }

    [Obsolete("Use GetMarkupAsync instead. This will be removed in Umbraco 15.")]
    public abstract string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0);

    public virtual Task<string?> GeOEmbedDataAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken) => Task.FromResult(GetMarkup(url, maxWidth ?? 0, maxHeight ?? 0));

    public virtual string GetEmbedProviderUrl(string url, int? maxWidth, int? maxHeight) => GetEmbedProviderUrl(url, maxWidth ?? 0, maxHeight ?? 0);
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

    [Obsolete("Use DownloadResponseAsync instead. This will be removed in Umbraco 15.")]
    public virtual string DownloadResponse(string url)
    {
        if (_httpClient == null)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(Constants.HttpClients.Headers.UserAgentProductName);
        }

        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
        {
            using HttpResponseMessage response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().Result;
        }
    }

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

    [Obsolete("Use GetJsonResponseAsync instead. This will be removed in Umbraco 15.")]
    public virtual T? GetJsonResponse<T>(string url)
        where T : class
    {
        var response = DownloadResponse(url);
        return _jsonSerializer.Deserialize<T>(response);
    }

    public virtual async Task<T?> GetJsonResponseAsync<T>(string url, CancellationToken cancellationToken)
        where T : class
    {
        var response = await DownloadResponseAsync(url, cancellationToken);
        return _jsonSerializer.Deserialize<T>(response);
    }

    public virtual async Task<XmlDocument> GetXmlResponseAsync(string url, CancellationToken cancellationToken)
    {
        var response =  await DownloadResponseAsync(url, cancellationToken);
        var doc = new XmlDocument();
        doc.LoadXml(response);

        return doc;
    }

    [Obsolete("Use GetXmlResponseAsync instead. This will be removed in Umbraco 15.")]
    public virtual XmlDocument GetXmlResponse(string url)
    {
        var response = DownloadResponse(url);
        var doc = new XmlDocument();
        doc.LoadXml(response);

        return doc;
    }

    public virtual string GetXmlProperty(XmlDocument doc, string property)
    {
        XmlNode? selectSingleNode = doc.SelectSingleNode(property);
        return selectSingleNode != null ? selectSingleNode.InnerText : string.Empty;
    }
}
