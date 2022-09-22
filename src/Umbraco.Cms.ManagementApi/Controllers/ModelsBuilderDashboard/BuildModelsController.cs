using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.ModelsBuilderDashboard;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilderDashboard;

public class BuildModelsController : ModelsBuilderDashboardControllerBase
{
    private readonly ModelsBuilderSettings _modelsBuilderSettings;
    private readonly ModelsGenerationError _mbErrors;
    private readonly ModelsGenerator _modelGenerator;
    private readonly IModelsBuilderDashboardViewModelFactory _modelsBuilderDashboardViewModelFactory;

    public BuildModelsController(
        ModelsBuilderSettings modelsBuilderSettings,
        ModelsGenerationError mbErrors,
        ModelsGenerator modelGenerator,
        IModelsBuilderDashboardViewModelFactory modelsBuilderDashboardViewModelFactory)
    {
        _modelsBuilderSettings = modelsBuilderSettings;
        _mbErrors = mbErrors;
        _modelGenerator = modelGenerator;
        _modelsBuilderDashboardViewModelFactory = modelsBuilderDashboardViewModelFactory;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [MapToApiVersion("1.0")]
    public IActionResult BuildModels()
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

                return new ObjectResult(problemDetailsModel) { StatusCode = StatusCodes.Status428PreconditionRequired };
            }

            _modelGenerator.GenerateModels();
            _mbErrors.Clear();
        }
        catch (Exception e)
        {
            _mbErrors.Report("Failed to build models.", e);
        }

        return Created("api/v1/modelsBuilderDashboard", null);
    }
}
