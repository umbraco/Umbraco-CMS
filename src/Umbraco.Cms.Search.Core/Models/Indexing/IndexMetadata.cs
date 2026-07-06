namespace Umbraco.Cms.Search.Core.Models.Indexing;

public record IndexMetadata(long DocumentCount, HealthStatus HealthStatus, string ProviderName);
