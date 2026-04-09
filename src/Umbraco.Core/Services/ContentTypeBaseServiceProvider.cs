using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides the appropriate content type service based on the type of content base entity.
/// </summary>
public class ContentTypeBaseServiceProvider : IContentTypeBaseServiceProvider
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeBaseServiceProvider"/> class.
    /// </summary>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="mediaTypeService">The media type service.</param>
    /// <param name="memberTypeService">The member type service.</param>
    public ContentTypeBaseServiceProvider(IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;
    }

    /// <inheritdoc />
    public IContentTypeBaseService For(IContentBase contentBase)
    {
        if (contentBase == null)
        {
            throw new ArgumentNullException(nameof(contentBase));
        }

        switch (contentBase)
        {
            case IContent _:
                return _contentTypeService;
            case IMedia _:
                return _mediaTypeService;
            case IMember _:
                return _memberTypeService;
            default:
                throw new ArgumentException(
                    $"Invalid contentBase type: {contentBase.GetType().FullName}",
                    nameof(contentBase));
        }
    }

    /// <inheritdoc />
    // note: this should be a default interface method with C# 8
    public IContentTypeComposition? GetContentTypeOf(IContentBase contentBase)
    {
        if (contentBase == null)
        {
            throw new ArgumentNullException(nameof(contentBase));
        }

        return For(contentBase).Get(contentBase.ContentTypeId);
    }
}
