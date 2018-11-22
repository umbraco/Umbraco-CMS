using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Allows for specifying an attribute on a property of a custm PreValueEditor to be included in the field list. OTherwise it can be attributed 
    /// on a custom implemention of a PreValueField to have the properties auto-filled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class PreValueFieldAttribute : Attribute
    {
        /// <summary>
        /// Used when specifying a PreValueFieldType
        /// </summary>
        public PreValueFieldAttribute(Type preValueFieldType)
        {
            PreValueFieldType = preValueFieldType;
        }

        public PreValueFieldAttribute(string key, string name, string view)
        {
            Key = key;
            Name = name;
            View = view;
        }

        public PreValueFieldAttribute(string name, string view)
        {
            Name = name;
            View = view;
        }

        /// <summary>
        /// The description to display for the pre-value field
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The key to store the pre-value against in the databaes
        /// </summary>
        /// <remarks>
        /// If this is not specified and the attribute is being used at the property level then the property name will become the key
        /// </remarks>
        public string Key { get; private set; }

        /// <summary>
        /// The name (label) of the pre-value field
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The view to use to render the pre-value field
        /// </summary>
        public string View { get; private set; }

        /// <summary>
        /// Whether or not to hide the label for the pre-value field
        /// </summary>
        public bool HideLabel { get; set; }

        /// <summary>
        /// This can be used when assigned to a property which will attempt to create the type
        /// of PreValueField declared and assign it to the fields. Any property declared on this
        /// attribute will get overwritten on the class that is instantiated.
        /// </summary>
        public Type PreValueFieldType { get; set; }
    }
}