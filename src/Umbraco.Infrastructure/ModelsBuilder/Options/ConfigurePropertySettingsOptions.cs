using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Options;

public class ConfigurePropertySettingsOptions :
    IConfigureOptions<ContentPropertySettings>,
    IConfigureOptions<MemberPropertySettings>,
    IConfigureOptions<MediaPropertySettings>
{

    public void Configure(ContentPropertySettings options)
    {
        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        options.AddReservedFieldNames(reservedProperties);
        options.AddReservedFieldNames(reservedMethods);
    }

    public void Configure(MemberPropertySettings options)
    {
        var reservedProperties = typeof(IPublishedMember).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedMember).GetPublicMethods().Select(x => x.Name).ToHashSet();
        options.AddReservedFieldNames(reservedProperties);
        options.AddReservedFieldNames(reservedMethods);
    }

    public void Configure(MediaPropertySettings options)
    {
        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        options.AddReservedFieldNames(reservedProperties);
        options.AddReservedFieldNames(reservedMethods);
    }
}
