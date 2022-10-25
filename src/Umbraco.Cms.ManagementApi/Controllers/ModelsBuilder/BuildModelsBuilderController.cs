using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilderDashboard;

public class BuildModelsBuilderController : ModelsBuilderControllerBase
{
    private ModelsBuilderSettings _modelsBuilderSettings;
    private readonly ModelsGenerationError _mbErrors;
    private readonly ModelsGenerator _modelGenerator;

    public BuildModelsBuilderController(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        ModelsGenerationError mbErrors,
        ModelsGenerator modelGenerator)
    {
        _mbErrors = mbErrors;
        _modelGenerator = modelGenerator;
        _modelsBuilderSettings = modelsBuilderSettings.CurrentValue;

        modelsBuilderSettings.OnChange(x => _modelsBuilderSettings = x);
    }

    [HttpPost("build")]
    [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> BuildModels()
    {
        try
        {
            if (!_modelsBuilderSettings.ModelsMode.SupportsExplicitGeneration())
            {
                var problemDetailsModel = new ProblemDetails
                {
                    Title = "Models generation is not enabled",
                    Detail = "ModelsBuilderMode is not set to SourceCodeManual or SourceCodeAuto",
                    Status = StatusCodes.Status428PreconditionRequired,
                    Type = "Error",
                };

                return await Task.FromResult(new ObjectResult(problemDetailsModel) { StatusCode = StatusCodes.Status428PreconditionRequired });
            }

            _modelGenerator.GenerateModels();
            _mbErrors.Clear();
        }
        catch (Exception e)
        {
            _mbErrors.Report("Failed to build models.", e);
        }

        return await Task.FromResult(Created("api/v1/modelsBuilderDashboard", null));
    }
}
