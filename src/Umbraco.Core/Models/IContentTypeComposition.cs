using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines the Composition of a ContentType
    /// </summary>
    public interface IContentTypeComposition : IContentTypeBase
    {
        /// <summary>
        /// Gets or sets the content types that compose this content type.
        /// </summary>
        IEnumerable<IContentTypeComposition> ContentTypeComposition { get; set;  }

        /// <summary>
        /// Gets the property groups for the entire composition.
        /// </summary>
        IEnumerable<PropertyGroup> CompositionPropertyGroups { get; }

        /// <summary>
        /// Gets the property types for the entire composition.
        /// </summary>
        IEnumerable<PropertyType> CompositionPropertyTypes { get; }

        /// <summary>
        /// Adds a new ContentType to the list of composite ContentTypes
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to add</param>
        /// <returns>True if ContentType was added, otherwise returns False</returns>
        bool AddContentType(IContentTypeComposition contentType);

        /// <summary>
        /// Removes a ContentType with the supplied alias from the the list of composite ContentTypes
        /// </summary>
        /// <param name="alias">Alias of a <see cref="IContentType"/></param>
        /// <returns>True if ContentType was removed, otherwise returns False</returns>
        bool RemoveContentType(string alias);

        /// <summary>
        /// Checks if a ContentType with the supplied alias exists in the list of composite ContentTypes
        /// </summary>
        /// <param name="alias">Alias of a <see cref="IContentType"/></param>
        /// <returns>True if ContentType with alias exists, otherwise returns False</returns>
        bool ContentTypeCompositionExists(string alias);

        /// <summary>
        /// Gets a list of ContentType aliases from the current composition 
        /// </summary>
        /// <returns>An enumerable list of string aliases</returns>
        IEnumerable<string> CompositionAliases();

        /// <summary>
        /// Gets a list of ContentType Ids from the current composition 
        /// </summary>
        /// <returns>An enumerable list of integer ids</returns>
        IEnumerable<int> CompositionIds();
    }
}