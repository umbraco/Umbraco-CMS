using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

internal class ReservedFieldNamesService : IReservedFieldNamesService
{
    private readonly ContentPropertySettings _contentPropertySettings;
    private readonly MemberPropertySettings _memberPropertySettings;
    private readonly MediaPropertySettings _mediaPropertySettings;

    public ReservedFieldNamesService(
        IOptions<ContentPropertySettings> contentPropertySettings,
        IOptions<MemberPropertySettings> memberPropertySettings,
        IOptions<MediaPropertySettings> mediaPropertySettings)
    {
        _contentPropertySettings = contentPropertySettings.Value;
        _memberPropertySettings = memberPropertySettings.Value;
        _mediaPropertySettings = mediaPropertySettings.Value;
    }

    public ISet<string> GetDocumentReservedFieldNames()
    {
        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        reservedProperties.UnionWith(reservedMethods);
        reservedProperties.UnionWith(_contentPropertySettings.ReservedFieldNames);

        return reservedProperties;
    }

    public ISet<string> GetMediaReservedFieldNames()
    {
        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        reservedProperties.UnionWith(reservedMethods);
        reservedProperties.UnionWith(_mediaPropertySettings.ReservedFieldNames);

        return reservedProperties;
    }

    public ISet<string> GetMemberReservedFieldNames()
    {
        var reservedProperties = typeof(IPublishedMember).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedMember).GetPublicMethods().Select(x => x.Name).ToHashSet();
        reservedProperties.UnionWith(reservedMethods);
        reservedProperties.UnionWith(_memberPropertySettings.ReservedFieldNames);

        return reservedProperties;
    }
}
