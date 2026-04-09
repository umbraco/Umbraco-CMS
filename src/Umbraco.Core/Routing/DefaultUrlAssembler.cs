using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides the default implementation of <see cref="IUrlAssembler" />.
/// </summary>
public class DefaultUrlAssembler : IUrlAssembler
{
    private readonly UriUtility _uriUtility;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultUrlAssembler" /> class.
    /// </summary>
    /// <param name="uriUtility">The URI utility.</param>
    public DefaultUrlAssembler(UriUtility uriUtility) => _uriUtility = uriUtility;

    /// <inheritdoc />
    public Uri AssembleUrl(string path, Uri current, UrlMode mode)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException($"{nameof(path)} cannot be null or whitespace", nameof(path));
        }

        // the path is absolute so we just return it as is
        if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
        {
            return new Uri(path);
        }

        Uri uri;

        switch (mode)
        {
            case UrlMode.Absolute:
                uri = new Uri(current.GetLeftPart(UriPartial.Authority) + path);
                break;
            case UrlMode.Relative:
            case UrlMode.Auto:
            case UrlMode.Default:
                uri = new Uri(path, UriKind.Relative);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }

        return _uriUtility.MediaUriFromUmbraco(uri);
    }
}
