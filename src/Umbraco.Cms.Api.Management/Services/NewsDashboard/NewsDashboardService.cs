using System.Text.Json;
using Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

namespace Umbraco.Cms.Api.Management.Services.NewsDashboard;

public class NewsDashboardService : INewsDashboardService
{
    public bool TryMapModel(string json, out IEnumerable<NewsDashboardItem>? newsDashboardItem)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            newsDashboardItem = JsonSerializer.Deserialize<List<NewsDashboardItem>>(json, options);
            return newsDashboardItem is not null && newsDashboardItem.Any();
        }
        catch (JsonException)
        {
            newsDashboardItem = null;
            return false;
        }
    }
}
