using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Services;

public class UploadFileService : IUploadFileService
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILocalizedTextService _localizedTextService;

    public UploadFileService(IHostingEnvironment hostingEnvironment, ILocalizedTextService localizedTextService)
    {
        _hostingEnvironment = hostingEnvironment;
        _localizedTextService = localizedTextService;
    }

    public FormFileUploadResult TryLoad(IFormFile file)
    {
        var formFileUploadResult = new FormFileUploadResult();
        var fileName = file.FileName.Trim(Constants.CharArrays.DoubleQuote);
        var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
        var root = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads);
        formFileUploadResult.TemporaryPath = Path.Combine(root, fileName);

        if (!Path.GetFullPath(formFileUploadResult.TemporaryPath).StartsWith(Path.GetFullPath(root)))
        {
            formFileUploadResult.ErrorMessage = _localizedTextService.Localize("media", "invalidFileName");
            formFileUploadResult.CouldLoad = false;
            return formFileUploadResult;
        }

        if (!ext.InvariantEquals("udt"))
        {
            formFileUploadResult.ErrorMessage = _localizedTextService.Localize("media", "disallowedFileType");
            formFileUploadResult.CouldLoad = false;
            return formFileUploadResult;
        }

        using (FileStream stream = File.Create(formFileUploadResult.TemporaryPath))
        {
            file.CopyToAsync(stream).GetAwaiter().GetResult();
        }

        formFileUploadResult.XmlDocument = new XmlDocument {XmlResolver = null};
        formFileUploadResult.XmlDocument.Load(formFileUploadResult.TemporaryPath);

        if (formFileUploadResult.XmlDocument.DocumentElement != null)
        {
            return formFileUploadResult;
        }

        formFileUploadResult.ErrorMessage = _localizedTextService.Localize("speechBubbles", "fileErrorNotFound");
        formFileUploadResult.CouldLoad = false;
        return formFileUploadResult;

    }
}
