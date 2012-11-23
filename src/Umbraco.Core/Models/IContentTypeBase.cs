using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines the base for a ContentType with properties that
    /// are shared between ContentTypes and MediaTypes.
    /// </summary>
    public interface IContentTypeBase : IAggregateRoot
    {
        /// <summary>
        /// Gets or Sets the Alias of the ContentType
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// Gets or Sets the Name of the ContentType
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or Sets the Id of Parent of the ContentType
        /// </summary>
        int ParentId { get; set; }

        /// <summary>
        /// Gets or Sets the Level of the Content
        /// </summary>
        int Level { get; set; }

        /// <summary>
        /// Gets or Sets the Path of the Content
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Gets or Sets the Description for the ContentType
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or Sets the Sort Order of the ContentType
        /// </summary>
        int SortOrder { get; set; }

        /// <summary>
        /// Gets or Sets the Icon for the ContentType
        /// </summary>
        string Icon { get; set; }

        /// <summary>
        /// Gets or Sets the Thumbnail for the ContentType
        /// </summary>
        string Thumbnail { get; set; }

        /// <summary>
        /// Gets or Sets the Id of the User who created the ContentType
        /// </summary>
        int CreatorId { get; set; }

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

        void SetLazyParentId(Lazy<int> id);
    }
}