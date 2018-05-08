﻿using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Provides a base class for content items.
    /// </summary>
    /// <remarks>
    /// <para>Content items are documents, medias and members.</para>
    /// <para>Content items have a content type, and properties.</para>
    /// </remarks>
    public interface IContentBase : IUmbracoEntity
    {
        /// <summary>
        /// Integer Id of the default ContentType
        /// </summary>
        int ContentTypeId { get; }

        /// <summary>
        /// Gets the identifier of the writer.
        /// </summary>
        int WriterId { get; set; }

        /// <summary>
        /// Gets the version identifier.
        /// </summary>
        int VersionId { get; }

        /// <summary>
        /// Sets the name of the content item for a specified language.
        /// </summary>
        /// <remarks>
        /// <para>When <paramref name="culture"/> is <c>null</c>, sets the invariant
        /// language, which sets the <see cref="TreeEntityBase.Name"/> property.</para>
        /// </remarks>
        void SetName(string value, string culture);

        /// <summary>
        /// Gets the name of the content item for a specified language.
        /// </summary>
        /// <remarks>
        /// <para>When <paramref name="culture"/> is <c>null</c>, gets the invariant
        /// language, which is the value of the <see cref="TreeEntityBase.Name"/> property.</para>
        /// </remarks>
        string GetName(string culture);

        /// <summary>
        /// Gets the names of the content item.
        /// </summary>
        /// <remarks>
        /// <para>Because a dictionary key cannot be <c>null</c> this cannot get the invariant
        /// name, which must be get or set via the <see cref="TreeEntityBase.Name"/> property.</para>
        /// </remarks>
        IReadOnlyDictionary<string, string> Names { get; }

        /// <summary>
        /// Gets a value indicating whether a given culture is available.
        /// </summary>
        /// <remarks>
        /// <para>A culture becomes available whenever the content name for this culture is
        /// non-null, and it becomes unavailable whenever the content name is null.</para>
        /// </remarks>
        bool IsCultureAvailable(string culture);

        /// <summary>
        /// Gets the date a culture was created.
        /// </summary>
        DateTime GetCultureDate(string culture);

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
        /// Gets a value indicating whether the content entity has a property with the supplied alias.
        /// </summary>
        /// <remarks>Indicates that the content entity has a property with the supplied alias, but
        /// not necessarily that the content has a value for that property. Could be missing.</remarks>
        bool HasProperty(string propertyTypeAlias);

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        object GetValue(string propertyTypeAlias, string culture = null, string segment = null, bool published = false);

        /// <summary>
        /// Gets the typed value of a Property
        /// </summary>
        TValue GetValue<TValue>(string propertyTypeAlias, string culture = null, string segment = null, bool published = false);

        /// <summary>
        /// Sets the (edited) value of a Property
        /// </summary>
        void SetValue(string propertyTypeAlias, object value, string culture = null, string segment = null);

        /// <summary>
        /// Gets a value indicating whether the content and all its properties values are valid.
        /// </summary>
        Property[] ValidateAll();

        /// <summary>
        /// Gets a value indicating whether the content and its properties values are valid.
        /// </summary>
        Property[] Validate(string culture = null, string segment = null);

        /// <summary>
        /// Gets a value indicating whether the content and its culture/any properties values are valid.
        /// </summary>
        Property[] ValidateCulture(string culture = null);
    }
}
