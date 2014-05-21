using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// An interface that indicates that a property editor supports tags and will store it's published tags into the tag db table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SupportTagsAttribute : Attribute
    {
        public Type TagPropertyDefinitionType { get; private set; }

        /// <summary>
        /// Defines a tag property definition type to invoke at runtime to get the tags configuration for a property
        /// </summary>
        /// <param name="tagPropertyDefinitionType"></param>
        public SupportTagsAttribute(Type tagPropertyDefinitionType)
            : this()
        {
            if (tagPropertyDefinitionType == null) throw new ArgumentNullException("tagPropertyDefinitionType");
            TagPropertyDefinitionType = tagPropertyDefinitionType;
        }

        /// <summary>
        /// Normal constructor specifying the default tags configuration for a property
        /// </summary>
        public SupportTagsAttribute()
        {
            ValueType = TagValueType.FromDelimitedValue;
            Delimiter = ",";
            ReplaceTags = true;
            TagGroup = "default";
            StorageType = TagCacheStorageType.Csv;
        }

        /// <summary>
        /// Defines how the tag values will be extracted, default is FromDelimitedValue
        /// </summary>
        public TagValueType ValueType { get; set; }

        /// <summary>
        /// Defines how to store the tags in cache (CSV or Json)
        /// </summary>
        public TagCacheStorageType StorageType { get; set; }

        /// <summary>
        /// Defines a custom delimiter, the default is a comma
        /// </summary>
        public string Delimiter { get; set; }

        /// <summary>
        /// Determines whether or not to replace the tags with the new value or append them (true to replace, false to append), default is true
        /// </summary>
        public bool ReplaceTags { get; set; }

        /// <summary>
        /// The tag group to use when tagging, the default is 'default'
        /// </summary>
        public string TagGroup { get; set; }
    }
}