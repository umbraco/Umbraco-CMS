using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

[ApiVersion("1.0")]
public class BuildModelsBuilderController : ModelsBuilderControllerBase
{
    private ModelsBuilderSettings _modelsBuilderSettings;
    private readonly ModelsGenerationError _mbErrors;
    private readonly IModelsGenerator _modelGenerator;

    [ActivatorUtilitiesConstructor]
    public BuildModelsBuilderController(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        ModelsGenerationError mbErrors,
        IModelsGenerator modelGenerator)
    {
        _mbErrors = mbErrors;
        _modelGenerator = modelGenerator;
        _modelsBuilderSettings = modelsBuilderSettings.CurrentValue;

        modelsBuilderSettings.OnChange(x => _modelsBuilderSettings = x);
    }

    [Obsolete("Please use the constructor that accepts IModelsGenerator only. Will be removed in V16.")]
    public BuildModelsBuilderController(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        ModelsGenerationError mbErrors,
        ModelsGenerator modelGenerator)
        : this(modelsBuilderSettings, mbErrors, (IModelsGenerator)modelGenerator)
    {
    }

    // this constructor is required for the DI, otherwise it'll throw an "Ambiguous Constructor" errors at boot time.
    [Obsolete("Please use the constructor that accepts IModelsGenerator only. Will be removed in V16.")]
    public BuildModelsBuilderController(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        ModelsGenerationError mbErrors,
        IModelsGenerator modelGenerator,
        ModelsGenerator notUsed)
        : this(modelsBuilderSettings, mbErrors, modelGenerator)
    {
    }

    [HttpPost("build")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [MapToApiVersion("1.0")]
    public Task<IActionResult> BuildModels(CancellationToken cancellationToken)
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
