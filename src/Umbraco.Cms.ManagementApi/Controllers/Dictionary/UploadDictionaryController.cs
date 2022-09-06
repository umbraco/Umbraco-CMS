using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

public class UploadDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IHostingEnvironment _hostingEnvironment;

    public UploadDictionaryController(ILocalizedTextService localizedTextService, IHostingEnvironment hostingEnvironment)
    {
        _localizedTextService = localizedTextService;
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpPost("upload")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<DictionaryImportViewModel> Upload(IFormFile file)
    {
        var fileName = file.FileName.Trim(Constants.CharArrays.DoubleQuote);
        var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
        var root = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads);
        var tempPath = Path.Combine(root, fileName);

        if (!Path.GetFullPath(tempPath).StartsWith(Path.GetFullPath(root)))
        {
            return ValidationProblem(
                _localizedTextService.Localize("media", "failedFileUpload"),
                _localizedTextService.Localize("media", "invalidFileName"));
        }

        if (!ext.InvariantEquals("udt"))
        {
            return ValidationProblem(
                _localizedTextService.Localize("media", "failedFileUpload"),
                _localizedTextService.Localize("media", "disallowedFileType"));
        }

        using (FileStream stream = System.IO.File.Create(tempPath))
        {
            file.CopyToAsync(stream).GetAwaiter().GetResult();
        }

        var xd = new XmlDocument {XmlResolver = null};
        xd.Load(tempPath);

        if (xd.DocumentElement == null)
        {
            return ValidationProblem(
                _localizedTextService.Localize("media", "failedFileUpload"),
                _localizedTextService.Localize("speechBubbles", "fileErrorNotFound"));
        }

        var model = new DictionaryImportViewModel
        {
            TempFileName = tempPath, DictionaryItems = new List<DictionaryItemsImportViewModel>(),
        };

        var level = 1;
        var currentParent = string.Empty;
        foreach (XmlNode dictionaryItem in xd.GetElementsByTagName("DictionaryItem"))
        {
            var name = dictionaryItem.Attributes?.GetNamedItem("Name")?.Value ?? string.Empty;
            var parentKey = dictionaryItem?.ParentNode?.Attributes?.GetNamedItem("Key")?.Value ?? string.Empty;

            if (parentKey != currentParent || level == 1)
            {
                level += 1;
                currentParent = parentKey;
            }

            model.DictionaryItems.Add(new DictionaryItemsImportViewModel { Level = level, Name = name });
        }

        if (!model.DictionaryItems.Any())
        {
            return ValidationProblem(
                _localizedTextService.Localize("media", "failedFileUpload"),
                _localizedTextService.Localize("dictionary", "noItemsInFile"));
        }

        return model;
    }
}
