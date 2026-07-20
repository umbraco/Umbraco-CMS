using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Media;

/// <summary>
/// Defines the mapping configuration for media entities within the Umbraco CMS API management layer.
/// </summary>
public class MediaMapDefinition : ContentMapDefinition<IMedia, MediaValueResponseModel, MediaVariantResponseModel>, IMapDefinition
{
    private readonly CommonMapper _commonMapper;
    private ContentSettings _contentSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Mapping.Media.MediaMapDefinition"/> class with the specified dependencies.
    /// </summary>
    /// <param name="propertyEditorCollection">The <see cref="PropertyEditorCollection"/> containing available property editors.</param>
    /// <param name="commonMapper">The <see cref="CommonMapper"/> instance used for common mapping operations.</param>
    /// <param name="dataValueEditorFactory">The <see cref="IDataValueEditorFactory"/> used to create data value editors.</param>
    /// <param name="contentSettings">The <see cref="IOptionsMonitor{ContentSettings}"/> providing access to content settings options.</param>
    public MediaMapDefinition(
        PropertyEditorCollection propertyEditorCollection,
        CommonMapper commonMapper,
        IDataValueEditorFactory dataValueEditorFactory,
        IOptionsMonitor<ContentSettings> contentSettings)
        : base(propertyEditorCollection, dataValueEditorFactory)
    {
        _commonMapper = commonMapper;
        _contentSettings = contentSettings.CurrentValue;
        contentSettings.OnChange(x => _contentSettings = x);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaMapDefinition"/> class.
    /// </summary>
    /// <param name="propertyEditorCollection">A collection containing the available property editors.</param>
    /// <param name="commonMapper">An instance of <see cref="CommonMapper"/> used for common mapping operations.</param>
    /// <param name="dataValueEditorFactory">A factory for creating data value editors.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 18.")]
    public MediaMapDefinition(
        PropertyEditorCollection propertyEditorCollection,
        CommonMapper commonMapper,
        IDataValueEditorFactory dataValueEditorFactory)
        : this(
              propertyEditorCollection,
              commonMapper,
              dataValueEditorFactory,
              StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<ContentSettings>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Mapping.Media.MediaMapDefinition"/> class.
    /// </summary>
    /// <param name="propertyEditorCollection">A collection containing the property editors used for mapping media properties.</param>
    /// <param name="commonMapper">An instance of <see cref="CommonMapper"/> used for common mapping operations.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 18.")]
    public MediaMapDefinition(
        PropertyEditorCollection propertyEditorCollection,
        CommonMapper commonMapper)
        : this(
            propertyEditorCollection,
            commonMapper,
            StaticServiceProvider.Instance.GetRequiredService<IDataValueEditorFactory>())
    {
    }

    /// <summary>
    /// Configures the object mappings for media entities, defining how <see cref="IMedia"/> instances are mapped to <see cref="MediaResponseModel"/> and <see cref="MediaCollectionResponseModel"/>.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to register the mappings.</param>
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IMedia, MediaResponseModel>((_, _) => new MediaResponseModel(), Map);
        mapper.Define<IMedia, MediaCollectionResponseModel>((_, _) => new MediaCollectionResponseModel(), Map);
    }

    // Umbraco.Code.MapAll -Urls -Flags
    private void Map(IMedia source, MediaResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.MediaType = context.Map<MediaTypeReferenceResponseModel>(source.ContentType)!;
        target.Values = MapValueViewModels(source.Properties);
        target.Variants = MapVariantViewModels(source);
        target.IsTrashed = source.Trashed;

        // If protection for media files in the recycle bin is enabled, and the media item is trashed, amend the value of the file path
        // to have the `.deleted` suffix that will have been added to the persisted file.
        if (target.IsTrashed && _contentSettings.EnableMediaRecycleBinProtection)
        {
            foreach (MediaValueResponseModel valueModel in target.Values
                .Where(x => x.EditorAlias.Equals(Core.Constants.PropertyEditors.Aliases.ImageCropper)))
            {
                if (valueModel.Value is not null &&
                    valueModel.Value is ImageCropperValue imageCropperValue &&
                    string.IsNullOrWhiteSpace(imageCropperValue.Src) is false)
                {
                    valueModel.Value = new ImageCropperValue
                    {
                        Crops = imageCropperValue.Crops,
                        FocalPoint = imageCropperValue.FocalPoint,
                        TemporaryFileId = imageCropperValue.TemporaryFileId,
                        Src = SuffixMediaPath(imageCropperValue.Src, Core.Constants.Conventions.Media.TrashedMediaSuffix),
                    };
                }
            }
        }
    }

    private static string SuffixMediaPath(string filePath, string suffix)
    {
        int lastDotIndex = filePath.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            return filePath + suffix;
        }

        return filePath[..lastDotIndex] + suffix + filePath[lastDotIndex..];
    }

    // Umbraco.Code.MapAll -Flags
    private void Map(IMedia source, MediaCollectionResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.MediaType = context.Map<MediaTypeCollectionReferenceResponseModel>(source.ContentType)!;
        target.SortOrder = source.SortOrder;
        target.Creator = _commonMapper.GetOwnerName(source, context);

        // If there's a set of property aliases specified in the collection configuration, we will check if the current property's
        // value should be mapped. If it isn't one of the ones specified in 'includeProperties', we will just return the result
        // without mapping the value.
        var includedProperties = context.GetIncludedProperties();

        IEnumerable<IProperty> properties = source.Properties;
        if (includedProperties is not null)
        {
            properties = properties.Where(property => includedProperties.Contains(property.Alias));
        }

        target.Values = MapValueViewModels(properties);
        target.Variants = MapVariantViewModels(source);
    }
}
