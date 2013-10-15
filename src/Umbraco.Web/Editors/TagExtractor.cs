using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
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
        /// <param name="propertyValue"></param>
        /// <param name="attribute"></param>
        public static void SetPropertyTags(IContentBase content, Property property, object propertyValue, SupportTagsAttribute attribute)
        {
            switch (attribute.ValueType)
            {
                case TagValueType.FromDelimitedValue:
                    var tags = propertyValue.ToString().Split(new[] { attribute.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                    content.SetTags(property.Alias, tags, attribute.ReplaceTags, attribute.TagGroup);
                    break;
                case TagValueType.CustomTagList:
                    //for this to work the object value must be IENumerable<string>
                    var stringList = propertyValue as IEnumerable<string>;
                    if (stringList != null)
                    {
                        content.SetTags(property.Alias, stringList, attribute.ReplaceTags, attribute.TagGroup);
                    }
                    break;
            }
        }

    }
}