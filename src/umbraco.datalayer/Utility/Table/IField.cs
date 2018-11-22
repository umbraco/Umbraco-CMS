using System;

namespace umbraco.DataLayer.Utility.Table
{
    /// <summary>
    /// Interface for classes that represent a table field.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public interface IField
    {
        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the data type of the field.
        /// </summary>
        /// <value>The field data type.</value>
        Type DataType { get; }

        /// <summary>
        /// Gets a value indicating the field size.
        /// </summary>
        /// <value>The size.</value>
        int Size { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties.</value>
        FieldProperties Properties { get; }

        /// <summary>
        /// Determines whether the field has the specified property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if the field has the property; otherwise, <c>false</c>.</returns>
        bool HasProperty(FieldProperties property);
    }
}
