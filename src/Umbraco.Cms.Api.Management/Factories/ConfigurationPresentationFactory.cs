using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class ConfigurationPresentationFactory : IConfigurationPresentationFactory
{
    private readonly IReservedFieldNamesService _reservedFieldNamesService;
    private readonly ContentSettings _contentSettings;
    private readonly SegmentSettings _segmentSettings;

    public ConfigurationPresentationFactory(
        IReservedFieldNamesService reservedFieldNamesService,
        IOptions<ContentSettings> contentSettings,
        IOptions<SegmentSettings> segmentSettings)
    {
        _reservedFieldNamesService = reservedFieldNamesService;
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
            ReservedFieldNames = _reservedFieldNamesService.GetDocumentReservedFieldNames(),
        };

    public MemberConfigurationResponseModel CreateMemberConfigurationResponseModel() =>
        new()
        {
            ReservedFieldNames = _reservedFieldNamesService.GetMemberReservedFieldNames(),
        };

    public MediaConfigurationResponseModel CreateMediaConfigurationResponseModel() =>
        new()
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            ReservedFieldNames = _reservedFieldNamesService.GetMediaReservedFieldNames(),
        };
}
