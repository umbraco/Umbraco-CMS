using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// A simple utility class to extract the tag values from a property/property editor and set them on the content
    /// </summary>
    internal class TagExtractor
    {
        public static SupportTagsAttribute GetAttribute(PropertyEditor propEd)
        {
            var tagSupport = propEd.GetType().GetCustomAttribute<SupportTagsAttribute>(false);
            return tagSupport;
        }

        /// <summary>
        /// Sets the tag values on the content property based on the property editor's tags attribute
        /// </summary>
        /// <param name="content"></param>
        /// <param name="property"></param>
        /// <param name="propertyData"></param>
        /// <param name="convertedPropertyValue"></param>
        /// <param name="attribute"></param>
        public static void SetPropertyTags(IContentBase content, Property property, ContentPropertyData propertyData, object convertedPropertyValue, SupportTagsAttribute attribute)
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
                SetPropertyTags(content, property, convertedPropertyValue, def.Delimiter, def.ReplaceTags, def.TagGroup, attribute.ValueType);
            }
            else
            {
                SetPropertyTags(content, property, convertedPropertyValue, attribute.Delimiter, attribute.ReplaceTags, attribute.TagGroup, attribute.ValueType);
            }
        }

        public static void SetPropertyTags(IContentBase content, Property property, object convertedPropertyValue, string delimiter, bool replaceTags, string tagGroup, TagValueType valueType)
        {
            if (convertedPropertyValue == null)
            {
                convertedPropertyValue = "";
            }

            switch (valueType)
            {
                case TagValueType.FromDelimitedValue:
                    var tags = convertedPropertyValue.ToString().Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                    content.SetTags(property.Alias, tags, replaceTags, tagGroup);
                    break;
                case TagValueType.CustomTagList:
                    //for this to work the object value must be IENumerable<string>
                    var stringList = convertedPropertyValue as IEnumerable<string>;
                    if (stringList != null)
                    {
                        content.SetTags(property.Alias, stringList, replaceTags, tagGroup);
                    }
                    break;
            }
        }

    }
}