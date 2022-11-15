using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.SectionAccessMedia)]
public class MediaPickerThreeController : ContentControllerBase
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ContentSettings _contentSettings;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly IIOHelper _ioHelper;

    public MediaPickerThreeController(
        ICultureDictionary cultureDictionary,
        ILoggerFactory loggerFactory,
        IShortStringHelper shortStringHelper,
        IEventMessagesFactory eventMessages,
        ILocalizedTextService localizedTextService,
        IJsonSerializer serializer,
        IHostingEnvironment hostingEnvironment,
        IOptionsSnapshot<ContentSettings> contentSettings,
        IImageUrlGenerator imageUrlGenerator,
        IIOHelper ioHelper)
        : base(cultureDictionary, loggerFactory, shortStringHelper, eventMessages, localizedTextService, serializer)
    {
        _hostingEnvironment = hostingEnvironment;
        _contentSettings = contentSettings.Value;
        _imageUrlGenerator = imageUrlGenerator;
        _ioHelper = ioHelper;
    }

    [HttpPost]
    public async Task<IActionResult> UploadMedia(List<IFormFile> file)
    {
        // Create an unique folder path to help with concurrent users to avoid filename clash
        var imageTempPath =
            _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads + "/" + Guid.NewGuid());

        // Ensure image temp path exists
        if (Directory.Exists(imageTempPath) == false)
        {
            Directory.CreateDirectory(imageTempPath);
        }

        // Must have a file
        if (file.Count == 0)
        {
            return NotFound();
        }

        // Should only have one file
        if (file.Count > 1)
        {
            return new UmbracoProblemResult("Only one file can be uploaded at a time", HttpStatusCode.BadRequest);
        }

        // Really we should only have one file per request to this endpoint
        IFormFile formFile = file.First();

        var fileName = formFile.FileName.Trim(new[] { '\"' }).TrimEnd();
        var safeFileName = fileName.ToSafeFileName(ShortStringHelper);
        var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLowerInvariant();

        if (_contentSettings.IsFileAllowedForUpload(ext) == false)
        {
            // Throw some error - to say can't upload this IMG type
            return new UmbracoProblemResult("This is not an image filetype extension that is approved", HttpStatusCode.BadRequest);
        }

        var newFilePath = imageTempPath + Path.DirectorySeparatorChar + safeFileName;
        var relativeNewFilePath = GetRelativePath(newFilePath);

        await using (FileStream stream = System.IO.File.Create(newFilePath))
        {
            await formFile.CopyToAsync(stream);
        }

        return Ok(new { tmpLocation = relativeNewFilePath });
    }

    // Use private method istead of _ioHelper.GetRelativePath as that is relative for the webroot and not the content root.
    private string GetRelativePath(string path)
    {
        if (path.IsFullPath())
        {
            var rootDirectory = _hostingEnvironment.MapPathContentRoot("~");
            var relativePath = _ioHelper.PathStartsWith(path, rootDirectory) ? path[rootDirectory.Length..] : path;
            path = relativePath;
        }

        return PathUtility.EnsurePathIsApplicationRootPrefixed(path);
    }
}
