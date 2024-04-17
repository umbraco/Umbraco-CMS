// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class MultiUrlPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IProfilingLogger _proflog;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IApiMediaUrlProvider _apiMediaUrlProvider;
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;

    [Obsolete("Use constructor that takes all parameters, scheduled for removal in V14")]
    public MultiUrlPickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IProfilingLogger proflog,
        IJsonSerializer jsonSerializer,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedUrlProvider publishedUrlProvider)
        : this(
            publishedSnapshotAccessor,
            proflog,
            jsonSerializer,
            umbracoContextAccessor,
            publishedUrlProvider,
            StaticServiceProvider.Instance.GetRequiredService<IApiContentNameProvider>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiMediaUrlProvider>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiContentRouteBuilder>())
    {
    }

    public MultiUrlPickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IProfilingLogger proflog,
        IJsonSerializer jsonSerializer,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedUrlProvider publishedUrlProvider,
        IApiContentNameProvider apiContentNameProvider,
        IApiMediaUrlProvider apiMediaUrlProvider,
        IApiContentRouteBuilder apiContentRouteBuilder)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                     throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        _proflog = proflog ?? throw new ArgumentNullException(nameof(proflog));
        _jsonSerializer = jsonSerializer;
        _publishedUrlProvider = publishedUrlProvider;
        _apiContentNameProvider = apiContentNameProvider;
        _apiMediaUrlProvider = apiMediaUrlProvider;
        _apiContentRouteBuilder = apiContentRouteBuilder;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        Constants.PropertyEditors.Aliases.MultiUrlPicker.Equals(propertyType.EditorAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) =>
        IsSingleUrlPicker(propertyType)
            ? typeof(Link)
            : typeof(IEnumerable<Link>);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) =>
        PropertyCacheLevel.Snapshot;

    public override bool? IsValue(object? value, PropertyValueLevel level) =>
        value is not null && value.ToString() != "[]";

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview) => source?.ToString()!;

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        using (!_proflog.IsEnabled(Core.Logging.LogLevel.Debug) ? null : _proflog.DebugDuration<MultiUrlPickerValueConverter>(
                   $"ConvertPropertyToLinks ({propertyType.DataType.Id})"))
        {
            var maxNumber = propertyType.DataType.ConfigurationAs<MultiUrlPickerConfiguration>()!.MaxNumber;

            if (string.IsNullOrWhiteSpace(inter?.ToString()))
            {
                return maxNumber == 1 ? null : Enumerable.Empty<Link>();
            }

            var links = new List<Link>();
            IEnumerable<MultiUrlPickerValueEditor.LinkDto>? dtos = ParseLinkDtos(inter.ToString()!);
            IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
            if (dtos is null)
            {
                return links;
            }

            foreach (MultiUrlPickerValueEditor.LinkDto dto in dtos)
            {
                LinkType type = LinkType.External;
                var url = dto.Url;
                var name = dto.Name;
                IPublishedContent? content = null;

                if (dto.Udi is not null)
                {
                    type = dto.Udi.EntityType == Constants.UdiEntityType.Media
                        ? LinkType.Media
                        : LinkType.Content;

                    content = type == LinkType.Media
                        ? publishedSnapshot.Media?.GetById(preview, dto.Udi.Guid)
                        : publishedSnapshot.Content?.GetById(preview, dto.Udi.Guid);

                    if (content == null || content.ContentType.ItemType == PublishedItemType.Element)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(name))
                    {
                        name = content.Name;
                    }

                    url = content.Url(_publishedUrlProvider);
                }

                links.Add(
                    new Link
                    {
                        Name = name,
                        Target = dto.Target,
                        Type = type,
                        Udi = dto.Udi,
                        Content = content,
                        Url = url + dto.QueryString,
                    });
            }

            if (maxNumber == 1)
            {
                return links.FirstOrDefault();
            }

            if (maxNumber > 0)
            {
                return links.Take(maxNumber);
            }

            return links;
        }
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Elements;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<ApiLink>);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        IEnumerable<ApiLink> DefaultValue() => Array.Empty<ApiLink>();

        if (inter is not string value || value.IsNullOrWhiteSpace())
        {
            return DefaultValue();
        }

        MultiUrlPickerValueEditor.LinkDto[]? dtos = ParseLinkDtos(value)?.ToArray();
        if (dtos == null || dtos.Any() == false)
        {
            return DefaultValue();
        }

        IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();

        ApiLink? ToLink(MultiUrlPickerValueEditor.LinkDto item)
        {
            switch (item.Udi?.EntityType)
            {
                case Constants.UdiEntityType.Document:
                    IPublishedContent? content = publishedSnapshot.Content?.GetById(item.Udi.Guid);
                    IApiContentRoute? route = content != null
                        ? _apiContentRouteBuilder.Build(content)
                        : null;
                    return content == null || route == null
                        ? null
                        : ApiLink.Content(
                            item.Name.IfNullOrWhiteSpace(_apiContentNameProvider.GetName(content)),
                            item.QueryString,
                            item.Target,
                            content.Key,
                            content.ContentType.Alias,
                            route);
                case Constants.UdiEntityType.Media:
                    IPublishedContent? media = publishedSnapshot.Media?.GetById(item.Udi.Guid);
                    return media == null
                        ? null
                        : ApiLink.Media(
                            item.Name.IfNullOrWhiteSpace(_apiContentNameProvider.GetName(media)),
                            $"{_apiMediaUrlProvider.GetUrl(media)}{item.QueryString}",
                            item.QueryString,
                            item.Target,
                            media.Key,
                            media.ContentType.Alias);
                default:
                    return ApiLink.External(item.Name, $"{item.Url}{item.QueryString}", item.QueryString, item.Target);
            }
        }

        return dtos.Select(ToLink).WhereNotNull().ToArray();
    }

    private static bool IsSingleUrlPicker(IPublishedPropertyType propertyType)
        => propertyType.DataType.ConfigurationAs<MultiUrlPickerConfiguration>()!.MaxNumber == 1;

    private IEnumerable<MultiUrlPickerValueEditor.LinkDto>? ParseLinkDtos(string inter)
        => inter.DetectIsJson() ? _jsonSerializer.Deserialize<IEnumerable<MultiUrlPickerValueEditor.LinkDto>>(inter) : null;
}
