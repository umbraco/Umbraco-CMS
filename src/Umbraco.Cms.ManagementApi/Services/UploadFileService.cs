using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Models;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.ManagementApi.Services;

public class UploadFileService : IUploadFileService
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILocalizedTextService _localizedTextService;

    public UploadFileService(IHostEnvironment hostEnvironment, ILocalizedTextService localizedTextService)
    {
        _hostEnvironment = hostEnvironment;
        _localizedTextService = localizedTextService;
    }

    public FormFileUploadResult TryLoad(IFormFile file)
    {
        var formFileUploadResult = new FormFileUploadResult();
        var fileName = file.FileName.Trim(Constants.CharArrays.DoubleQuote);
        var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
        var root = _hostEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads);
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
