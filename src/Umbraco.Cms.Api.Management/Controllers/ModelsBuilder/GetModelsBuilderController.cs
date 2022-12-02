using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

public class GetModelsBuilderController : ModelsBuilderControllerBase
{
    private readonly IModelsBuilderViewModelFactory _modelsBuilderViewModelFactory;

    public GetModelsBuilderController(IModelsBuilderViewModelFactory modelsBuilderViewModelFactory) => _modelsBuilderViewModelFactory = modelsBuilderViewModelFactory;

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ModelsBuilderViewModel), StatusCodes.Status200OK)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ModelsBuilderViewModel>> GetDashboard() => await Task.FromResult(Ok(_modelsBuilderViewModelFactory.Create()));
}
