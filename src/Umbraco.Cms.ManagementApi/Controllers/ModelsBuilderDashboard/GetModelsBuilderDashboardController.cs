using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.ModelsBuilderDashboard;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilderDashboard;

public class GetModelsBuilderDashboardController : ModelsBuilderDashboardControllerBase
{
    private readonly IModelsBuilderDashboardViewModelFactory _modelsBuilderDashboardViewModelFactory;

    public GetModelsBuilderDashboardController(IModelsBuilderDashboardViewModelFactory modelsBuilderDashboardViewModelFactory) => _modelsBuilderDashboardViewModelFactory = modelsBuilderDashboardViewModelFactory;

    [HttpGet]
    public ModelsBuilderDashboardViewModel GetDashboard() => _modelsBuilderDashboardViewModelFactory.Create();
}
