using System;
using System.Collections.Generic;

namespace Umbraco.ModelsBuilder.Building
{
    /// <summary>
    /// Represents a model property.
    /// </summary>
    public class PropertyModel
    {
        /// <summary>
        /// Gets the alias of the property.
        /// </summary>
        public string Alias;

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets the description of the property.
        /// </summary>
        public string Description;

        /// <summary>
        /// Gets the clr name of the property.
        /// </summary>
        /// <remarks>This is just the local name eg "Price".</remarks>
        public string ClrName;

        /// <summary>
        /// Gets the Model Clr type of the property values.
        /// </summary>
        /// <remarks>As indicated by the <c>PublishedPropertyType</c>, ie by the <c>IPropertyValueConverter</c>
        /// if any, else <c>object</c>. May include some ModelType that will need to be mapped.</remarks>
        public Type ModelClrType;

        /// <summary>
        /// Gets the CLR type name of the property values.
        /// </summary>
        public string ClrTypeName;

        /// <summary>
        /// Gets a value indicating whether this property should be excluded from generation.
        /// </summary>
        public bool IsIgnored;

        /// <summary>
        /// Gets the generation errors for the property.
        /// </summary>
        /// <remarks>This should be null, unless something prevents the property from being
        /// generated, and then the value should explain what. This can be used to generate
        /// commented out code eg in PureLive.</remarks>
        public List<string> Errors;

        /// <summary>
        /// Adds an error.
        /// </summary>
        public void AddError(string error)
        {
            if (Errors == null) Errors = new List<string>();
            Errors.Add(error);
        }
    }
}
