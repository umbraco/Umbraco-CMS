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

    /// <summary>
    /// Provides API endpoints for importing dictionary items into the Umbraco CMS.
    /// </summary>
[ApiVersion("1.0")]
public class ImportDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemImportService _dictionaryItemImportService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportDictionaryController"/> class, which handles dictionary item import operations in the Umbraco backoffice API.
    /// </summary>
    /// <param name="dictionaryItemImportService">Service used to import dictionary items.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public ImportDictionaryController(
        IDictionaryItemImportService dictionaryItemImportService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dictionaryItemImportService = dictionaryItemImportService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Imports a dictionary from a provided UDT file upload.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the import operation.</param>
    /// <param name="importDictionaryRequestModel">The model containing the uploaded UDT file and optional parent dictionary item information.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the import operation:
    /// returns <c>201 Created</c> on success, <c>400 Bad Request</c> for invalid file types or content, and <c>404 Not Found</c> if the parent or file is missing.
    /// </returns>
    [HttpPost("import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Imports a dictionary.")]
    [EndpointDescription("Imports a dictionary from the provided file upload.")]
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
