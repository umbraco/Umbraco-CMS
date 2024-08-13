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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        ImportDictionaryRequestModel importDictionaryRequestModel)
    {
        Attempt<IDictionaryItem?, DictionaryImportOperationStatus> result = await _dictionaryItemImportService
            .ImportDictionaryItemFromUdtFileAsync(
                importDictionaryRequestModel.TemporaryFile.Id,
                importDictionaryRequestModel.Parent?.Id,
                CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyDictionaryController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : OperationStatusResult(result.Status, problemDetailsBuilder => result.Status switch
            {
                DictionaryImportOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                    .WithTitle("The parent dictionary item could not be found.")
                    .Build()),
                DictionaryImportOperationStatus.TemporaryFileNotFound => NotFound(problemDetailsBuilder
                    .WithTitle("The temporary file with specified id could not be found.")
                    .Build()),
                DictionaryImportOperationStatus.InvalidFileType => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid file type")
                    .WithDetail("The dictionary import only supports UDT files.")
                    .Build()),
                DictionaryImportOperationStatus.InvalidFileContent => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid file content")
                    .WithDetail("The uploaded file could not be read as a valid UDT file.")
                    .Build()),
                _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                    .WithTitle("Unknown dictionary import operation status.")
                    .Build()),
            });
    }
}
