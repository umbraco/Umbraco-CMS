// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Infrastructure.PropertyEditors;

/// <summary>
/// Default <see cref="IFileUploadPathResolver"/> that stores files using the standard media path scheme,
/// ignoring the data type configuration.
/// </summary>
internal sealed class FileUploadPathResolver : IFileUploadPathResolver
{
    private readonly MediaFileManager _mediaFileManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadPathResolver"/> class.
    /// </summary>
    /// <param name="mediaFileManager">Manages media file storage and computes the standard media path.</param>
    public FileUploadPathResolver(MediaFileManager mediaFileManager)
        => _mediaFileManager = mediaFileManager;

    /// <inheritdoc/>
    public string ResolvePath(string fileName, object? dataTypeConfiguration, Guid contentKey, Guid propertyTypeKey)
        => _mediaFileManager.GetMediaPath(fileName, contentKey, propertyTypeKey);
}
