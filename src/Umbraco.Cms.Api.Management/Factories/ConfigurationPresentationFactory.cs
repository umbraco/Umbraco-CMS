using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class ConfigurationPresentationFactory : IConfigurationPresentationFactory
{
    private readonly MemberPropertySettings _memberPropertySettings;
    private readonly ContentPropertySettings _contentPropertySettings;
    private readonly GlobalSettings _globalSettings;
    private readonly ContentSettings _contentSettings;
    private readonly SegmentSettings _segmentSettings;

    public ConfigurationPresentationFactory(
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IOptionsSnapshot<ContentSettings> contentSettings,
        IOptionsSnapshot<SegmentSettings> segmentSettings,
        IOptionsSnapshot<ContentPropertySettings> contentPropertySettings,
        IOptionsSnapshot<MemberPropertySettings> memberPropertySettings)
    {
        _memberPropertySettings = memberPropertySettings.Value;
        _contentPropertySettings = contentPropertySettings.Value;
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
            ReservedFieldNames = GetDocumentReservedFieldNames(),
        };

    public MemberConfigurationResponseModel CreateMemberConfigurationResponseModel() =>
        new()
        {
            ReservedFieldNames = GetMemberReservedFieldNames(),
        };

    public MediaConfigurationResponseModel CreateMediaConfigurationResponseModel()
    {
        var responseModel = new MediaConfigurationResponseModel
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            SanitizeTinyMce = _globalSettings.SanitizeTinyMce,
            ReservedFieldNames = GetMediaReservedFieldNames(),
        };
        return responseModel;
    }

    private ISet<string> GetMediaReservedFieldNames()
    {
        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        reservedProperties.UnionWith(reservedMethods);
        reservedProperties.UnionWith(_memberPropertySettings.ReservedFieldNames);

        return reservedProperties;
    }

    private ISet<string> GetMemberReservedFieldNames()
    {
        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        reservedProperties.UnionWith(reservedMethods);
        reservedProperties.UnionWith(_memberPropertySettings.ReservedFieldNames);

        return reservedProperties;
    }

    private ISet<string> GetDocumentReservedFieldNames()
    {
        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        reservedProperties.UnionWith(reservedMethods);
        reservedProperties.UnionWith(_contentPropertySettings.ReservedFieldNames);

        return reservedProperties;
    }
}
