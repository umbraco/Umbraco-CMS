using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

[ApiVersion("1.0")]
public class GetModelsBuilderController : ModelsBuilderControllerBase
{
    private readonly IModelsBuilderPresentationFactory _modelsBuilderPresentationFactory;

    public GetModelsBuilderController(IModelsBuilderPresentationFactory modelsBuilderPresentationFactory) => _modelsBuilderPresentationFactory = modelsBuilderPresentationFactory;

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ModelsBuilderResponseModel), StatusCodes.Status200OK)]
    [MapToApiVersion("1.0")]
    public Task<ActionResult<ModelsBuilderResponseModel>> GetDashboard(CancellationToken cancellationToken)
        => Task.FromResult<ActionResult<ModelsBuilderResponseModel>>(Ok(_modelsBuilderPresentationFactory.Create()));
}
