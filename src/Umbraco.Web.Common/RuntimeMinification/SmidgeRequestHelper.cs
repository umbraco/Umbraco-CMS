using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Smidge;
using Smidge.Models;

namespace Umbraco.Cms.Web.Common.RuntimeMinification;

public class SmidgeRequestHelper : IRequestHelper
{
    private readonly RequestHelper _wrappedRequestHelper;

    public SmidgeRequestHelper(IWebsiteInfo siteInfo) => _wrappedRequestHelper = new RequestHelper(siteInfo);

    /// <inheritdoc />
    public string Content(string path) => _wrappedRequestHelper.Content(path);

    /// <inheritdoc />
    public string Content(IWebFile file) => _wrappedRequestHelper.Content(file);

    /// <inheritdoc />
    public bool IsExternalRequestPath(string path) => _wrappedRequestHelper.IsExternalRequestPath(path);

    /// <summary>
    ///     Overrides the default order of compression from Smidge, since Brotli is super slow (~10 seconds for backoffice.js)
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public CompressionType GetClientCompression(IDictionary<string, StringValues> headers)
    {
        CompressionType type = CompressionType.None;

        if (headers is not IHeaderDictionary headerDictionary)
        {
            headerDictionary = new HeaderDictionary(headers.Count);
            foreach ((var key, StringValues stringValues) in headers)
            {
                headerDictionary[key] = stringValues;
            }
        }

        var acceptEncoding = headerDictionary.GetCommaSeparatedValues(HeaderNames.AcceptEncoding);
        if (acceptEncoding.Length > 0)
        {
            // Prefer in order: GZip, Deflate, Brotli.
            for (var i = 0; i < acceptEncoding.Length; i++)
            {
                var encoding = acceptEncoding[i].Trim();

                var parsed = CompressionType.Parse(encoding);

                // Not pack200-gzip.
                if (parsed == CompressionType.GZip)
                {
                    return CompressionType.GZip;
                }

                if (parsed == CompressionType.Deflate)
                {
                    type = CompressionType.Deflate;
                }

                // Brotli is typically last in the accept encoding header.
                if (type != CompressionType.Deflate && parsed == CompressionType.Brotli)
                {
                    type = CompressionType.Brotli;
                }
            }
        }

        return type;
    }
}
