using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

[Obsolete($"This service has been superseded by {nameof(IMediaImportService)}. Will be removed in V16.")]
public class TemporaryMediaService : ITemporaryMediaService
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IMediaService _mediaService;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
    private readonly IHostEnvironment _hostingEnvironment;
    private readonly ILogger<TemporaryMediaService> _logger;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    public TemporaryMediaService(
        IShortStringHelper shortStringHelper,
        MediaFileManager mediaFileManager,
        IMediaService mediaService,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        IHostEnvironment hostingEnvironment,
        ILogger<TemporaryMediaService> logger,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        AppCaches appCaches)
    {
        _shortStringHelper = shortStringHelper;
        _mediaFileManager = mediaFileManager;
        _mediaService = mediaService;
        _mediaUrlGenerators = mediaUrlGenerators;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    public IMedia Save(string temporaryLocation, Guid? startNode, string? mediaTypeAlias)
    {
        IUser? user = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser;
        int userId = user?.Id ?? Constants.Security.SuperUserId;
        var absoluteTempImagePath = _hostingEnvironment.MapPathContentRoot(temporaryLocation);
        var fileName = Path.GetFileName(absoluteTempImagePath);
        var safeFileName = fileName.ToSafeFileName(_shortStringHelper);
        var mediaItemName = safeFileName.ToFriendlyName();

        IMedia mediaFile;

        if (startNode is null)
        {
            int[]? userStartNodes = user?.CalculateMediaStartNodeIds(_entityService, _appCaches);

            mediaFile = _mediaService.CreateMedia(mediaItemName, userStartNodes != null && userStartNodes.Any() ? userStartNodes[0] : Constants.System.Root, mediaTypeAlias ?? Constants.Conventions.MediaTypes.File, userId);
        }
        else
        {
            mediaFile = _mediaService.CreateMedia(mediaItemName, startNode.Value, mediaTypeAlias ?? Constants.Conventions.MediaTypes.File, userId);
        }

        var fileInfo = new FileInfo(absoluteTempImagePath);

        FileStream? fileStream = fileInfo.OpenReadWithRetry();
        if (fileStream is null)
        {
            throw new InvalidOperationException("Could not acquire file stream");
        }

        using (fileStream)
        {
            mediaFile.SetValue(_mediaFileManager, _mediaUrlGenerators, _shortStringHelper, _contentTypeBaseServiceProvider, Constants.Conventions.Media.File, safeFileName, fileStream);
        }

        _mediaService.Save(mediaFile, userId);

        // Delete temp file now that we have persisted it
        var folderName = Path.GetDirectoryName(absoluteTempImagePath);
        try
        {
            if (folderName is not null)
            {
                Directory.Delete(folderName, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not delete temp file or folder {FileName}", absoluteTempImagePath);
        }

        return mediaFile;
    }
}
