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
/// Converts the value stored by the Multi URL Picker property editor into a collection of strongly-typed link objects
/// that can be easily consumed by application code.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class MultiUrlPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
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
    /// Initializes a new instance of the <see cref="MultiUrlPickerValueConverter"/> class with the specified dependencies.
    /// </summary>
    /// <param name="proflog">The <see cref="IProfilingLogger"/> used for profiling and logging.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> used for serializing and deserializing JSON data.</param>
    /// <param name="publishedUrlProvider">The <see cref="IPublishedUrlProvider"/> used to provide published URLs.</param>
    /// <param name="apiContentNameProvider">The <see cref="IApiContentNameProvider"/> used to provide API content names.</param>
    /// <param name="apiMediaUrlProvider">The <see cref="IApiMediaUrlProvider"/> used to provide API media URLs.</param>
    /// <param name="apiContentRouteBuilder">The <see cref="IApiContentRouteBuilder"/> used to build API content routes.</param>
    /// <param name="contentCache">The <see cref="IPublishedContentCache"/> used to cache published content.</param>
    /// <param name="mediaCache">The <see cref="IPublishedMediaCache"/> used to cache published media.</param>
    public MultiUrlPickerValueConverter(
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
    /// Determines whether this value converter should be used for the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type to evaluate.</param>
    /// <returns><c>true</c> if the property type uses the Multi Url Picker editor; otherwise, <c>false</c>.</returns>
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        Constants.PropertyEditors.Aliases.MultiUrlPicker.Equals(propertyType.EditorAlias);

    /// <summary>
    /// Determines and returns the .NET type that will be used to represent the value of the property,
    /// based on whether the property type is configured as a single or multiple URL picker.
    /// </summary>
    /// <param name="propertyType">The published property type to evaluate.</param>
    /// <returns>
    /// <see cref="Link"/> if the property is a single URL picker; otherwise, <see cref="IEnumerable{Link}"/> for multiple URLs.
    /// </returns>
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) =>
        IsSingleUrlPicker(propertyType)
            ? typeof(Link)
            : typeof(IEnumerable<Link>);

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
    /// If the maximum number of links is 1, returns a single <see cref="Link"/> or <c>null</c> if none are present.
    /// If the maximum number is greater than 1, returns a collection of <see cref="Link"/> up to the maximum allowed.
    /// If no links are present, returns <c>null</c> or an empty collection depending on the configuration.
    /// </returns>
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

                    url = content.Url(_publishedUrlProvider);
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
        if (dtos == null || dtos.Any() == false)
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

    private static bool IsSingleUrlPicker(IPublishedPropertyType propertyType)
        => propertyType.DataType.ConfigurationAs<MultiUrlPickerConfiguration>()!.MaxNumber == 1;

    private IEnumerable<MultiUrlPickerValueEditor.LinkDto>? ParseLinkDtos(string inter)
        => inter.DetectIsJson() ? _jsonSerializer.Deserialize<IEnumerable<MultiUrlPickerValueEditor.LinkDto>>(inter) : null;
}
