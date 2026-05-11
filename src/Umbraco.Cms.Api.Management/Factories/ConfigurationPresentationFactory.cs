using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create configuration presentation models for the API management layer.
/// </summary>
public class ConfigurationPresentationFactory : IConfigurationPresentationFactory
{
    private readonly IReservedFieldNamesService _reservedFieldNamesService;
    private readonly UmbracoFeatures _umbracoFeatures;
    private readonly DataTypesSettings _dataTypesSettings;
    private readonly ContentSettings _contentSettings;
    private readonly SegmentSettings _segmentSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationPresentationFactory"/> class.
    /// </summary>
    /// <param name="reservedFieldNamesService">Service used to manage reserved field names.</param>
    /// <param name="contentSettings">The options for content settings.</param>
    /// <param name="segmentSettings">The options for segment settings.</param>
    /// <param name="dataTypesSettings">The options for data types settings.</param>
    /// <param name="umbracoFeatures">Configuration for Umbraco features.</param>
    public ConfigurationPresentationFactory(
        IReservedFieldNamesService reservedFieldNamesService,
        IOptions<ContentSettings> contentSettings,
        IOptions<SegmentSettings> segmentSettings,
        IOptions<DataTypesSettings> dataTypesSettings,
        UmbracoFeatures umbracoFeatures)
    {
        _reservedFieldNamesService = reservedFieldNamesService;
        _umbracoFeatures = umbracoFeatures;
        _dataTypesSettings = dataTypesSettings.Value;
        _contentSettings = contentSettings.Value;
        _segmentSettings = segmentSettings.Value;
    }

    /// <summary>
    /// Creates a <see cref="DocumentConfigurationResponseModel"/> populated with the current content and segment configuration settings.
    /// </summary>
    /// <returns>A <see cref="DocumentConfigurationResponseModel"/> containing the current document configuration values.</returns>
    public DocumentConfigurationResponseModel CreateDocumentConfigurationResponseModel() =>
        new()
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            AllowEditInvariantFromNonDefault = _contentSettings.AllowEditInvariantFromNonDefault,
            AllowNonExistingSegmentsCreation = _segmentSettings.AllowCreation,
        };

    /// <summary>
    /// Creates a new <see cref="Umbraco.Cms.Api.Management.Models.DocumentTypeConfigurationResponseModel"/> instance populated with the current document type configuration settings, including data type mutability, template availability, segment usage, and reserved field names.
    /// </summary>
    /// <returns>
    /// A <see cref="Umbraco.Cms.Api.Management.Models.DocumentTypeConfigurationResponseModel"/> representing the current document type configuration.
    /// </returns>
    public DocumentTypeConfigurationResponseModel CreateDocumentTypeConfigurationResponseModel() =>
        new()
        {
            DataTypesCanBeChanged = _dataTypesSettings.CanBeChanged,
            DisableTemplates = _umbracoFeatures.Disabled.DisableTemplates,
            UseSegments = _segmentSettings.Enabled,
            ReservedFieldNames = _reservedFieldNamesService.GetDocumentReservedFieldNames(),
        };

    /// <summary>
    /// Instantiates and returns a new <see cref="MemberConfigurationResponseModel"/>.
    /// </summary>
    /// <returns>A new <see cref="MemberConfigurationResponseModel"/>.</returns>
    public MemberConfigurationResponseModel CreateMemberConfigurationResponseModel() =>
        new();

    /// <summary>
    /// Creates a <see cref="Umbraco.Cms.Api.Management.Models.MemberTypeConfigurationResponseModel"/> containing the reserved member field names.
    /// </summary>
    /// <returns>A <see cref="Umbraco.Cms.Api.Management.Models.MemberTypeConfigurationResponseModel"/> with reserved member field names populated.</returns>
    public MemberTypeConfigurationResponseModel CreateMemberTypeConfigurationResponseModel() =>
        new()
        {
            ReservedFieldNames = _reservedFieldNamesService.GetMemberReservedFieldNames(),
        };

    /// <summary>
    /// Creates a new instance of <see cref="Umbraco.Cms.Api.Management.Models.MediaConfigurationResponseModel"/> with media configuration settings.
    /// </summary>
    /// <returns>A <see cref="Umbraco.Cms.Api.Management.Models.MediaConfigurationResponseModel"/> containing the media configuration.</returns>
    public MediaConfigurationResponseModel CreateMediaConfigurationResponseModel() =>
        new()
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
        };

    /// <summary>
    /// Creates a new instance of <see cref="Umbraco.Cms.Api.Management.Models.MediaTypeConfigurationResponseModel"/> containing configuration data for media types.
    /// </summary>
    /// <returns>
    /// A <see cref="Umbraco.Cms.Api.Management.Models.MediaTypeConfigurationResponseModel"/> that includes the list of reserved field names for media types.
    /// </returns>
    public MediaTypeConfigurationResponseModel CreateMediaTypeConfigurationResponseModel() =>
        new()
        {
            ReservedFieldNames = _reservedFieldNamesService.GetMediaReservedFieldNames(),
        };
}
