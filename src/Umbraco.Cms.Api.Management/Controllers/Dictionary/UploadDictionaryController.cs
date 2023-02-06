using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Factories;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class UploadDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IUploadFileService _uploadFileService;
    private readonly IDictionaryFactory _dictionaryFactory;

    public UploadDictionaryController(ILocalizedTextService localizedTextService, IUploadFileService uploadFileService, IDictionaryFactory dictionaryFactory)
    {
        _localizedTextService = localizedTextService;
        _uploadFileService = uploadFileService;
        _dictionaryFactory = dictionaryFactory;
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

        DictionaryImportViewModel model = _dictionaryFactory.CreateDictionaryImportViewModel(formFileUploadResult);

        if (!model.DictionaryItems.Any())
        {
            return ValidationProblem(
                _localizedTextService.Localize("media", "failedFileUpload"),
                _localizedTextService.Localize("dictionary", "noItemsInFile"));
        }

        return await Task.FromResult(model);
    }
}
