using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The tags property value converter.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.PropertyEditors.PropertyValueConverterBase" />
[DefaultPropertyValueConverter]
public class TagsValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsValueConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <exception cref="System.ArgumentNullException">jsonSerializer</exception>
    public TagsValueConverter(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsValueConverter" /> class.
    /// </summary>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    [Obsolete("The IDataTypeService is not used anymore. This constructor will be removed in a future version.")]
    public TagsValueConverter(IDataTypeService dataTypeService, IJsonSerializer jsonSerializer)
        : this(jsonSerializer)
    { }

    /// <summary>
    /// Clears the data type configuration caches.
    /// </summary>
    [Obsolete("Caching of data type configuration is not done anymore. This method will be removed in a future version.")]
    public static void ClearCaches()
    { }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.Tags);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<string>);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        string? sourceString = source?.ToString();
        if (string.IsNullOrEmpty(sourceString))
        {
            return Array.Empty<string>();
        }

        return IsJson(propertyType)
            ? _jsonSerializer.Deserialize<string[]>(sourceString) ?? Array.Empty<string>()
            : sourceString.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool IsJson(IPublishedPropertyType propertyType)
        => propertyType.DataType.ConfigurationAs<TagConfiguration>()?.StorageType == TagsStorageType.Json;
}
