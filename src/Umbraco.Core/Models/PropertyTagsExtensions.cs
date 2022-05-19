using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="Property" /> class to manage tags.
/// </summary>
public static class PropertyTagsExtensions
{
    // gets the tag configuration for a property
    // from the datatype configuration, and the editor tag configuration attribute
    public static TagConfiguration? GetTagConfiguration(this IProperty property, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        IDataEditor? editor = propertyEditors[property.PropertyType?.PropertyEditorAlias];
        TagsPropertyEditorAttribute? tagAttribute = editor?.GetTagAttribute();
        if (tagAttribute == null)
        {
            return null;
        }

        var configurationObject = property.PropertyType is null
            ? null
            : dataTypeService.GetDataType(property.PropertyType.DataTypeId)?.Configuration;
        TagConfiguration? configuration = ConfigurationEditor.ConfigurationAs<TagConfiguration>(configurationObject);

        if (configuration?.Delimiter == default && configuration?.Delimiter is not null)
        {
            configuration.Delimiter = tagAttribute.Delimiter;
        }

        return configuration;
    }

    /// <summary>
    ///     Assign tags.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="serializer"></param>
    /// <param name="tags">The tags.</param>
    /// <param name="merge">A value indicating whether to merge the tags with existing tags instead of replacing them.</param>
    /// <param name="culture">A culture, for multi-lingual properties.</param>
    /// <param name="propertyEditors"></param>
    /// <param name="dataTypeService"></param>
    public static void AssignTags(this IProperty property, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IJsonSerializer serializer, IEnumerable<string> tags, bool merge = false, string? culture = null)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        TagConfiguration? configuration = property.GetTagConfiguration(propertyEditors, dataTypeService);
        if (configuration == null)
        {
            throw new NotSupportedException($"Property with alias \"{property.Alias}\" does not support tags.");
        }

