using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

/// <summary>
/// Controller for retrieving the configuration and status of the Models Builder.
/// </summary>
[ApiVersion("1.0")]
public class GetModelsBuilderController : ModelsBuilderControllerBase
{
    private readonly IModelsBuilderPresentationFactory _modelsBuilderPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetModelsBuilderController"/> class.
    /// </summary>
    /// <param name="modelsBuilderPresentationFactory">Factory used to create presentation models for the Models Builder API endpoints.</param>
    public GetModelsBuilderController(IModelsBuilderPresentationFactory modelsBuilderPresentationFactory) => _modelsBuilderPresentationFactory = modelsBuilderPresentationFactory;

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ModelsBuilderResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets models builder dashboard data.")]
    [EndpointDescription("Gets the dashboard data and current state of the models builder.")]
    [MapToApiVersion("1.0")]
    public Task<ActionResult<ModelsBuilderResponseModel>> GetDashboard(CancellationToken cancellationToken)
        => Task.FromResult<ActionResult<ModelsBuilderResponseModel>>(Ok(_modelsBuilderPresentationFactory.Create()));
}
