using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public class ContentTypeBaseServiceProvider : IContentTypeBaseServiceProvider
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;

    public ContentTypeBaseServiceProvider(IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;
    }

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
