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
        /// Gets all property types, across all property groups.
        /// </summary>
        IEnumerable<PropertyType> PropertyTypes { get; }

        /// <summary>
        /// Gets or sets the property types that are not in a group.
        /// </summary>
        IEnumerable<PropertyType> NoGroupPropertyTypes { get; set; }

        /// <summary>
        /// Removes a PropertyType from the current ContentType
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the <see cref="PropertyType"/> to remove</param>
        void RemovePropertyType(string propertyTypeAlias);

        /// <summary>
        /// Removes a PropertyGroup from the current ContentType
        /// </summary>
        /// <param name="propertyGroupName">Name of the <see cref="PropertyGroup"/> to remove</param>
        void RemovePropertyGroup(string propertyGroupName);

        /// <summary>
        /// Sets the ParentId from the lazy integer id
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        void SetLazyParentId(Lazy<int> id);

        /// <summary>
        /// Checks whether a PropertyType with a given alias already exists
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns>Returns <c>True</c> if a PropertyType with the passed in alias exists, otherwise <c>False</c></returns>
        bool PropertyTypeExists(string propertyTypeAlias);

        /// <summary>
        /// Adds a PropertyType to a specific PropertyGroup
        /// </summary>
        /// <param name="propertyType"><see cref="PropertyType"/> to add</param>
        /// <param name="propertyGroupName">Name of the PropertyGroup to add the PropertyType to</param>
        /// <returns>Returns <c>True</c> if PropertyType was added, otherwise <c>False</c></returns>
        bool AddPropertyType(PropertyType propertyType, string propertyGroupName);

        /// <summary>
        /// Adds a PropertyType, which does not belong to a PropertyGroup.
        /// </summary>
        /// <param name="propertyType"><see cref="PropertyType"/> to add</param>
        /// <returns>Returns <c>True</c> if PropertyType was added, otherwise <c>False</c></returns>
        bool AddPropertyType(PropertyType propertyType);

        /// <summary>
        /// Adds a PropertyGroup.
        /// This method will also check if a group already exists with the same name and link it to the parent.
        /// </summary>
        /// <param name="groupName">Name of the PropertyGroup to add</param>
        /// <returns>Returns <c>True</c> if a PropertyGroup with the passed in name was added, otherwise <c>False</c></returns>
        bool AddPropertyGroup(string groupName);

        /// <summary>
        /// Moves a PropertyType to a specified PropertyGroup
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType to move</param>
        /// <param name="propertyGroupName">Name of the PropertyGroup to move the PropertyType to</param>
        /// <returns></returns>
        bool MovePropertyType(string propertyTypeAlias, string propertyGroupName);
    }
}