        property.AssignTags(tags, merge, configuration.StorageType, serializer, configuration.Delimiter, culture);
    }

    /// <summary>
    ///     Removes tags.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="serializer"></param>
    /// <param name="tags">The tags.</param>
    /// <param name="culture">A culture, for multi-lingual properties.</param>
    /// <param name="propertyEditors"></param>
    /// <param name="dataTypeService"></param>
    public static void RemoveTags(this IProperty property, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IJsonSerializer serializer, IEnumerable<string> tags, string? culture = null)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        TagConfiguration? configuration = property.GetTagConfiguration(propertyEditors, dataTypeService);
        if (configuration == null)
        {
            throw new NotSupportedException($"Property with alias \"{property.Alias}\" does not support tags.");
        }

        property.RemoveTags(tags, configuration.StorageType, serializer, configuration.Delimiter, culture);
    }

    // used by ContentRepositoryBase
    public static IEnumerable<string> GetTagsValue(this IProperty property, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IJsonSerializer serializer, string? culture = null)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        TagConfiguration? configuration = property.GetTagConfiguration(propertyEditors, dataTypeService);
        if (configuration == null)
        {
            throw new NotSupportedException($"Property with alias \"{property.Alias}\" does not support tags.");
        }

        return property.GetTagsValue(configuration.StorageType, serializer, configuration.Delimiter, culture);
    }

    /// <summary>
    ///     Sets tags on a content property, based on the property editor tags configuration.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="serializer"></param>
    /// <param name="value">The property value.</param>
    /// <param name="tagConfiguration">The datatype configuration.</param>
    /// <param name="culture">A culture, for multi-lingual properties.</param>
    /// <remarks>
    ///     <para>The value is either a string (delimited string) or an enumeration of strings (tag list).</para>
    ///     <para>
    ///         This is used both by the content repositories to initialize a property with some tag values, and by the
    ///         content controllers to update a property with values received from the property editor.
    ///     </para>
    /// </remarks>
    public static void SetTagsValue(this IProperty property, IJsonSerializer serializer, object? value, TagConfiguration? tagConfiguration, string? culture)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        if (tagConfiguration == null)
        {
            throw new ArgumentNullException(nameof(tagConfiguration));
        }

        TagsStorageType storageType = tagConfiguration.StorageType;
        var delimiter = tagConfiguration.Delimiter;

        SetTagsValue(property, value, storageType, serializer, delimiter, culture);
    }

    // assumes that parameters are consistent with the datatype configuration
    private static void AssignTags(this IProperty property, IEnumerable<string> tags, bool merge, TagsStorageType storageType, IJsonSerializer serializer, char delimiter, string? culture)
    {
        // set the property value
        var trimmedTags = tags.Select(x => x.Trim()).ToArray();

        if (merge)
        {
            IEnumerable<string> currentTags = property.GetTagsValue(storageType, serializer, delimiter);

            switch (storageType)
            {
                case TagsStorageType.Csv:
                    property.SetValue(
                        string.Join(delimiter.ToString(), currentTags.Union(trimmedTags)).NullOrWhiteSpaceAsNull(),
                        culture); // csv string
                    break;

                case TagsStorageType.Json:
                    var updatedTags = currentTags.Union(trimmedTags).ToArray();
                    var updatedValue = updatedTags.Length == 0 ? null : serializer.Serialize(updatedTags);
                    property.SetValue(updatedValue, culture); // json array
                    break;
            }
        }
        else
        {
            switch (storageType)
            {
                case TagsStorageType.Csv:
                    property.SetValue(
                        string.Join(delimiter.ToString(), trimmedTags).NullOrWhiteSpaceAsNull(),
                        culture); // csv string
                    break;

                case TagsStorageType.Json:
                    var updatedValue = trimmedTags.Length == 0 ? null : serializer.Serialize(trimmedTags);
                    property.SetValue(updatedValue, culture); // json array
                    break;
            }
        }
    }

    // assumes that parameters are consistent with the datatype configuration
    private static void RemoveTags(this IProperty property, IEnumerable<string> tags, TagsStorageType storageType, IJsonSerializer serializer, char delimiter, string? culture)
    {
        // already empty = nothing to do
        var value = property.GetValue(culture)?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        // set the property value
        var trimmedTags = tags.Select(x => x.Trim()).ToArray();
        IEnumerable<string> currentTags = property.GetTagsValue(storageType, serializer, delimiter, culture);
        switch (storageType)
        {
            case TagsStorageType.Csv:
                property.SetValue(
                    string.Join(delimiter.ToString(), currentTags.Except(trimmedTags)).NullOrWhiteSpaceAsNull(),
                    culture); // csv string
                break;

            case TagsStorageType.Json:
                var updatedTags = currentTags.Except(trimmedTags).ToArray();
                var updatedValue = updatedTags.Length == 0 ? null : serializer.Serialize(updatedTags);
                property.SetValue(updatedValue, culture); // json array
                break;
        }
    }

    private static IEnumerable<string> GetTagsValue(this IProperty property, TagsStorageType storageType, IJsonSerializer serializer, char delimiter, string? culture = null)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        var value = property.GetValue(culture)?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Enumerable.Empty<string>();
        }

        switch (storageType)
        {
            case TagsStorageType.Csv:
                return value.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

            case TagsStorageType.Json:
                try
                {
                    return serializer.Deserialize<string[]>(value)?.Select(x => x.Trim()) ?? Enumerable.Empty<string>();
                }
                catch (Exception)
                {
                    // cannot parse, malformed
                    return Enumerable.Empty<string>();
                }

            default:
                throw new NotSupportedException($"Value \"{storageType}\" is not a valid TagsStorageType.");
        }
    }

    // assumes that parameters are consistent with the datatype configuration
    // value can be an enumeration of string, or a serialized value using storageType format
    private static void SetTagsValue(IProperty property, object? value, TagsStorageType storageType, IJsonSerializer serializer, char delimiter, string? culture)
    {
        if (value == null)
        {
            value = Enumerable.Empty<string>();
        }

        // if value is already an enumeration of strings, just use it
        if (value is IEnumerable<string> tags1)
        {
            property.AssignTags(tags1, false, storageType, serializer, delimiter, culture);
            return;
        }

        // otherwise, deserialize value based upon storage type
        switch (storageType)
        {
            case TagsStorageType.Csv:
                var tags2 = value.ToString()!.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                property.AssignTags(tags2, false, storageType, serializer, delimiter, culture);
                break;

            case TagsStorageType.Json:
                try
                {
                    IEnumerable<string>? tags3 = serializer.Deserialize<IEnumerable<string>>(value.ToString()!);
                    property.AssignTags(tags3 ?? Enumerable.Empty<string>(), false, storageType, serializer, delimiter, culture);
                }
                catch (Exception ex)
                {
                    StaticApplicationLogging.Logger.LogWarning(
                        ex,
                        "Could not automatically convert stored json value to an enumerable string '{Json}'",
                        value.ToString());
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(storageType));
        }
    }
}
