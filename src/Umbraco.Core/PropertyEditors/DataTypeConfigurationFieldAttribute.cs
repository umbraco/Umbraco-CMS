using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Marks a custom DataTypeConfigurationEditor property as a configuration field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DataTypeConfigurationFieldAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeConfigurationField"/> class.
        /// </summary>
        public DataTypeConfigurationFieldAttribute(Type preValueFieldType)
        {
            PreValueFieldType = preValueFieldType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeConfigurationField"/> class.
        /// </summary>
        public DataTypeConfigurationFieldAttribute(string key, string name, string view)
        {
            Key = key;
            Name = name;
            View = view;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeConfigurationField"/> class.
        /// </summary>
        public DataTypeConfigurationFieldAttribute(string name, string view)
        {
            Name = name;
            View = view;
        }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the description of the field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the label of the field.
        /// </summary>
        public bool HideLabel { get; set; }

        /// <summary>
        /// Gets or sets the key of the field.
        /// </summary>
        /// <remarks>Defaults to the field property name if not specified.</remarks>
        public string Key { get; }

        /// <summary>
        /// Gets or sets the view to used in the editor.
        /// </summary>
        public string View { get; }

        /// <summary>
        /// Gets or sets the Clr type of the <see cref="DataTypeConfigurationField" />. Properties
        /// from this type are used as defaults, unless explicitely specified in this attribute.
        /// </summary>
        public Type PreValueFieldType { get; set; }
    }
}
