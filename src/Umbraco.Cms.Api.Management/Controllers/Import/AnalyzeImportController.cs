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

/// <summary>
/// Provides API endpoints for analyzing data during import operations in the management API.
/// </summary>
[ApiVersion("1.0")]
public class AnalyzeImportController : ImportControllerBase
{
    private readonly ITemporaryFileToXmlImportService _temporaryFileToXmlImportService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeImportController"/> class with the specified services for XML import and mapping.
    /// </summary>
    /// <param name="temporaryFileToXmlImportService">An instance of <see cref="ITemporaryFileToXmlImportService"/> used to import temporary files as XML.</param>
    /// <param name="mapper">An instance of <see cref="IUmbracoMapper"/> used for mapping between models.</param>
    public AnalyzeImportController(
        ITemporaryFileToXmlImportService temporaryFileToXmlImportService,
        IUmbracoMapper mapper)
    {
        _temporaryFileToXmlImportService = temporaryFileToXmlImportService;
        _mapper = mapper;
    }

    /// <summary>
    /// Analyzes a previously uploaded import file and returns details about the entities that would be imported.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="temporaryFileId">The unique identifier of the temporary file to analyze.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing an <see cref="EntityImportAnalysisResponseModel"/> with the analysis result if successful;
    /// otherwise, a <see cref="ProblemDetails"/> response describing the error (such as 400 Bad Request or 404 Not Found).
    /// </returns>
    [HttpGet("analyze")]
    [ProducesResponseType(typeof(EntityImportAnalysisResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Analyzes an import file.")]
    [EndpointDescription("Analyzes the uploaded import file and returns an analysis of the imported entities.")]
    public async Task<IActionResult> Analyze(CancellationToken cancellationToken, Guid temporaryFileId)
    {
        Attempt<EntityXmlAnalysis?, TemporaryFileXmlImportOperationStatus> analyzeResult = await _temporaryFileToXmlImportService.AnalyzeAsync(temporaryFileId);

        return analyzeResult.Success is false
            ? TemporaryFileXmlImportOperationStatusResult(analyzeResult.Status)
            : Ok(_mapper.Map<EntityImportAnalysisResponseModel>(analyzeResult.Result));
    }
}
