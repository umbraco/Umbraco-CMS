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

        public string Description { get; set; }
        public string Key { get; private set; }
        public string Name { get; private set; }
        public string View { get; private set; }
        public bool HideLabel { get; set; }

        /// <summary>
        /// This can be used when assigned to a property which will attempt to create the type
        /// of PreValueField declared and assign it to the fields. Any property declared on this
        /// attribute will get overwritten on the class that is instantiated.
        /// </summary>
        public Type PreValueFieldType { get; set; }
    }
}