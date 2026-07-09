using System.Collections.Concurrent;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Provides storage and management for mapper configurations.
/// </summary>
public class MapperConfigurationStore : ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>
{
}
