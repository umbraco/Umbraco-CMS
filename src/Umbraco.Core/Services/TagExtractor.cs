using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// A utility class to extract the tag values from a property/property editor and set them on the content
    /// </summary>
    public class TagExtractor
    {
        /// <summary>
        /// Will return the <see cref="SupportTagsAttribute"/> for the given <see cref="PropertyEditor"/>
        /// </summary>
        /// <param name="propEd"></param>
        /// <returns></returns>
        public static SupportTagsAttribute GetAttribute(PropertyEditor propEd)
        {
            if (propEd == null) return null;
            var tagSupport = propEd.GetType().GetCustomAttribute<SupportTagsAttribute>(false);
            return tagSupport;
        }

        /// <summary>
        /// Sets the tag values on the content property based on the property editor's tags attribute
        /// </summary>
        /// <param name="property">The content's Property</param>
        /// <param name="propertyData">The data that has been submitted to be saved for a content property</param>
        /// <param name="convertedPropertyValue">
        /// If the <see cref="TagValueType"/> is <see cref="TagValueType.FromDelimitedValue"/> then this is expected to be a delimited string, 
        /// otherwise if it is <see cref="TagValueType.CustomTagList"/> then this is expected to be IEnumerable{string}
        /// </param>
        /// <param name="attribute"></param>
        public static void SetPropertyTags(Property property, ContentPropertyData propertyData, object convertedPropertyValue, SupportTagsAttribute attribute)
        {
            //check for a custom definition
            if (attribute.TagPropertyDefinitionType != null)
            {
                //try to create it
                TagPropertyDefinition def;
                try
                {
                    def = (TagPropertyDefinition) Activator.CreateInstance(attribute.TagPropertyDefinitionType, propertyData, attribute);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<TagExtractor>("Could not create custom " + attribute.TagPropertyDefinitionType + " tag definition", ex);
                    throw;
                }
                SetPropertyTags(property, convertedPropertyValue, def.Delimiter, def.ReplaceTags, def.TagGroup, attribute.ValueType, def.StorageType);
            }
            else
            {
                SetPropertyTags(property, convertedPropertyValue, attribute.Delimiter, attribute.ReplaceTags, attribute.TagGroup, attribute.ValueType, attribute.StorageType);
            }
        }

        /// <summary>
        /// Sets the tag values on the content property based on the property editor's tags attribute
        /// </summary>
        /// <param name="property">The content's Property</param>
        /// <param name="convertedPropertyValue">
        /// If the <see cref="TagValueType"/> is <see cref="TagValueType.FromDelimitedValue"/> then this is expected to be a delimited string, 
        /// otherwise if it is <see cref="TagValueType.CustomTagList"/> then this is expected to be IEnumerable{string}
        /// </param>
        /// <param name="delimiter">The delimiter for the value if convertedPropertyValue is a string delimited value</param>
        /// <param name="replaceTags">Whether or not to replace the tags with the new value or append them (true to replace, false to append)</param>
        /// <param name="tagGroup">The tag group to use when tagging</param>
        /// <param name="valueType">Defines how the tag values will be extracted</param>
        /// <param name="storageType">Defines how to store the tags in cache (CSV or Json)</param>
        internal static void SetPropertyTags(Property property, object convertedPropertyValue, string delimiter, bool replaceTags, string tagGroup, TagValueType valueType, TagCacheStorageType storageType)
        {
            if (convertedPropertyValue == null)
            {
                convertedPropertyValue = "";
            }

            switch (valueType)
            {
                case TagValueType.FromDelimitedValue:
                    var tags = convertedPropertyValue.ToString().Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                    property.SetTags(storageType, property.Alias, tags, replaceTags, tagGroup);
                    break;
                case TagValueType.CustomTagList:
                    //for this to work the object value must be IEnumerable<string>
                    var stringList = convertedPropertyValue as IEnumerable<string>;
                    if (stringList != null)
                    {
                        property.SetTags(storageType, property.Alias, stringList, replaceTags, tagGroup);
                    }
                    else
                    {
                        //it's not enumerable string, so lets see if we can automatically make it that way based on the current storage type
                        switch (storageType)
                        {
                            case TagCacheStorageType.Csv:
                                var split = convertedPropertyValue.ToString().Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                                //recurse with new value
                                SetPropertyTags(property, split, delimiter, replaceTags, tagGroup, valueType, storageType);
                                break;
                            case TagCacheStorageType.Json:
                                try
                                {
                                    var parsedJson = JsonConvert.DeserializeObject<IEnumerable<string>>(convertedPropertyValue.ToString());
                                    if (parsedJson != null)
                                    {
                                        //recurse with new value
                                        SetPropertyTags(property, parsedJson, delimiter, replaceTags, tagGroup, valueType, storageType);   
                                    }                                    
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.WarnWithException<TagExtractor>("Could not automatically convert stored json value to an enumerable string", ex);
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("storageType");
                        }
                    }
                    break;
            }
        }

    }
}