using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Options;

public class ConfigurePropertySettingsOptions :
    IConfigureOptions<ContentPropertySettings>,
    IConfigureOptions<MemberPropertySettings>,
    IConfigureOptions<MediaPropertySettings>
{
    private readonly ModelsBuilderSettings _settings;

    public ConfigurePropertySettingsOptions(IOptions<ModelsBuilderSettings> config)
    {
        _settings = config.Value;
    }

    public void Configure(ContentPropertySettings options)
    {
        if (_settings.ModelsMode is ModelsMode.Nothing)
        {
            return;
        }

        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        options.AddReservedFieldNames(reservedProperties);
        options.AddReservedFieldNames(reservedMethods);
    }

    public void Configure(MemberPropertySettings options)
    {
        if (_settings.ModelsMode is ModelsMode.Nothing)
        {
            return;
        }

        var reservedProperties = typeof(IPublishedMember).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedMember).GetPublicMethods().Select(x => x.Name).ToHashSet();
        options.AddReservedFieldNames(reservedProperties);
        options.AddReservedFieldNames(reservedMethods);
    }

    public void Configure(MediaPropertySettings options)
    {
        if (_settings.ModelsMode is ModelsMode.Nothing)
        {
            return;
        }

        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        options.AddReservedFieldNames(reservedProperties);
        options.AddReservedFieldNames(reservedMethods);
    }
}
