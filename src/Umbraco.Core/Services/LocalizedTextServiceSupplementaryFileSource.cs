using Microsoft.Extensions.FileProviders;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents a supplementary file source for localized text services, allowing plugins and extensions
///     to provide additional or overriding localization keys.
/// </summary>
public class LocalizedTextServiceSupplementaryFileSource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LocalizedTextServiceSupplementaryFileSource" /> class.
    /// </summary>
    /// <param name="file">The file information for the supplementary localization file.</param>
    /// <param name="overwriteCoreKeys">
    ///     If set to <c>true</c>, keys in this file will overwrite core localization keys.
    /// </param>
    public LocalizedTextServiceSupplementaryFileSource(IFileInfo file, bool overwriteCoreKeys)
    {
        FileInfo = file ?? throw new ArgumentNullException(nameof(file));
        OverwriteCoreKeys = overwriteCoreKeys;
    }

    /// <summary>
    ///     Gets the file information for this supplementary localization file.
    /// </summary>
    public IFileInfo FileInfo { get; }

    /// <summary>
    ///     Gets a value indicating whether keys in this file should overwrite core localization keys.
    /// </summary>
    public bool OverwriteCoreKeys { get; }
}
