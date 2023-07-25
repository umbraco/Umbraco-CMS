using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public class MediaTypeEditingService : ContentTypeEditingServiceBase<IMediaType, IMediaTypeService, MediaTypeCreateModel, MediaTypePropertyTypeModel, MediaTypePropertyContainerModel>, IMediaTypeEditingService
{
    private readonly IMediaTypeService _mediaTypeService;

    public async Task<Attempt<IMediaType?, ContentTypeOperationStatus>> CreateAsync(MediaTypeCreateModel model, Guid userKey)
    {
        Attempt<IMediaType?, ContentTypeOperationStatus> result = await HandleCreateAsync(model, userKey);
        if (result.Success)
        {
            _mediaTypeService.Save(result.Result);
        }

        return result;
    }

    public MediaTypeEditingService(
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper)
        : base(contentTypeService, mediaTypeService, dataTypeService, entityService, shortStringHelper)
        => _mediaTypeService = mediaTypeService;

    protected override Guid[] GetAvailableCompositionKeys(IContentTypeComposition? source, IContentTypeComposition[] allContentTypes, MediaTypeCreateModel model)
        => Array.Empty<Guid>();

    protected override IMediaType CreateContentType(IShortStringHelper shortStringHelper, int parentId)
        => new MediaType(shortStringHelper, parentId);

    protected override bool SupportsPublishing => false;

    protected override UmbracoObjectTypes ContentObjectType => UmbracoObjectTypes.MediaType;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.MediaTypeContainer;
}
