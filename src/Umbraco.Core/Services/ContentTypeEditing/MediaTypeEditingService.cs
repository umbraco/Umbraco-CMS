using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

internal sealed class MediaTypeEditingService : ContentTypeEditingServiceBase<IMediaType, IMediaTypeService, MediaTypePropertyTypeModel, MediaTypePropertyContainerModel>, IMediaTypeEditingService
{
    private readonly IMediaTypeService _mediaTypeService;

    public MediaTypeEditingService(
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper)
        : base(contentTypeService, mediaTypeService, dataTypeService, entityService, shortStringHelper)
        => _mediaTypeService = mediaTypeService;

    public async Task<Attempt<IMediaType?, ContentTypeOperationStatus>> CreateAsync(MediaTypeCreateModel model, Guid userKey)
    {
        Attempt<IMediaType?, ContentTypeOperationStatus> result = await ValidateAndMapForCreationAsync(model, model.Key, model.ParentKey);
        if (result.Success)
        {
            IMediaType mediaType = result.Result ?? throw new InvalidOperationException($"{nameof(ValidateAndMapForCreationAsync)} succeeded but did not yield any result");
            await _mediaTypeService.SaveAsync(mediaType, userKey);
        }

        return result;
    }

    public async Task<Attempt<IMediaType?, ContentTypeOperationStatus>> UpdateAsync(IMediaType mediaType, MediaTypeUpdateModel model, Guid userKey)
    {
        Attempt<IMediaType?, ContentTypeOperationStatus> result = await ValidateAndMapForUpdateAsync(mediaType, model);
        if (result.Success)
        {
            mediaType = result.Result ?? throw new InvalidOperationException($"{nameof(ValidateAndMapForUpdateAsync)} succeeded but did not yield any result");
            await _mediaTypeService.SaveAsync(mediaType, userKey);
        }

        return result;
    }

    protected override Guid[] GetAvailableCompositionKeys(IContentTypeComposition? source, IContentTypeComposition[] allContentTypes, bool isElement)
        => Array.Empty<Guid>();

    protected override IMediaType CreateContentType(IShortStringHelper shortStringHelper, int parentId)
        => new MediaType(shortStringHelper, parentId);

    protected override bool SupportsPublishing => false;

    protected override UmbracoObjectTypes ContentTypeObjectType => UmbracoObjectTypes.MediaType;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.MediaTypeContainer;
}
