using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

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
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public MediaImportService(
        IShortStringHelper shortStringHelper,
        MediaFileManager mediaFileManager,
        IMediaService mediaService,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        IEntityService entityService,
        AppCaches appCaches,
        IUserService userService,
        IUserIdKeyResolver userIdKeyResolver)
    {
        _shortStringHelper = shortStringHelper;
        _mediaFileManager = mediaFileManager;
        _mediaService = mediaService;
        _mediaUrlGenerators = mediaUrlGenerators;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        _entityService = entityService;
        _appCaches = appCaches;
        _userService = userService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    public async Task<IMedia> ImportAsync(string fileName, Stream fileStream, Guid? parentId, string? mediaTypeAlias, Guid userKey)
    {
        if (fileStream.CanRead == false)
        {
            throw new InvalidOperationException("Could not read from file stream, please ensure it is open and readable");
        }

        var userId = await _userIdKeyResolver.GetAsync(userKey);
        IUser user = _userService.GetUserById(userId)
                     ?? throw new ArgumentException($"Could not find a user with the specified user key ({userKey})", nameof(userKey));

        var safeFileName = fileName.ToSafeFileName(_shortStringHelper);
        var mediaItemName = safeFileName.ToFriendlyName();

        IMedia mediaFile;

        if (parentId is null)
        {
            int[]? userStartNodes = user.CalculateMediaStartNodeIds(_entityService, _appCaches);

            mediaFile = _mediaService.CreateMedia(mediaItemName, userStartNodes != null && userStartNodes.Any() ? userStartNodes[0] : Constants.System.Root, mediaTypeAlias ?? Constants.Conventions.MediaTypes.File, userId);
        }
        else
        {
            mediaFile = _mediaService.CreateMedia(mediaItemName, parentId.Value, mediaTypeAlias ?? Constants.Conventions.MediaTypes.File, userId);
        }

        mediaFile.SetValue(_mediaFileManager, _mediaUrlGenerators, _shortStringHelper, _contentTypeBaseServiceProvider, Constants.Conventions.Media.File, safeFileName, fileStream);

        _mediaService.Save(mediaFile, userId);

        return mediaFile;
    }
}
