using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Import;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ImportExport;
using Umbraco.Cms.Core.Services.ImportExport;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Import;

[ApiVersion("1.0")]
public class AnalyzeImportController : ImportControllerBase
{
    private readonly ITemporaryFileToXmlImportService _temporaryFileToXmlImportService;
    private readonly IUmbracoMapper _mapper;

    public AnalyzeImportController(
        ITemporaryFileToXmlImportService temporaryFileToXmlImportService,
        IUmbracoMapper mapper)
    {
        _temporaryFileToXmlImportService = temporaryFileToXmlImportService;
        _mapper = mapper;
    }

    [HttpGet("analyze")]
    [ProducesResponseType(typeof(EntityImportAnalysisResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Analyze(CancellationToken cancellationToken, Guid temporaryFileId)
    {
        Attempt<EntityXmlAnalysis?, TemporaryFileXmlImportOperationStatus> analyzeResult = await _temporaryFileToXmlImportService.AnalyzeAsync(temporaryFileId);

        return analyzeResult.Success is false
            ? TemporaryFileXmlImportOperationStatusResult(analyzeResult.Status)
            : Ok(_mapper.Map<EntityImportAnalysisResponseModel>(analyzeResult.Result));
    }
}
