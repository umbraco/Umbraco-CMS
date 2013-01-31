using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines the base for a ContentType with properties that
    /// are shared between ContentTypes and MediaTypes.
    /// </summary>
    public interface IContentTypeBase : IUmbracoEntity
    {
        /// <summary>
        /// Gets or Sets the Alias of the ContentType
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// Gets or Sets the Description for the ContentType
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or Sets the Icon for the ContentType
        /// </summary>
        string Icon { get; set; }

        /// <summary>
        /// Gets or Sets the Thumbnail for the ContentType
        /// </summary>
        string Thumbnail { get; set; }

        /// <summary>
        /// Gets or Sets a boolean indicating whether this ContentType is allowed at the root
        /// </summary>
        bool AllowedAsRoot { get; set; }

        /// <summary>
        /// Gets or Sets a boolean indicating whether this ContentType is a Container
        /// </summary>
        /// <remarks>
        /// ContentType Containers doesn't show children in the tree, but rather in grid-type view.
        /// </remarks>
        bool IsContainer { get; set; }

        /// <summary>
        /// Gets or Sets a list of integer Ids of the ContentTypes allowed under the ContentType
        /// </summary>
        IEnumerable<ContentTypeSort> AllowedContentTypes { get; set; }

        /// <summary>
        /// Gets or Sets a collection of Property Groups
        /// </summary>
        PropertyGroupCollection PropertyGroups { get; set; }

        /// <summary>
        /// Gets an enumerable list of Property Types aggregated for all groups
        /// </summary>
        IEnumerable<PropertyType> PropertyTypes { get; }

        /// <summary>
        /// Removes a PropertyType from the current ContentType
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the <see cref="PropertyType"/> to remove</param>
        void RemovePropertyType(string propertyTypeAlias);

        /// <summary>
        /// Sets the ParentId from the lazy integer id
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        void SetLazyParentId(Lazy<int> id);
    }
}