using System;

namespace Umbraco.Core.Persistence.Mappers
{
    public interface IMapperCollection
    {
        BaseMapper this[Type type] { get; }
    }
}
