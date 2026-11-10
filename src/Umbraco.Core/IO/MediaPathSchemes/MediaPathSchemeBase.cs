// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.IO.MediaPathSchemes;

public abstract class MediaPathSchemeBase
{
    /// <summary>
    ///     Determines whether a media file path is one this scheme could have produced for the given content and
    ///     property type.
    /// </summary>
    /// <param name="fileManager">The media filesystem.</param>
    /// <param name="filepath">The filesystem-relative path of the file to check.</param>
    /// <param name="itemGuid">The (content, media) item unique identifier the file is expected to belong to.</param>
    /// <param name="propertyGuid">The property type unique identifier the file is expected to belong to.</param>
    /// <returns>
    ///     <c>true</c> if the path could belong to <paramref name="itemGuid" /> and <paramref name="propertyGuid" />;
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This implementation is shared between all core schemes. It proves foreignness by recomputing the path
    ///         from the keys via <see cref="MediaFileManager.GetMediaPath" /> and comparing, so it never blocks a
    ///         legitimate operation.
    ///         It relies on this scheme deriving distinct paths per item - which a storage-correct scheme already does,
    ///         since otherwise different items sharing a filename would overwrite each other. A scheme whose path does
    ///         not vary by content key, or whose mapping cannot be reproduced by recomputation, should supply its own
    ///         implementation to answer authoritatively.
    ///     </para>
    /// </remarks>
    protected bool IsFilePathOwnedByInternal(MediaFileManager fileManager, string filepath, Guid itemGuid, Guid propertyGuid)
    {
        var filename = Path.GetFileName(filepath);
        if (string.IsNullOrWhiteSpace(filename))
        {
            return false;
        }

        var expectedPath = fileManager.GetMediaPath(filename, itemGuid, propertyGuid);
        return string.Equals(
            MediaFileManager.NormalizePathForComparison(filepath),
            MediaFileManager.NormalizePathForComparison(expectedPath),
            StringComparison.OrdinalIgnoreCase);
    }
}
