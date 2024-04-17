using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represents a collection of deployable file types used for each specific entity type.
/// </summary>
public interface IFileTypeCollection
{
    /// <summary>
    /// Gets the <see cref="IFileType"/> for the specified entity type.
    /// </summary>
    /// <value>
    /// The <see cref="IFileType"/>.
    /// </value>
    /// <param name="entityType">The entity type.</param>
    /// <returns>
    /// The <see cref="IFileType"/> for the specified entity type.
    /// </returns>
    IFileType this[string entityType] { get; }

    /// <summary>
    /// Gets the <see cref="IFileType" /> for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="fileType">When this method returns, contains the file type associated with the specified entity type, if the item is found; otherwise, <c>null</c>.</param>
    /// <returns>
    ///   <c>true</c> if the file type associated with the specified entity type was found; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetValue(string entityType, [NotNullWhen(true)] out IFileType? fileType)
    {
        // TODO (V14): Remove default implementation
        if (Contains(entityType))
        {
            fileType = this[entityType];
            return true;
        }

        fileType = null;
        return false;
    }

    /// <summary>
    /// Determines whether this collection contains a file type for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>
    ///   <c>true</c> if this collection contains a file type for the specified entity type; otherwise, <c>false</c>.
    /// </returns>
    bool Contains(string entityType);

    /// <summary>
    /// Gets the entity types.
    /// </summary>
    /// <returns>
    /// The entity types.
    /// </returns>
    ICollection<string> GetEntityTypes() => Array.Empty<string>(); // TODO (V14): Remove default implementation
}
