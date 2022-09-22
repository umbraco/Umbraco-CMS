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
    [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
    [MapToApiVersion("1.0")]
    public async Task<ModelsBuilderViewModel> GetDashboard() => await Task.FromResult(_modelsBuilderViewModelFactory.Create());
}
