using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class ConfigurationPresentationFactory : IConfigurationPresentationFactory
{
    private readonly ContentPropertySettings _contentPropertySettings;
    private readonly GlobalSettings _globalSettings;
    private readonly ContentSettings _contentSettings;
    private readonly SegmentSettings _segmentSettings;

    public ConfigurationPresentationFactory(IOptionsSnapshot<GlobalSettings> globalSettings, IOptionsSnapshot<ContentSettings> contentSettings, IOptionsSnapshot<SegmentSettings> segmentSettings, IOptionsSnapshot<ContentPropertySettings> contentPropertySettings)
    {
        _contentPropertySettings = contentPropertySettings.Value;
        _globalSettings = globalSettings.Value;
        _contentSettings = contentSettings.Value;
        _segmentSettings = segmentSettings.Value;
    }

    public DocumentConfigurationResponseModel Create() =>
        new()
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            SanitizeTinyMce = _globalSettings.SanitizeTinyMce,
            AllowEditInvariantFromNonDefault = _contentSettings.AllowEditInvariantFromNonDefault,
            AllowNonExistingSegmentsCreation = _segmentSettings.AllowCreation,
            ReservedFieldNames = GetReservedFieldNames(),
        };

    private ISet<string> GetReservedFieldNames()
    {
        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        reservedProperties.UnionWith(reservedMethods);
        reservedProperties.UnionWith(_contentPropertySettings.ReservedFieldNames);

        return reservedProperties;
    }
}
