using Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

namespace Umbraco.Cms.Api.Management.Services.NewsDashboard;

/// <summary>
/// Defines a service for retrieving news dashboard items.
/// </summary>
public interface INewsDashboardService
{
    /// <summary>
    /// Asynchronously retrieves the collection of news dashboard items.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="NewsDashboardResponseModel"/> with the retrieved news dashboard items.
    /// </returns>
    Task<NewsDashboardResponseModel> GetItemsAsync();
}
