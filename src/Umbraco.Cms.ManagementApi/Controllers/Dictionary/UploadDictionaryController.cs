using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Models;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

public class UploadDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IUploadFileService _uploadFileService;

    public UploadDictionaryController(ILocalizedTextService localizedTextService, IUploadFileService uploadFileService)
    {
        _localizedTextService = localizedTextService;
        _uploadFileService = uploadFileService;
    }

    [HttpPost("upload")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DictionaryImportViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DictionaryImportViewModel>> Upload(IFormFile file)
    {
        FormFileUploadResult formFileUploadResult = _uploadFileService.TryLoad(file);
        if (formFileUploadResult.CouldLoad is false || formFileUploadResult.XmlDocument is null)
        {
            return await Task.FromResult(ValidationProblem(
                _localizedTextService.Localize("media", "failedFileUpload"),
                formFileUploadResult.ErrorMessage));
        }

        var model = new DictionaryImportViewModel
        {
            TempFileName = formFileUploadResult.TemporaryPath, DictionaryItems = new List<DictionaryItemsImportViewModel>(),
        };

        var level = 1;
        var currentParent = string.Empty;
        foreach (XmlNode dictionaryItem in formFileUploadResult.XmlDocument.GetElementsByTagName("DictionaryItem"))
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

        return await Task.FromResult(model);
    }
}
