using System;
using System.Collections.Concurrent;

namespace Umbraco.Infrastructure.Persistence.Mappers
{
    public class MapperConfigurationStore : ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>
    { }
}
