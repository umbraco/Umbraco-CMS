using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.Serialization;

public sealed class UmbracoJsonTypeInfoResolver : DefaultJsonTypeInfoResolver, IUmbracoJsonTypeInfoResolver
{
    private readonly ITypeFinder _typeFinder;
    private readonly ConcurrentDictionary<Type, ISet<Type>> _subTypesCache = new ConcurrentDictionary<Type, ISet<Type>>();

    public UmbracoJsonTypeInfoResolver(ITypeFinder typeFinder)
        => _typeFinder = typeFinder;

    public IEnumerable<Type> FindSubTypes(Type type)
    {
        JsonDerivedTypeAttribute[] explicitJsonDerivedTypes = type
            .GetCustomAttributes<JsonDerivedTypeAttribute>(false)
            .ToArray();
        if (explicitJsonDerivedTypes.Any())
        {
            return explicitJsonDerivedTypes.Select(a => a.DerivedType);
        }

        if (type.IsInterface is false)
        {
            // IMPORTANT: do NOT return an empty enumerable here. it will cause nullability to fail on reference
            //            properties, because "$ref" does not mix and match well with "nullable" in OpenAPI.
            //            see also https://github.com/OAI/OpenAPI-Specification/issues/1368
            return new[] { type };
        }

        if (_subTypesCache.TryGetValue(type, out ISet<Type>? cachedResult))
        {
            return cachedResult;
        }

        var result = _typeFinder.FindClassesOfType(type).OrderBy(x => x.Name).ToHashSet();
        _subTypesCache.TryAdd(type, result);
        return result;
    }

    public string? GetTypeDiscriminatorValue(Type type)
    {
        JsonDerivedTypeAttribute? jsonDerivedTypeAttribute = type
            .GetBaseTypes(false)
            .WhereNotNull()
            .SelectMany(baseType => baseType.GetCustomAttributes<JsonDerivedTypeAttribute>(false))
            .FirstOrDefault(attr => attr.DerivedType == type);

        if (jsonDerivedTypeAttribute is not null)
        {
            // IMPORTANT: do NOT perform fallback to type.Name here - it will work for the schema generation,
            //            but not for the actual serialization, and then it's only going to cause confusion.
            return jsonDerivedTypeAttribute.TypeDiscriminator?.ToString();
        }

        return typeof(IOpenApiDiscriminator).IsAssignableFrom(type) ? type.Name : null;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo result = base.GetTypeInfo(type, options);
        return type.IsInterface
            ? GetTypeInfoForInterface(result, type, options)
            : result;
    }

    private JsonTypeInfo GetTypeInfoForInterface(JsonTypeInfo result, Type type, JsonSerializerOptions options)
    {
        IEnumerable<Type> subTypes = FindSubTypes(type);

        if (!subTypes.Any())
        {
            return result;
        }

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


        return result;
    }
}
