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
    /// A simple utility class to extract the tag values from a property/property editor and set them on the content
    /// </summary>
    internal class TagExtractor
    {
        public static SupportTagsAttribute GetAttribute(PropertyEditor propEd)
        {
            if (propEd == null) return null;
            var tagSupport = propEd.GetType().GetCustomAttribute<SupportTagsAttribute>(false);
            return tagSupport;
        }

        /// <summary>
        /// Sets the tag values on the content property based on the property editor's tags attribute
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyData"></param>
        /// <param name="convertedPropertyValue"></param>
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

        public static void SetPropertyTags(Property property, object convertedPropertyValue, string delimiter, bool replaceTags, string tagGroup, TagValueType valueType, TagCacheStorageType storageType)
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
                    //for this to work the object value must be IENumerable<string>
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