using System;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Core.Persistence.Mappers
{
    public interface IMapperCollection : IBuilderCollection<BaseMapper>
    {
        BaseMapper this[Type type] { get; }
    }
}
