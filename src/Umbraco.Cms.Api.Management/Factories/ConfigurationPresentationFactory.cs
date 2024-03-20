using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class ConfigurationPresentationFactory : IConfigurationPresentationFactory
{
    private readonly IReservedFieldNamesService _reservedFieldNamesService;
    private readonly GlobalSettings _globalSettings;
    private readonly ContentSettings _contentSettings;
    private readonly SegmentSettings _segmentSettings;

    public ConfigurationPresentationFactory(
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IOptionsSnapshot<ContentSettings> contentSettings,
        IOptionsSnapshot<SegmentSettings> segmentSettings,
        IReservedFieldNamesService reservedFieldNamesService)
    {
        _reservedFieldNamesService = reservedFieldNamesService;
        _globalSettings = globalSettings.Value;
        _contentSettings = contentSettings.Value;
        _segmentSettings = segmentSettings.Value;
    }

    public DocumentConfigurationResponseModel CreateDocumentConfigurationModel() =>
        new()
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            SanitizeTinyMce = _globalSettings.SanitizeTinyMce,
            AllowEditInvariantFromNonDefault = _contentSettings.AllowEditInvariantFromNonDefault,
            AllowNonExistingSegmentsCreation = _segmentSettings.AllowCreation,
            ReservedFieldNames = _reservedFieldNamesService.GetDocumentReservedFieldNames(),
        };

    public MemberConfigurationResponseModel CreateMemberConfigurationResponseModel() =>
        new()
        {
            ReservedFieldNames = _reservedFieldNamesService.GetMemberReservedFieldNames(),
        };

    public MediaConfigurationResponseModel CreateMediaConfigurationResponseModel()
    {
        var responseModel = new MediaConfigurationResponseModel
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            SanitizeTinyMce = _globalSettings.SanitizeTinyMce,
            ReservedFieldNames = _reservedFieldNamesService.GetMediaReservedFieldNames(),
        };
        return responseModel;
    }
}
