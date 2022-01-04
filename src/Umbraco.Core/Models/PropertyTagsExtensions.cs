using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Provides extension methods for the <see cref="Property"/> class to manage tags.
    /// </summary>
    public static class PropertyTagsExtensions
    {
        // TODO: inject
        private static PropertyEditorCollection PropertyEditors => Current.PropertyEditors;
        private static IDataTypeService DataTypeService => Current.Services.DataTypeService;

        // gets the tag configuration for a property
        // from the datatype configuration, and the editor tag configuration attribute
        internal static TagConfiguration GetTagConfiguration(this Property property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var editor = PropertyEditors[property.PropertyType.PropertyEditorAlias];
            var tagAttribute = editor.GetTagAttribute();
            if (tagAttribute == null) return null;

            var configurationObject = DataTypeService.GetDataType(property.PropertyType.DataTypeId).Configuration;
            var configuration = ConfigurationEditor.ConfigurationAs<TagConfiguration>(configurationObject);

            if (configuration.Delimiter == default)
                configuration.Delimiter = tagAttribute.Delimiter;

            return configuration;
        }

        /// <summary>
        /// Assign tags.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="merge">A value indicating whether to merge the tags with existing tags instead of replacing them.</param>
        /// <param name="culture">A culture, for multi-lingual properties.</param>
        public static void AssignTags(this Property property, IEnumerable<string> tags, bool merge = false, string culture = null)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var configuration = property.GetTagConfiguration();
            if (configuration == null)
                throw new NotSupportedException($"Property with alias \"{property.Alias}\" does not support tags.");

            property.AssignTags(tags, merge, configuration.StorageType, configuration.Delimiter, culture);
        }

        // assumes that parameters are consistent with the datatype configuration
        private static void AssignTags(this Property property, IEnumerable<string> tags, bool merge, TagsStorageType storageType, char delimiter, string culture)
        {
            // set the property value
            var trimmedTags = tags.Select(x => x.Trim()).ToArray();

            if (merge)
            {
                var currentTags = property.GetTagsValue(storageType, delimiter);

                switch (storageType)
                {
                    case TagsStorageType.Csv:
                        property.SetValue(string.Join(delimiter.ToString(), currentTags.Union(trimmedTags)).NullOrWhiteSpaceAsNull(), culture); // csv string
                        break;

                    case TagsStorageType.Json:
                        var updatedTags = currentTags.Union(trimmedTags).ToArray();
                        var updatedValue = updatedTags.Length == 0 ? null : JsonConvert.SerializeObject(updatedTags, Formatting.None);
                        property.SetValue(updatedValue, culture); // json array
                        break;
                }
            }
            else
            {
                switch (storageType)
                {
                    case TagsStorageType.Csv:
                        property.SetValue(string.Join(delimiter.ToString(), trimmedTags).NullOrWhiteSpaceAsNull(), culture); // csv string
                        break;

                    case TagsStorageType.Json:
                        var updatedValue = trimmedTags.Length == 0 ? null : JsonConvert.SerializeObject(trimmedTags, Formatting.None);
                        property.SetValue(updatedValue, culture); // json array
                        break;
                }
            }
        }

        /// <summary>
        /// Removes tags.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="culture">A culture, for multi-lingual properties.</param>
        public static void RemoveTags(this Property property, IEnumerable<string> tags, string culture = null)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var configuration = property.GetTagConfiguration();
            if (configuration == null)
                throw new NotSupportedException($"Property with alias \"{property.Alias}\" does not support tags.");

            property.RemoveTags(tags, configuration.StorageType, configuration.Delimiter, culture);
        }

        // assumes that parameters are consistent with the datatype configuration
        private static void RemoveTags(this Property property, IEnumerable<string> tags, TagsStorageType storageType, char delimiter, string culture)
        {
            // already empty = nothing to do
            var value = property.GetValue(culture)?.ToString();
            if (string.IsNullOrWhiteSpace(value)) return;

            // set the property value
            var trimmedTags = tags.Select(x => x.Trim()).ToArray();
            var currentTags = property.GetTagsValue(storageType, delimiter, culture);
            switch (storageType)
            {
                case TagsStorageType.Csv:
                    property.SetValue(string.Join(delimiter.ToString(), currentTags.Except(trimmedTags)).NullOrWhiteSpaceAsNull(), culture); // csv string
                    break;

                case TagsStorageType.Json:
                    var updatedTags = currentTags.Except(trimmedTags).ToArray();
                    var updatedValue = updatedTags.Length == 0 ? null : JsonConvert.SerializeObject(updatedTags, Formatting.None);
                    property.SetValue(updatedValue, culture); // json array
                    break;
            }
        }

        // used by ContentRepositoryBase
        internal static IEnumerable<string> GetTagsValue(this Property property, string culture = null)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var configuration = property.GetTagConfiguration();
            if (configuration == null)
                throw new NotSupportedException($"Property with alias \"{property.Alias}\" does not support tags.");

            return property.GetTagsValue(configuration.StorageType, configuration.Delimiter, culture);
        }

        private static IEnumerable<string> GetTagsValue(this Property property, TagsStorageType storageType, char delimiter, string culture = null)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var value = property.GetValue(culture)?.ToString();
            if (string.IsNullOrWhiteSpace(value)) return Enumerable.Empty<string>();

            switch (storageType)
            {
                case TagsStorageType.Csv:
                    return value.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

                case TagsStorageType.Json:
                    try
                    {
                        return JsonConvert.DeserializeObject<string[]>(value).Select(x => x.Trim());
                    }
                    catch (JsonException)
                    {
                        //cannot parse, malformed
                        return Enumerable.Empty<string>();
                    }

                default:
                    throw new NotSupportedException($"Value \"{storageType}\" is not a valid TagsStorageType.");
            }
        }

        /// <summary>
        /// Sets tags on a content property, based on the property editor tags configuration.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The property value.</param>
        /// <param name="tagConfiguration">The datatype configuration.</param>
        /// <param name="culture">A culture, for multi-lingual properties.</param>
        /// <remarks>
        /// <para>The value is either a string (delimited string) or an enumeration of strings (tag list).</para>
        /// <para>This is used both by the content repositories to initialize a property with some tag values, and by the
        /// content controllers to update a property with values received from the property editor.</para>
        /// </remarks>
        internal static void SetTagsValue(this Property property, object value, TagConfiguration tagConfiguration, string culture)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (tagConfiguration == null) throw new ArgumentNullException(nameof(tagConfiguration));

            var storageType = tagConfiguration.StorageType;
            var delimiter = tagConfiguration.Delimiter;

            SetTagsValue(property, value, storageType, delimiter, culture);
        }

        // assumes that parameters are consistent with the datatype configuration
        // value can be an enumeration of string, or a serialized value using storageType format
        private static void SetTagsValue(Property property, object value, TagsStorageType storageType, char delimiter, string culture)
        {
            if (value == null) value = Enumerable.Empty<string>();

            // if value is already an enumeration of strings, just use it
            if (value is IEnumerable<string> tags1)
            {
                property.AssignTags(tags1, false, storageType, delimiter, culture);
                return;
            }

            // otherwise, deserialize value based upon storage type
            switch (storageType)
            {
                case TagsStorageType.Csv:
                    var tags2 = value.ToString().Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                    property.AssignTags(tags2, false, storageType, delimiter, culture);
                    break;

                case TagsStorageType.Json:
                    try
                    {
                        var tags3 = JsonConvert.DeserializeObject<IEnumerable<string>>(value.ToString());
                        property.AssignTags(tags3 ?? Enumerable.Empty<string>(), false, storageType, delimiter, culture);
                    }
                    catch (Exception ex)
                    {
                        Current.Logger.Warn(typeof(PropertyTagsExtensions), ex, "Could not automatically convert stored json value to an enumerable string '{Json}'", value.ToString());
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(storageType));
            }
        }
    }
}
