using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

[ApiVersion("1.0")]
public class BuildModelsBuilderController : ModelsBuilderControllerBase
{
    private readonly ModelsBuilderSettings _modelsBuilderSettings;
    private readonly ModelsGenerationError _mbErrors;
    private readonly IModelsGenerator _modelGenerator;

    public BuildModelsBuilderController(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        ModelsGenerationError mbErrors,
        IModelsGenerator modelGenerator)
    {
        _mbErrors = mbErrors;
        _modelGenerator = modelGenerator;
        _modelsBuilderSettings = modelsBuilderSettings.CurrentValue;
    }

    [HttpPost("build")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [EndpointSummary("Builds models.")]
    [EndpointDescription("Triggers the models builder to generate strongly-typed models for content types.")]
    [MapToApiVersion("1.0")]
    public Task<IActionResult> BuildModels(CancellationToken cancellationToken)
    {
        try
        {
            if (_modelsBuilderSettings.ModelsMode != Constants.ModelsBuilder.ModelsModes.SourceCodeManual
                && _modelsBuilderSettings.ModelsMode != Constants.ModelsBuilder.ModelsModes.SourceCodeAuto)
            {
                var problemDetailsModel = new ProblemDetails
                {
                    Title = "Models generation is not enabled",
                    Detail = "ModelsBuilderMode is not set to SourceCodeManual or SourceCodeAuto",
                    Status = StatusCodes.Status428PreconditionRequired,
                    Type = "Error",
                };

                return Task.FromResult<IActionResult>(new ObjectResult(problemDetailsModel) { StatusCode = StatusCodes.Status428PreconditionRequired });
            }

            _modelGenerator.GenerateModels();
            _mbErrors.Clear();
        }
        catch (Exception e)
        {
            _mbErrors.Report("Failed to build models.", e);
        }

        return Task.FromResult<IActionResult>(Ok());
    }
}
