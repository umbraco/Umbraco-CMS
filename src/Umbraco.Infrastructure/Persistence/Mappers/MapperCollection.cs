using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Represents a collection of persistence mappers responsible for mapping between database records and domain entities in Umbraco.
/// </summary>
public class MapperCollection : BuilderCollectionBase<BaseMapper>, IMapperCollection
{
    private readonly Lazy<ConcurrentDictionary<Type, BaseMapper>> _index;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapperCollection"/> class with a function that provides the collection of <see cref="BaseMapper"/> instances.
    /// </summary>
    /// <param name="items">A function that returns an <see cref="IEnumerable{T}"/> of <see cref="BaseMapper"/> objects to be included in the collection.</param>
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

    /// <summary>Attempts to get a mapper for the specified type.</summary>
    /// <param name="type">The type to get the mapper for.</param>
    /// <param name="mapper">When this method returns, contains the mapper associated with the specified type, if found; otherwise, null.</param>
    /// <returns>True if a mapper for the specified type was found; otherwise, false.</returns>
    public bool TryGetMapper(Type type, [MaybeNullWhen(false)] out BaseMapper mapper) =>
        _index.Value.TryGetValue(type, out mapper);
}
