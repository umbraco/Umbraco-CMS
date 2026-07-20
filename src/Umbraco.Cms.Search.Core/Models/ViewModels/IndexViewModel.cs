using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Models.ViewModels;

public class IndexViewModel
{
    public required string IndexAlias { get; set; }

    public required string ProviderName { get; set; }

    public long DocumentCount { get; set; }

    public HealthStatus HealthStatus { get; set; }
}
