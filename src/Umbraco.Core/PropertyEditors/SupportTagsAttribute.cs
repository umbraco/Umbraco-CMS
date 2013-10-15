using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// An interface that indicates that a property editor supports tags and will store it's published tags into the tag db table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SupportTagsAttribute : Attribute
    {
        //TODO: We should be able to add an overload to this to provide a 'tag definition' so developers can dynamically change 
        // things like TagGroup and ReplaceTags at runtime.

        public SupportTagsAttribute()
        {
            ValueType = TagValueType.FromDelimitedValue;
            Delimiter = ",";
            ReplaceTags = true;
            TagGroup = "default";
        }

        /// <summary>
        /// Defines how the tag values will be extracted, default is FromDelimitedValue
        /// </summary>
        public TagValueType ValueType { get; set; }

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