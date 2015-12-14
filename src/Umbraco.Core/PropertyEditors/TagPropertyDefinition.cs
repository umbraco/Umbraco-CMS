using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Allows for dynamically changing how a property's data is tagged at runtime during property setting
    /// </summary>
    public abstract class TagPropertyDefinition
    {
        /// <summary>
        /// The property data that will create the tag data
        /// </summary>
        public ContentPropertyData PropertySaving { get; private set; }

        /// <summary>
        /// The attribute that has specified this definition type
        /// </summary>
        public SupportTagsAttribute TagsAttribute { get; set; }

        /// <summary>
        /// Constructor specifies the defaults and sets the ContentPropertyData being used to set the tag values which
        /// can be used to dynamically adjust the tags definition for this property.
        /// </summary>
        /// <param name="propertySaving"></param>
        /// <param name="tagsAttribute"></param>
        protected TagPropertyDefinition(ContentPropertyData propertySaving, SupportTagsAttribute tagsAttribute)
        {
            PropertySaving = propertySaving;
            TagsAttribute = tagsAttribute;
            Delimiter = tagsAttribute.Delimiter;
            ReplaceTags = tagsAttribute.ReplaceTags;
            TagGroup = tagsAttribute.TagGroup;

            var preValues = propertySaving.PreValues.PreValuesAsDictionary;
            StorageType =  preValues.ContainsKey("storageType") && preValues["storageType"].Value == TagCacheStorageType.Json.ToString() ? 
                TagCacheStorageType.Json : TagCacheStorageType.Csv;
        }

        /// <summary>
        /// Defines how to store the tags in cache (CSV or Json)
        /// </summary>
        public virtual TagCacheStorageType StorageType { get; private set; }

        /// <summary>
        /// Defines a custom delimiter, the default is a comma
        /// </summary>
        public virtual string Delimiter { get; private set; }

        /// <summary>
        /// Determines whether or not to replace the tags with the new value or append them (true to replace, false to append), default is true
        /// </summary>
        public virtual bool ReplaceTags { get; private set; }

        /// <summary>
        /// The tag group to use when tagging, the default is 'default'
        /// </summary>
        public virtual string TagGroup { get; private set; }
    }

}