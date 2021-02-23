using System;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers
{
    public interface IMapperCollection : IBuilderCollection<BaseMapper>
    {
        bool TryGetMapper(Type type, out BaseMapper mapper);
        BaseMapper this[Type type] { get; }
    }
}
