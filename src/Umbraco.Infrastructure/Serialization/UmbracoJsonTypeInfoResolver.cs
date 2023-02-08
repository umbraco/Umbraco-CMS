using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Serialization;

public sealed class UmbracoJsonTypeInfoResolver : DefaultJsonTypeInfoResolver, IUmbracoJsonTypeInfoResolver
{
    private readonly ITypeFinder _typeFinder;
    private readonly ConcurrentDictionary<Type, ISet<Type>> _subTypesCache = new ConcurrentDictionary<Type, ISet<Type>>();

    public UmbracoJsonTypeInfoResolver(ITypeFinder typeFinder)
    {
        _typeFinder = typeFinder;
    }

    public IEnumerable<Type> FindSubTypes(Type type)
    {
        if (_subTypesCache.TryGetValue(type, out ISet<Type>? cachedResult))
        {
            return cachedResult;
        }

        var result = _typeFinder.FindClassesOfType(type).ToHashSet();
        _subTypesCache.TryAdd(type, result);
        return result;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo result = base.GetTypeInfo(type, options);

        if (type.IsInterface)
        {
            var subTypes = FindSubTypes(type);

            if (subTypes.Any())
            {
                JsonPolymorphismOptions jsonPolymorphismOptions = result.PolymorphismOptions ?? new JsonPolymorphismOptions();

                IEnumerable<Type> knownSubTypes = jsonPolymorphismOptions.DerivedTypes.Select(x => x.DerivedType);
                IEnumerable<Type> subTypesToAdd = subTypes.Except(knownSubTypes);
                foreach (Type subType in subTypesToAdd)
                {
                    jsonPolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(
                        subType,
                        subType.Name ?? string.Empty));
                }

                result.PolymorphismOptions = jsonPolymorphismOptions;

            }
        }

        return result;
    }
}
