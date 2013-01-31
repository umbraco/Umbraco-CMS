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
        /// Gets the Guid Id of the Content's Version
        /// </summary>
        Guid Version { get; }

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
        /// Indicates whether the content object has a property with the supplied alias
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns>True if Property with given alias exists, otherwise False</returns>
        bool HasProperty(string propertyTypeAlias);

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns><see cref="Property"/> Value as an <see cref="object"/></returns>
        object GetValue(string propertyTypeAlias);

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        /// <typeparam name="TPassType">Type of the value to return</typeparam>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns><see cref="Property"/> Value as a <see cref="TPassType"/></returns>
        TPassType GetValue<TPassType>(string propertyTypeAlias);

        /// <summary>
        /// Sets the <see cref="System.Object"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        void SetValue(string propertyTypeAlias, object value);

        /// <summary>
        /// Boolean indicating whether the content and its properties are valid
        /// </summary>
        /// <returns>True if content is valid otherwise false</returns>
        bool IsValid();

        /// <summary>
        /// Changes the Trashed state of the content object
        /// </summary>
        /// <param name="isTrashed">Boolean indicating whether content is trashed (true) or not trashed (false)</param>
        /// <param name="parentId"> </param>
        void ChangeTrashedState(bool isTrashed, int parentId = -20);
    }
}