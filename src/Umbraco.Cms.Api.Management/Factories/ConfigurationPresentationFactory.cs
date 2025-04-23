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

public class ConfigurationPresentationFactory : IConfigurationPresentationFactory
{
    private readonly IReservedFieldNamesService _reservedFieldNamesService;
    private readonly UmbracoFeatures _umbracoFeatures;
    private readonly DataTypesSettings _dataTypesSettings;
    private readonly ContentSettings _contentSettings;
    private readonly SegmentSettings _segmentSettings;

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

    public DocumentConfigurationResponseModel CreateDocumentConfigurationResponseModel() =>
        new()
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            AllowEditInvariantFromNonDefault = _contentSettings.AllowEditInvariantFromNonDefault,
            AllowNonExistingSegmentsCreation = _segmentSettings.AllowCreation,
        };

    public DocumentTypeConfigurationResponseModel CreateDocumentTypeConfigurationResponseModel() =>
        new()
        {
            DataTypesCanBeChanged = _dataTypesSettings.CanBeChanged,
            DisableTemplates = _umbracoFeatures.Disabled.DisableTemplates,
            UseSegments = _segmentSettings.Enabled,
            ReservedFieldNames = _reservedFieldNamesService.GetDocumentReservedFieldNames(),
        };

    public MemberConfigurationResponseModel CreateMemberConfigurationResponseModel() =>
        new();

    public MemberTypeConfigurationResponseModel CreateMemberTypeConfigurationResponseModel() =>
        new()
        {
            ReservedFieldNames = _reservedFieldNamesService.GetMemberReservedFieldNames(),
        };

    public MediaConfigurationResponseModel CreateMediaConfigurationResponseModel() =>
        new()
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
        };

    public MediaTypeConfigurationResponseModel CreateMediaTypeConfigurationResponseModel() =>
        new()
        {
            ReservedFieldNames = _reservedFieldNamesService.GetMediaReservedFieldNames(),
        };
}
