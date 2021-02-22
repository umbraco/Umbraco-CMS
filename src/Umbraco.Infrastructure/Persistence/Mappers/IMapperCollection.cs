using System;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Core.Persistence.Mappers
{
    public interface IMapperCollection : IBuilderCollection<BaseMapper>
    {
        bool TryGetMapper(Type type, out BaseMapper mapper);
        BaseMapper this[Type type] { get; }
    }
}
