using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.ModelsBuilderDashboard;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilderDashboard;

public class GetModelsBuilderDashboardController : ModelsBuilderDashboardControllerBase
{
    private readonly IModelsBuilderDashboardViewModelFactory _modelsBuilderDashboardViewModelFactory;

    public GetModelsBuilderDashboardController(IModelsBuilderDashboardViewModelFactory modelsBuilderDashboardViewModelFactory) => _modelsBuilderDashboardViewModelFactory = modelsBuilderDashboardViewModelFactory;

    [HttpGet]
    [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
    [MapToApiVersion("1.0")]
    public ModelsBuilderDashboardViewModel GetDashboard() => _modelsBuilderDashboardViewModelFactory.Create();
}
