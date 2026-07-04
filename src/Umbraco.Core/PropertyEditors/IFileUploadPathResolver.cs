// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Resolves the storage path for a file managed by the file upload property editor (or an editor derived from it).
/// </summary>
/// <remarks>
/// This is distinct from <see cref="IO.IMediaPathScheme"/>: the path scheme is the low-level, system-wide layout of
/// media files, whereas this resolver is a configuration-aware strategy scoped to the file upload editor. Replace the
/// registered implementation (via <c>AddUnique</c>) to control where uploaded and copied files are stored — for example
/// to prepend a prefix read from a custom data type configuration.
/// </remarks>
public interface IFileUploadPathResolver
{
    /// <summary>
    /// Resolves the filesystem-relative path at which a file should be stored.
    /// </summary>
    /// <param name="fileName">The name of the file being stored.</param>
    /// <param name="dataTypeConfiguration">
    /// The configuration of the data type owning the property, or <c>null</c> if unavailable. A custom
    /// implementation can cast this to its own configuration type to vary the path per data type.
    /// </param>
    /// <param name="contentKey">The key of the content item owning the file.</param>
    /// <param name="propertyTypeKey">The key of the property type owning the file.</param>
    /// <returns>The filesystem-relative path to store the file at.</returns>
    string ResolvePath(string fileName, object? dataTypeConfiguration, Guid contentKey, Guid propertyTypeKey);
}
