using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines the base for a Content object with properties that
    /// are shared between Content and Media.
    /// </summary>
    public interface IContentBase : IUmbracoEntity
    {
        /// <summary>
        /// Integer Id of the default ContentType
        /// </summary>
        int ContentTypeId { get; }

        /// <summary>
        /// Gets the Guid identifier of the content's version.
        /// </summary>
        Guid Version { get; }

        /// <summary>
        /// Gets the identifier of the writer.
        /// </summary>
        int WriterId { get; set; }

        /// <summary>
        /// List of properties, which make up all the data available for this Content object
        /// </summary>
        /// <remarks>Properties are loaded as part of the Content object graph</remarks>
        PropertyCollection Properties { get; set; }

        /// <summary>
        /// List of PropertyGroups available on this Content object
        /// </summary>
        /// <remarks>PropertyGroups are kind of lazy loaded as part of the object graph</remarks>
        IEnumerable<PropertyGroup> PropertyGroups { get; }

        /// <summary>
        /// List of PropertyTypes available on this Content object
        /// </summary>
        /// <remarks>PropertyTypes are kind of lazy loaded as part of the object graph</remarks>
        IEnumerable<PropertyType> PropertyTypes { get; }

        /// <summary>
        /// Gets a value indicating whether the content object has a property with the supplied alias.
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns>True if Property with given alias exists, otherwise False</returns>
        bool HasProperty(string propertyTypeAlias);

        /// <summary>
        /// Gets the neutral value of a Property
        /// </summary>
        object GetValue(string propertyTypeAlias, bool published = false);

        /// <summary>
        /// Gets the culture value of a Property
        /// </summary>
        object GetValue(string propertyTypeAlias, int languageId, bool published = false);

        /// <summary>
        /// Gets the segment value of a Property
        /// </summary>
        object GetValue(string propertyTypeAlias, int languageId, string segment, bool published = false);

        /// <summary>
        /// Gets the typed neutral value of a Property
        /// </summary>
        TPropertyValue GetValue<TPropertyValue>(string propertyTypeAlias, bool published = false);

        /// <summary>
        /// Gets the typed neutral value of a Property
        /// </summary>
        TPropertyValue GetValue<TPropertyValue>(string propertyTypeAlias, int languageId, bool published = false);

        /// <summary>
        /// Gets the typed neutral value of a Property
        /// </summary>
        TPropertyValue GetValue<TPropertyValue>(string propertyTypeAlias, int languageId, string segment, bool published = false);

        /// <summary>
        /// Sets the neutral (draft) value of a Property
        /// </summary>
        void SetValue(string propertyTypeAlias, object value);

        /// <summary>
        /// Sets the culture (draft) value of a Property
        /// </summary>
        void SetValue(string propertyTypeAlias, int languageId, object value);

        /// <summary>
        /// Sets the segment (draft) value of a Property
        /// </summary>
        void SetValue(string propertyTypeAlias, int languageId, string segment, object value);

        /// <summary>
        /// Boolean indicating whether the content and its properties are valid
        /// </summary>
        /// <returns>True if content is valid otherwise false</returns>
        bool Validate();
    }
}
