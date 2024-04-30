﻿using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

internal sealed class MediaTypeEditingService : ContentTypeEditingServiceBase<IMediaType, IMediaTypeService, MediaTypePropertyTypeModel, MediaTypePropertyContainerModel>, IMediaTypeEditingService
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IImageUrlGenerator _imageUrlGenerator;

    public MediaTypeEditingService(
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper,
        IImageUrlGenerator imageUrlGenerator)
        : base(contentTypeService, mediaTypeService, dataTypeService, entityService, shortStringHelper)
    {
        _mediaTypeService = mediaTypeService;
        _dataTypeService = dataTypeService;
        _imageUrlGenerator = imageUrlGenerator;
    }

    public async Task<Attempt<IMediaType?, ContentTypeOperationStatus>> CreateAsync(MediaTypeCreateModel model, Guid userKey)
    {
        Attempt<IMediaType?, ContentTypeOperationStatus> result = await ValidateAndMapForCreationAsync(model, model.Key, model.ContainerKey);
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

    public async Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositionsAsync(
        Guid? key,
        IEnumerable<Guid> currentCompositeKeys,
        IEnumerable<string> currentPropertyAliases) =>
        await FindAvailableCompositionsAsync(key, currentCompositeKeys, currentPropertyAliases);

    public async Task<IEnumerable<IMediaType>> GetMediaTypesForFileExtension(string fileExtension)
    {
        fileExtension = fileExtension.TrimStart(Constants.CharArrays.Period);

        IMediaType[] candidateMediaTypes = _mediaTypeService.GetAll().Where(mt => mt.CompositionPropertyTypes.Any(pt => pt.Alias == Constants.Conventions.Media.File)).ToArray();
        var allowedMediaTypes = new List<IMediaType>();

        // is this an image format supported by the image cropper?
        if (_imageUrlGenerator.IsSupportedImageFormat(fileExtension))
        {
            // yes - add all media types with an image cropper "file" property
            allowedMediaTypes.AddRange(candidateMediaTypes
                .Where(mt => mt.CompositionPropertyTypes.Any(propertyType => propertyType is
                {
                    Alias: Constants.Conventions.Media.File,
                    PropertyEditorAlias: Constants.PropertyEditors.Aliases.ImageCropper
                })));
        }

        // find media types that have an explicit allow-list of file extensions
        // - NOTE: an empty allow-list should be interpreted as "all file extensions are allowed"
        IDictionary<IMediaType, IEnumerable<string>> allowedFileExtensionsByMediaType = await FetchAllowedFileExtensionsByMediaType(candidateMediaTypes);

        // add all media types where the file extension is explicitly allowed
        allowedMediaTypes.AddRange(allowedFileExtensionsByMediaType
            .Where(kvp => kvp.Value.Contains(fileExtension))
            .Select(kvp => kvp.Key));

        // if we at this point have no allowed media types, add all media types that allow any file extension
        if (allowedMediaTypes.Any() is false)
        {
            allowedMediaTypes.AddRange(allowedFileExtensionsByMediaType
                .Where(kvp => kvp.Value.Any() is false)
                .Select(kvp => kvp.Key));
        }

        return allowedMediaTypes;
    }

    protected override IMediaType CreateContentType(IShortStringHelper shortStringHelper, int parentId)
        => new MediaType(shortStringHelper, parentId);

    protected override bool SupportsPublishing => false;

    protected override UmbracoObjectTypes ContentTypeObjectType => UmbracoObjectTypes.MediaType;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.MediaTypeContainer;

    private async Task<IDictionary<IMediaType, IEnumerable<string>>> FetchAllowedFileExtensionsByMediaType(IEnumerable<IMediaType> mediaTypes)
    {
        var allowedFileExtensionsByMediaType = new Dictionary<IMediaType, IEnumerable<string>>();
        foreach (IMediaType mediaType in mediaTypes)
        {
            IPropertyType? propertyType = mediaType
                .CompositionPropertyTypes
                .FirstOrDefault(propertyType => propertyType is
                {
                    Alias: Constants.Conventions.Media.File,
                    PropertyEditorAlias: Constants.PropertyEditors.Aliases.UploadField
                });
            if (propertyType is null)
            {
                continue;
            }

            IDataType? dataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey);
            FileUploadConfiguration? fileUploadConfiguration = dataType?.ConfigurationAs<FileUploadConfiguration>();

            if (fileUploadConfiguration is null)
            {
                continue;
            }

            allowedFileExtensionsByMediaType[mediaType] = fileUploadConfiguration.FileExtensions;
        }

        return allowedFileExtensionsByMediaType;
    }
}
