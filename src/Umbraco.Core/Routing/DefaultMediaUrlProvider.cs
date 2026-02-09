using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Default media URL provider.
/// </summary>
public class DefaultMediaUrlProvider : IMediaUrlProvider
{
    private readonly MediaUrlGeneratorCollection _mediaPathGenerators;
    private readonly IUrlAssembler _urlAssembler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultMediaUrlProvider" /> class.
    /// </summary>
    /// <param name="mediaPathGenerators">The media path generators.</param>
    /// <param name="urlAssembler">The URL assembler.</param>
    public DefaultMediaUrlProvider(MediaUrlGeneratorCollection mediaPathGenerators, IUrlAssembler urlAssembler)
    {
        _mediaPathGenerators = mediaPathGenerators;
        _urlAssembler = urlAssembler;
    }

    /// <inheritdoc />
    public virtual UrlInfo? GetMediaUrl(
        IPublishedContent content,
        string propertyAlias,
        UrlMode mode,
        string? culture,
        Uri current)
    {
        IPublishedProperty? prop = content.GetProperty(propertyAlias);

        // get the raw source value since this is what is used by IDataEditorWithMediaPath for processing
        var value = prop?.GetSourceValue(culture);
        if (value == null)
        {
            return null;
        }

        IPublishedPropertyType? propType = prop?.PropertyType;

        if (_mediaPathGenerators.TryGetMediaPath(propType?.EditorAlias, value, out var path))
        {
            Uri url = _urlAssembler.AssembleUrl(path!, current, mode);
            return UrlInfo.FromUri(url, Constants.UrlProviders.Media, culture);
        }

        return null;
    }
}
