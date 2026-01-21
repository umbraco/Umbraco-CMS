using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Service for importing media files into the Umbraco media library.
/// </summary>
internal sealed class MediaImportService : IMediaImportService
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IMediaService _mediaService;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaImportService" /> class.
    /// </summary>
    /// <param name="shortStringHelper">The short string helper for file name sanitization.</param>
    /// <param name="mediaFileManager">The media file manager.</param>
    /// <param name="mediaService">The media service.</param>
    /// <param name="mediaUrlGenerators">The media URL generators collection.</param>
    /// <param name="contentTypeBaseServiceProvider">The content type base service provider.</param>
    /// <param name="entityService">The entity service.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="userService">The user service.</param>
    public MediaImportService(
        IShortStringHelper shortStringHelper,
        MediaFileManager mediaFileManager,
        IMediaService mediaService,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        IEntityService entityService,
        AppCaches appCaches,
        IUserService userService)
    {
        _shortStringHelper = shortStringHelper;
        _mediaFileManager = mediaFileManager;
        _mediaService = mediaService;
        _mediaUrlGenerators = mediaUrlGenerators;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        _entityService = entityService;
        _appCaches = appCaches;
        _userService = userService;
    }

    /// <inheritdoc />
    public async Task<IMedia> ImportAsync(string fileName, Stream fileStream, Guid? parentId, string? mediaTypeAlias, Guid userKey)
    {
        if (fileStream.CanRead == false)
        {
            throw new InvalidOperationException("Could not read from file stream, please ensure it is open and readable");
        }

        IUser user = await _userService.GetRequiredUserAsync(userKey);

        var safeFileName = fileName.ToSafeFileName(_shortStringHelper);
        var mediaItemName = safeFileName.ToFriendlyName();

        IMedia mediaFile;

        if (parentId is null)
        {
            int[]? userStartNodes = user.CalculateMediaStartNodeIds(_entityService, _appCaches);

            mediaFile = _mediaService.CreateMedia(mediaItemName, userStartNodes != null && userStartNodes.Any() ? userStartNodes[0] : Constants.System.Root, mediaTypeAlias ?? Constants.Conventions.MediaTypes.File, user.Id);
        }
        else
        {
            mediaFile = _mediaService.CreateMedia(mediaItemName, parentId.Value, mediaTypeAlias ?? Constants.Conventions.MediaTypes.File, user.Id);
        }

        mediaFile.SetValue(_mediaFileManager, _mediaUrlGenerators, _shortStringHelper, _contentTypeBaseServiceProvider, Constants.Conventions.Media.File, safeFileName, fileStream);

        _mediaService.Save(mediaFile, user.Id);

        return mediaFile;
    }
}
