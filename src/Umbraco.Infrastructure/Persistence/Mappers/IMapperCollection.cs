using System;
using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers
{
    public interface IMapperCollection : IBuilderCollection<BaseMapper>
    {
        bool TryGetMapper(Type type, [MaybeNullWhen(false)] out BaseMapper mapper);
        BaseMapper this[Type type] { get; }
    }
}
