using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.IO;

public sealed class MediaFileManager
{
    private readonly ILogger<MediaFileManager> _logger;
    private readonly IMediaPathScheme _mediaPathScheme;
    private readonly IServiceProvider _serviceProvider;
    private readonly IShortStringHelper _shortStringHelper;
    private MediaUrlGeneratorCollection? _mediaUrlGenerators;

    public MediaFileManager(
        IFileSystem fileSystem,
        IMediaPathScheme mediaPathScheme,
        ILogger<MediaFileManager> logger,
        IShortStringHelper shortStringHelper,
        IServiceProvider serviceProvider)
    {
        _mediaPathScheme = mediaPathScheme;
        _logger = logger;
        _shortStringHelper = shortStringHelper;
        _serviceProvider = serviceProvider;
        FileSystem = fileSystem;
    }

    [Obsolete("Use the ctr that doesn't include unused parameters.")]
    public MediaFileManager(
        IFileSystem fileSystem,
        IMediaPathScheme mediaPathScheme,
        ILogger<MediaFileManager> logger,
        IShortStringHelper shortStringHelper,
        IServiceProvider serviceProvider,
        IOptions<ContentSettings> contentSettings)
        : this(fileSystem, mediaPathScheme, logger, shortStringHelper, serviceProvider)
    {
    }

    /// <summary>
    ///     Gets the media filesystem.
    /// </summary>
    public IFileSystem FileSystem { get; }

    /// <summary>
    ///     Delete media files.
    /// </summary>
    /// <param name="files">Files to delete (filesystem-relative paths).</param>
    public void DeleteMediaFiles(IEnumerable<string> files)
    {
        files = files.Distinct();

        // kinda try to keep things under control
        var options = new ParallelOptions { MaxDegreeOfParallelism = 20 };

        Parallel.ForEach(files, options, file =>
        {
            try
            {
                if (file.IsNullOrWhiteSpace())
                {
                    return;
                }

                if (FileSystem.FileExists(file) == false)
                {
                    return;
                }

                FileSystem.DeleteFile(file);

                var directory = _mediaPathScheme.GetDeleteDirectory(this, file);
                if (!directory.IsNullOrWhiteSpace())
                {
                    FileSystem.DeleteDirectory(directory!, true);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete media file '{File}'.", file);
            }
        });
    }

    #region Media Path

    /// <summary>
    ///     Gets the file path of a media file.
    /// </summary>
    /// <param name="filename">The file name.</param>
    /// <param name="cuid">The unique identifier of the content/media owning the file.</param>
    /// <param name="puid">The unique identifier of the property type owning the file.</param>
    /// <returns>The filesystem-relative path to the media file.</returns>
    /// <remarks>With the old media path scheme, this CREATES a new media path each time it is invoked.</remarks>
    public string GetMediaPath(string? filename, Guid cuid, Guid puid)
    {
        filename = Path.GetFileName(filename);
        if (filename == null)
        {
            throw new ArgumentException("Cannot become a safe filename.", nameof(filename));
        }

        filename = _shortStringHelper.CleanStringForSafeFileName(filename.ToLowerInvariant());

        return _mediaPathScheme.GetFilePath(this, cuid, puid, filename);
    }

    #endregion

    #region Associated Media Files

    /// <summary>
    ///     Returns a stream (file) for a content item (or a null stream if there is no file).
    /// </summary>
    /// <param name="content"></param>
    /// <param name="mediaFilePath">The file path if a file was found</param>
    /// <param name="propertyTypeAlias"></param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="culture"></param>
    /// <param name="segment"></param>
    /// <returns></returns>
    public Stream GetFile(
        IContentBase content,
        out string? mediaFilePath,
        string propertyTypeAlias = Constants.Conventions.Media.File,
        string? culture = null,
        string? segment = null)
    {
        // TODO: If collections were lazy we could just inject them
        if (_mediaUrlGenerators == null)
        {
            _mediaUrlGenerators = _serviceProvider.GetRequiredService<MediaUrlGeneratorCollection>();
        }

        if (!content.TryGetMediaPath(propertyTypeAlias, _mediaUrlGenerators!, out mediaFilePath, culture, segment))
        {
            return Stream.Null;
        }

        return FileSystem.OpenFile(mediaFilePath!);
    }

    /// <summary>
    ///     Stores a media file associated to a property of a content item.
    /// </summary>
    /// <param name="content">The content item owning the media file.</param>
    /// <param name="propertyType">The property type owning the media file.</param>
    /// <param name="filename">The media file name.</param>
    /// <param name="filestream">A stream containing the media bytes.</param>
    /// <param name="oldpath">An optional filesystem-relative filepath to the previous media file.</param>
    /// <returns>The filesystem-relative filepath to the media file.</returns>
    /// <remarks>
    ///     <para>The file is considered "owned" by the content/propertyType.</para>
    ///     <para>
    ///         If an <paramref name="oldpath" /> is provided then that file (and associated thumbnails if any) is deleted
    ///         before the new file is saved, and depending on the media path scheme, the folder may be reused for the new
    ///         file.
    ///     </para>
    /// </remarks>
    public string StoreFile(IContentBase content, IPropertyType? propertyType, string filename, Stream filestream, string? oldpath)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (propertyType == null)
        {
            throw new ArgumentNullException(nameof(propertyType));
        }

        if (filename == null)
        {
            throw new ArgumentNullException(nameof(filename));
        }

        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(filename));
        }

        if (filestream == null)
        {
            throw new ArgumentNullException(nameof(filestream));
        }

        // clear the old file, if any
        if (string.IsNullOrWhiteSpace(oldpath) == false)
        {
            FileSystem.DeleteFile(oldpath);
        }

        // get the filepath, store the data
        var filepath = GetMediaPath(filename, content.Key, propertyType.Key);
        FileSystem.AddFile(filepath, filestream);
        return filepath;
    }

    /// <summary>
    ///     Copies a media file as a new media file, associated to a property of a content item.
    /// </summary>
    /// <param name="content">The content item owning the copy of the media file.</param>
    /// <param name="propertyType">The property type owning the copy of the media file.</param>
    /// <param name="sourcepath">The filesystem-relative path to the source media file.</param>
    /// <returns>The filesystem-relative path to the copy of the media file.</returns>
    public string? CopyFile(IContentBase content, IPropertyType propertyType, string sourcepath)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (propertyType == null)
        {
            throw new ArgumentNullException(nameof(propertyType));
        }

        if (sourcepath == null)
        {
            throw new ArgumentNullException(nameof(sourcepath));
        }

        if (string.IsNullOrWhiteSpace(sourcepath))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(sourcepath));
        }

        // ensure we have a file to copy
        if (FileSystem.FileExists(sourcepath) == false)
        {
            return null;
        }

        // get the filepath
        var filename = Path.GetFileName(sourcepath);
        var filepath = GetMediaPath(filename, content.Key, propertyType.Key);
        FileSystem.CopyFile(sourcepath, filepath);
        return filepath;
    }

    #endregion
}
