using System.Collections.Concurrent;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

public class MapperConfigurationStore : ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>
{
}
