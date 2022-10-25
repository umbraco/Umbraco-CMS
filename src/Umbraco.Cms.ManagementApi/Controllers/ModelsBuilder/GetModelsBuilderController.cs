using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.ModelsBuilderDashboard;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilderDashboard;

public class GetModelsBuilderController : ModelsBuilderControllerBase
{
    private readonly IModelsBuilderViewModelFactory _modelsBuilderViewModelFactory;

    public GetModelsBuilderController(IModelsBuilderViewModelFactory modelsBuilderViewModelFactory) => _modelsBuilderViewModelFactory = modelsBuilderViewModelFactory;

    [HttpGet]
    [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status200OK)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ModelsBuilderViewModel>> GetDashboard() => await Task.FromResult(Ok(_modelsBuilderViewModelFactory.Create()));
}
