// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Converts the value stored by a URL picker property editor into strongly-typed link objects that can be easily
/// consumed by application code.
/// </summary>
/// <remarks>
/// There is one URL picker editor per number of links a picker holds, so that the type a URL picker property yields
/// follows from the editor rather than from the configuration of the data type it is used through. Both store the
/// same value - a JSON array - which is what this base reads.
/// </remarks>
public abstract class MultiUrlPickerValueConverterBase : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IProfilingLogger _proflog;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IApiMediaUrlProvider _apiMediaUrlProvider;
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;
    private readonly IPublishedContentCache _contentCache;
    private readonly IPublishedMediaCache _mediaCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiUrlPickerValueConverterBase"/> class with the specified dependencies.
    /// </summary>
    /// <param name="proflog">The <see cref="IProfilingLogger"/> used for profiling and logging.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> used for serializing and deserializing JSON data.</param>
    /// <param name="publishedUrlProvider">The <see cref="IPublishedUrlProvider"/> used to provide published URLs.</param>
    /// <param name="apiContentNameProvider">The <see cref="IApiContentNameProvider"/> used to provide API content names.</param>
    /// <param name="apiMediaUrlProvider">The <see cref="IApiMediaUrlProvider"/> used to provide API media URLs.</param>
    /// <param name="apiContentRouteBuilder">The <see cref="IApiContentRouteBuilder"/> used to build API content routes.</param>
    /// <param name="contentCache">The <see cref="IPublishedContentCache"/> used to cache published content.</param>
    /// <param name="mediaCache">The <see cref="IPublishedMediaCache"/> used to cache published media.</param>
    protected MultiUrlPickerValueConverterBase(
        IProfilingLogger proflog,
        IJsonSerializer jsonSerializer,
        IPublishedUrlProvider publishedUrlProvider,
        IApiContentNameProvider apiContentNameProvider,
        IApiMediaUrlProvider apiMediaUrlProvider,
        IApiContentRouteBuilder apiContentRouteBuilder,
        IPublishedContentCache contentCache,
        IPublishedMediaCache mediaCache)
    {
        _proflog = proflog ?? throw new ArgumentNullException(nameof(proflog));
        _jsonSerializer = jsonSerializer;
        _publishedUrlProvider = publishedUrlProvider;
        _apiContentNameProvider = apiContentNameProvider;
        _apiMediaUrlProvider = apiMediaUrlProvider;
        _apiContentRouteBuilder = apiContentRouteBuilder;
        _contentCache = contentCache;
        _mediaCache = mediaCache;
    }

    /// <summary>
    /// Gets a value indicating whether the editor this converter serves holds more than one link.
    /// </summary>
    protected abstract bool HoldsMultipleLinks { get; }

    /// <summary>
    /// Gets the cache level at which the property value is stored for the Multi URL Picker value converter.
    /// </summary>
    /// <param name="propertyType">The published property type for which to determine the cache level.</param>
    /// <returns>The <see cref="PropertyCacheLevel.Snapshot"/> cache level, indicating the value is cached for the duration of a published snapshot.</returns>
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) =>
        PropertyCacheLevel.Snapshot;

    /// <summary>
    /// Determines whether the specified value should be considered a valid value for the multi URL picker property editor.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <param name="level">The level of the property value.</param>
    /// <returns>
    /// <c>true</c> if the value is not <c>null</c> and not an empty array (i.e., not equal to "[]"); otherwise, <c>false</c>.
    /// </returns>
    public override bool? IsValue(object? value, PropertyValueLevel level) =>
        value is not null && value.ToString() != "[]";

    /// <summary>
    /// Converts the source value to its string representation as an intermediate value.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The type of the published property.</param>
    /// <param name="source">The source value to convert.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>The string representation of the source value, or <c>null</c> if the source is <c>null</c>.</returns>
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview) => source?.ToString();

    /// <summary>
    /// Converts the intermediate value to the final object representation for the MultiUrlPicker property editor.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The published property type.</param>
    /// <param name="referenceCacheLevel">The cache level for reference resolution.</param>
    /// <param name="inter">The intermediate value to convert.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>
    /// A collection of <see cref="Link"/>, or - for an editor holding a single link - one <see cref="Link"/>, or
    /// <c>null</c> if none are present.
    /// </returns>
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        using (!_proflog.IsEnabled(Core.Logging.LogLevel.Debug) ? null : _proflog.DebugDuration<MultiUrlPickerValueConverterBase>(
                   $"ConvertPropertyToLinks ({propertyType.DataType.Id})"))
        {
            if (string.IsNullOrWhiteSpace(inter?.ToString()))
            {
                return HoldsMultipleLinks ? Enumerable.Empty<Link>() : null;
            }

            var links = new List<Link>();
            IEnumerable<MultiUrlPickerValueEditor.LinkDto>? dtos = ParseLinkDtos(inter.ToString()!);
            if (dtos is null)
            {
                return links;
            }

            foreach (MultiUrlPickerValueEditor.LinkDto dto in dtos)
            {
                LinkType type = LinkType.External;
                var url = dto.Url;

                if (dto.Udi is not null)
                {
                    type = dto.Udi.EntityType == Constants.UdiEntityType.Media
                        ? LinkType.Media
                        : LinkType.Content;

                    IPublishedContent? content = type == LinkType.Media
                        ? _mediaCache.GetById(preview, dto.Udi.Guid)
                        : _contentCache.GetById(preview, dto.Udi.Guid);

                    if (content == null || content.ContentType.ItemType == PublishedItemType.Element)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(dto.Name))
                    {
                        dto.Name = content.Name;
                    }

                    url = content.Url(_publishedUrlProvider, dto.Culture);
                }

                links.Add(
                    new Link
                    {
                        Name = dto.Name,
                        Target = dto.Target,
                        Type = type,
                        Udi = dto.Udi,
                        Url = url + dto.QueryString,
                    });
            }

            return HoldsMultipleLinks ? links : links.FirstOrDefault();
        }
    }

    /// <summary>
    /// Returns the cache level to be used by the delivery API for the specified published property type.
    /// </summary>
    /// <param name="propertyType">The published property type for which to determine the cache level.</param>
    /// <returns>The <see cref="PropertyCacheLevel"/> value indicating the cache level for the delivery API.</returns>
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Elements;

    /// <summary>
    /// Gets the type used by the Delivery API to represent the value of a Multi URL Picker property.
    /// </summary>
    /// <param name="propertyType">The published property type (not used in this implementation).</param>
    /// <returns>The <see cref="IEnumerable{ApiLink}"/> type.</returns>
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<ApiLink>);

    /// <summary>
    /// Converts the intermediate value of a multi URL picker property to an object suitable for the Delivery API.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The type of the published property.</param>
    /// <param name="referenceCacheLevel">The cache level for property references.</param>
    /// <param name="inter">The intermediate value to convert, typically a JSON string representing the links.</param>
    /// <param name="preview">A value indicating whether the conversion is for preview mode.</param>
    /// <param name="expanding">A value indicating whether nested properties are being expanded during conversion.</param>
    /// <returns>
    /// An array of <see cref="ApiLink"/> objects representing the converted links for the Delivery API, or an empty array if the intermediate value is null, empty, or cannot be parsed.
    /// </returns>
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        IEnumerable<ApiLink> DefaultValue() => Array.Empty<ApiLink>();

        if (inter is not string value || value.IsNullOrWhiteSpace())
        {
            return DefaultValue();
        }

        MultiUrlPickerValueEditor.LinkDto[]? dtos = ParseLinkDtos(value)?.ToArray();
        if (dtos == null || dtos.Length == 0)
        {
            return DefaultValue();
        }

        ApiLink? ToLink(MultiUrlPickerValueEditor.LinkDto item)
        {
            switch (item.Udi?.EntityType)
            {
                case Constants.UdiEntityType.Document:
                    IPublishedContent? content = _contentCache.GetById(item.Udi.Guid);
                    IApiContentRoute? route = content != null
                        ? _apiContentRouteBuilder.Build(content, item.Culture)
                        : null;
                    return content == null || route == null
                        ? null
                        : ApiLink.Content(
                            item.Name.IfNullOrWhiteSpace(_apiContentNameProvider.GetName(content)),
                            item.QueryString,
                            item.Target,
                            content.Key,
                            content.ContentType.Alias,
                            route,
                            item.Culture);
                case Constants.UdiEntityType.Media:
                    IPublishedContent? media = _mediaCache.GetById(item.Udi.Guid);
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

    private IEnumerable<MultiUrlPickerValueEditor.LinkDto>? ParseLinkDtos(string inter)
        => inter.DetectIsJson() ? _jsonSerializer.Deserialize<IEnumerable<MultiUrlPickerValueEditor.LinkDto>>(inter) : null;
}
