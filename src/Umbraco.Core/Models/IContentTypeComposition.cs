namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines the Composition of a ContentType
/// </summary>
public interface IContentTypeComposition : IContentTypeBase
{
    /// <summary>
    ///     Gets or sets the content types that compose this content type.
    /// </summary>
    // TODO: we should be storing key references, not the object else we are caching way too much
    IEnumerable<IContentTypeComposition> ContentTypeComposition { get; set; }

    /// <summary>
    ///     Gets the property groups for the entire composition.
    /// </summary>
    IEnumerable<PropertyGroup> CompositionPropertyGroups { get; }

    /// <summary>
    ///     Gets the property types for the entire composition.
    /// </summary>
    IEnumerable<IPropertyType> CompositionPropertyTypes { get; }

    /// <summary>
    ///     Returns a list of content type ids that have been removed from this instance's composition
    /// </summary>
    IEnumerable<int> RemovedContentTypes { get; }

    /// <summary>
    ///     Adds a new ContentType to the list of composite ContentTypes
    /// </summary>
    /// <param name="contentType"><see cref="IContentType" /> to add</param>
    /// <returns>True if ContentType was added, otherwise returns False</returns>
    // TODO (V18): Update nullability so only a non-null contentType can be passed.
    bool AddContentType(IContentTypeComposition? contentType);

    /// <summary>
    ///     Adds a new ContentType to the list of composite ContentTypes
    /// </summary>
    /// <param name="contentType"><see cref="IContentType" /> to add</param>
    /// <param name="removedPropertyTypeAliases">
    /// Aliases of property types that are being removed from the current content type in this update operation.
    /// The composition is allowed to have properties with these aliases since they won't conflict.
    /// </param>
    /// <returns>True if ContentType was added, otherwise returns False</returns>
    /// <remarks>
    ///     The default implementation of this overload ignores <paramref name="removedPropertyTypeAliases" />
    ///     and simply calls <see cref="AddContentType(IContentTypeComposition?)" />. Implementations that need to
    ///     respect removed property type aliases must override this method.
    /// </remarks>
    // TODO (V18): Remove the default implementation.
    bool AddContentType(IContentTypeComposition contentType, string[] removedPropertyTypeAliases) => AddContentType(contentType);

    /// <summary>
    ///     Removes a ContentType with the supplied alias from the list of composite ContentTypes
    /// </summary>
    /// <param name="alias">Alias of a <see cref="IContentType" /></param>
    /// <returns>True if ContentType was removed, otherwise returns False</returns>
    bool RemoveContentType(string alias);

    /// <summary>
    /// Removes a content type with a specified key from the composition.
    /// </summary>
    /// <param name="key">The key of the content type to remove.</param>
    /// <returns>True if the content type was removed, otherwise false.</returns>
    bool RemoveContentType(Guid key);

    /// <summary>
    ///     Checks if a ContentType with the supplied alias exists in the list of composite ContentTypes
    /// </summary>
    /// <param name="alias">Alias of a <see cref="IContentType" /></param>
    /// <returns>True if ContentType with alias exists, otherwise returns False</returns>
    bool ContentTypeCompositionExists(string alias);

    /// <summary>
    ///     Gets a list of ContentType aliases from the current composition
    /// </summary>
    /// <returns>An enumerable list of string aliases</returns>
    IEnumerable<string> CompositionAliases();

    /// <summary>
    ///     Gets a list of ContentType Ids from the current composition
    /// </summary>
    /// <returns>An enumerable list of integer ids</returns>
    IEnumerable<int> CompositionIds();

    /// <summary>
    ///     Gets a list of ContentType keys from the current composition
    /// </summary>
    /// <returns>An enumerable list of integer ids.</returns>
    IEnumerable<Guid> CompositionKeys();

    /// <summary>
    ///     Gets the property types obtained via composition.
    /// </summary>
    /// <remarks>
    ///     <para>Gets them raw, ie with their original variation.</para>
    /// </remarks>
    IEnumerable<IPropertyType> GetOriginalComposedPropertyTypes();
}
