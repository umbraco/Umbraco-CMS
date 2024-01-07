using Umbraco.Cms.Api.Management.ViewModels.ContentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal abstract class ContentPresentationFactoryBase<TContentType, TContentTypeService>
    where TContentTypeService : IContentTypeBaseService<TContentType>
    where TContentType : IContentTypeComposition
{
    private readonly TContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    protected ContentPresentationFactoryBase(TContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    protected TContentTypeReferenceResponseModel CreateContentTypeReferenceResponseModel<TContentTypeReferenceResponseModel>(IContentEntitySlim entity)
        where TContentTypeReferenceResponseModel : ContentTypeReferenceResponseModelBase, new()
    {
        // This sucks, since it'll cost an extra DB call.
        // but currently there's no really good way to get the content type key from an IDocumentEntitySlim or IMediaEntitySlim.
        // FIXME: to fix this, add content type key and "IsContainer" to IDocumentEntitySlim and IMediaEntitySlim, and use those here instead of fetching the entire content type.
        TContentType? contentType = _contentTypeService.Get(entity.ContentTypeAlias);
        return contentType is not null
            ? _umbracoMapper.Map<TContentTypeReferenceResponseModel>(contentType)!
            : new TContentTypeReferenceResponseModel();
    }
}
