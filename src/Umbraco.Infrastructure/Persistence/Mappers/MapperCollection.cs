using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

public class MapperCollection : BuilderCollectionBase<BaseMapper>, IMapperCollection
{
    private readonly Lazy<ConcurrentDictionary<Type, BaseMapper>> _index;

    public MapperCollection(Func<IEnumerable<BaseMapper>> items)
        : base(items) =>
        _index = new Lazy<ConcurrentDictionary<Type, BaseMapper>>(() =>
        {
            var d = new ConcurrentDictionary<Type, BaseMapper>();
            foreach (BaseMapper mapper in this)
            {
                IEnumerable<MapperForAttribute> attributes =
                    mapper.GetType().GetCustomAttributes<MapperForAttribute>(false);
                foreach (MapperForAttribute a in attributes)
                {
                    d.TryAdd(a.EntityType, mapper);
                }
            }

            return d;
        });

    /// <summary>
    ///     Returns a mapper for this type, throw an exception if not found
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public BaseMapper this[Type type]
    {
        get
        {
            if (_index.Value.TryGetValue(type, out BaseMapper? mapper))
            {
                return mapper;
            }

            throw new Exception($"Could not find a mapper matching type {type.FullName}.");
        }
    }

    public bool TryGetMapper(Type type, [MaybeNullWhen(false)] out BaseMapper mapper) =>
        _index.Value.TryGetValue(type, out mapper);
}
