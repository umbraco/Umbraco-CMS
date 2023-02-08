using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.Services.OperationStatus;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class UploadDictionaryController : DictionaryControllerBase
{
    private readonly IUploadFileService _uploadFileService;
    private readonly IDictionaryFactory _dictionaryFactory;

    public UploadDictionaryController(IUploadFileService uploadFileService, IDictionaryFactory dictionaryFactory)
    {
        _uploadFileService = uploadFileService;
        _dictionaryFactory = dictionaryFactory;
    }

    [HttpPost("upload")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DictionaryUploadViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        Attempt<UdtFileUpload, UdtFileUploadOperationStatus> result = await _uploadFileService.UploadUdtFileAsync(file);

        return result.Status switch
        {
            UdtFileUploadOperationStatus.Success => Ok(_dictionaryFactory.CreateDictionaryImportViewModel(result.Result)),
            UdtFileUploadOperationStatus.InvalidFileType => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid file type")
                .WithDetail("The dictionary import only supports UDT files.")
                .Build()),
            UdtFileUploadOperationStatus.InvalidFileContent => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid file content")
                .WithDetail("The uploaded file could not be read as a valid UDT file.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown UDT file upload operation status")
        };
    }
}
