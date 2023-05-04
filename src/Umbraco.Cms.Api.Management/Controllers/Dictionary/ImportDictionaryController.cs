using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.Services.OperationStatus;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

[ApiVersion("1.0")]
public class ImportDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemImportService _dictionaryItemImportService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public ImportDictionaryController(
        IDictionaryItemImportService dictionaryItemImportService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dictionaryItemImportService = dictionaryItemImportService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost("import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Import(ImportDictionaryRequestModel importDictionaryRequestModel)
    {
        Attempt<IDictionaryItem?, DictionaryImportOperationStatus> result = await _dictionaryItemImportService
            .ImportDictionaryItemFromUdtFileAsync(
                importDictionaryRequestModel.TemporaryFileId,
                importDictionaryRequestModel.ParentId,
                CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Status switch
        {
            DictionaryImportOperationStatus.Success => CreatedAtAction<ByKeyDictionaryController>(controller => nameof(controller.ByKey), result.Result!.Key),
            DictionaryImportOperationStatus.ParentNotFound => NotFound("The parent dictionary item could not be found."),
            DictionaryImportOperationStatus.TemporaryFileNotFound => NotFound("The temporary file with specified id could not be found."),
            DictionaryImportOperationStatus.InvalidFileType => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid file type")
                .WithDetail("The dictionary import only supports UDT files.")
                .Build()),
            DictionaryImportOperationStatus.InvalidFileContent => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid file content")
                .WithDetail("The uploaded file could not be read as a valid UDT file.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown dictionary import operation status")
        };
    }
}
