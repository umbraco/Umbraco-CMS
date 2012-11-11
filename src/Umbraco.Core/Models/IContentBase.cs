using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines the base for a Content object with properties that
    /// are shared between Content and Media.
    /// </summary>
    public interface IContentBase : IAggregateRoot
    {
        /// <summary>
        /// Gets or Sets the Id of the Parent for the Content
        /// </summary>
        int ParentId { get; set; }

        /// <summary>
        /// Gets or Sets the Name of the Content
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or Sets the Sort Order of the Content
        /// </summary>
        int SortOrder { get; set; }

        /// <summary>
        /// Gets or Sets the Level of the Content
        /// </summary>
        int Level { get; set; }

        /// <summary>
        /// Gets or Sets the Path of the Content
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Id of the user who created the Content
        /// </summary>
        int CreatorId { get; set; }

        /// <summary>
        /// Boolean indicating whether this Content is Trashed or not.
        /// If Content is Trashed it will be located in the Recyclebin.
        /// </summary>
        bool Trashed { get; }
        
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
        /// Sets the value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        void SetValue(string propertyTypeAlias, object value);

        /// <summary>
        /// Boolean indicating whether the content and its properties are valid
        /// </summary>
        /// <returns>True if content is valid otherwise false</returns>
        bool IsValid();
    }
}