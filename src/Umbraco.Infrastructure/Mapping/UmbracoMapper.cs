using System.Collections;
using System.Collections.Concurrent;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Mapping;

// notes:
// AutoMapper maps null to empty arrays, lists, etc

// TODO:
// when mapping from TSource, and no map is found, consider the actual source.GetType()?
// when mapping to TTarget, and no map is found, consider the actual target.GetType()?
// not sure we want to add magic to this simple mapper class, though

/// <summary>
///     Umbraco Mapper.
/// </summary>
/// <remarks>
///     <para>
///         When a map is defined from TSource to TTarget, the mapper automatically knows how to map
///         from IEnumerable{TSource} to IEnumerable{TTarget} (using a List{TTarget}) and to TTarget[].
///     </para>
///     <para>
///         When a map is defined from TSource to TTarget, the mapper automatically uses that map
///         for any source type that inherits from, or implements, TSource.
///     </para>
///     <para>
///         When a map is defined from TSource to TTarget, the mapper can map to TTarget exclusively
///         and cannot re-use that map for types that would inherit from, or implement, TTarget.
///     </para>
///     <para>
///         When using the Map{TSource, TTarget}(TSource source, ...) overloads, TSource is explicit. When
///         using the Map{TTarget}(object source, ...) TSource is defined as source.GetType().
///     </para>
///     <para>In both cases, TTarget is explicit and not typeof(target).</para>
/// </remarks>
public class UmbracoMapper : IUmbracoMapper
{
    // note
    //
    // the outer dictionary *can* be modified, see GetCtor and GetMap, hence have to be ConcurrentDictionary
    // the inner dictionaries are never modified and therefore can be simple Dictionary
    private readonly ConcurrentDictionary<Type, Dictionary<Type, Func<object, MapperContext, object>>> _ctors =
        new();

    private readonly ConcurrentDictionary<Type, Dictionary<Type, Action<object, object, MapperContext>>> _maps =
        new();

    private readonly ICoreScopeProvider _scopeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoMapper" /> class.
    /// </summary>
    /// <param name="profiles"></param>
    /// <param name="scopeProvider"></param>
    public UmbracoMapper(MapDefinitionCollection profiles, ICoreScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;

        foreach (IMapDefinition profile in profiles)
        {
            profile.DefineMaps(this);
        }
    }

    #region Define

    private static TTarget ThrowCtor<TSource, TTarget>(TSource source, MapperContext context)
        => throw new InvalidOperationException($"Don't know how to create {typeof(TTarget).FullName} instances.");

    private static void Identity<TSource, TTarget>(TSource source, TTarget target, MapperContext context)
    {
    }

    /// <summary>
    ///     Defines a mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    public void Define<TSource, TTarget>()
        => Define<TSource, TTarget>(ThrowCtor<TSource, TTarget>, Identity);

    /// <summary>
    ///     Defines a mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="map">A mapping method.</param>
    public void Define<TSource, TTarget>(Action<TSource, TTarget, MapperContext> map)
        => Define(ThrowCtor<TSource, TTarget>, map);

    /// <summary>
    ///     Defines a mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="ctor">A constructor method.</param>
    public void Define<TSource, TTarget>(Func<TSource, MapperContext, TTarget> ctor)
        => Define(ctor, Identity);

    /// <summary>
    ///     Defines a mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="ctor">A constructor method.</param>
    /// <param name="map">A mapping method.</param>
    public void Define<TSource, TTarget>(
        Func<TSource, MapperContext, TTarget> ctor,
        Action<TSource, TTarget, MapperContext> map)
    {
        Type sourceType = typeof(TSource);
        Type targetType = typeof(TTarget);

        Dictionary<Type, Func<object, MapperContext, object>> sourceCtors = DefineCtors(sourceType);
        if (ctor != null)
        {
            sourceCtors[targetType] = (source, context) => ctor((TSource)source, context)!;
        }

        Dictionary<Type, Action<object, object, MapperContext>> sourceMaps = DefineMaps(sourceType);
        sourceMaps[targetType] = (source, target, context) => map((TSource)source, (TTarget)target, context);
    }

    private Dictionary<Type, Func<object, MapperContext, object>> DefineCtors(Type sourceType) =>
        _ctors.GetOrAdd(sourceType, _ => new Dictionary<Type, Func<object, MapperContext, object>>());

    private Dictionary<Type, Action<object, object, MapperContext>> DefineMaps(Type sourceType) =>
        _maps.GetOrAdd(sourceType, _ => new Dictionary<Type, Action<object, object, MapperContext>>());

    #endregion

    #region Map

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>The target object.</returns>
    public TTarget? Map<TTarget>(object? source)
        => Map<TTarget>(source, new MapperContext(this));

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>The target object.</returns>
    public TTarget? Map<TTarget>(object? source, Action<MapperContext> f)
    {
        var context = new MapperContext(this);
        f(context);
        return Map<TTarget>(source, context);
    }

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="context">A mapper context.</param>
    /// <returns>The target object.</returns>
    public TTarget? Map<TTarget>(object? source, MapperContext context)
        => Map<TTarget>(source, source?.GetType(), context);

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>The target object.</returns>
    public TTarget? Map<TSource, TTarget>(TSource? source)
        => Map<TSource, TTarget>(source, new MapperContext(this));

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>The target object.</returns>
    public TTarget? Map<TSource, TTarget>(TSource source, Action<MapperContext> f)
    {
        var context = new MapperContext(this);
        f(context);
        return Map<TSource, TTarget>(source, context);
    }

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="context">A mapper context.</param>
    /// <returns>The target object.</returns>
    public TTarget? Map<TSource, TTarget>(TSource? source, MapperContext context)
        => Map<TTarget>(source, typeof(TSource), context);

    private TTarget? Map<TTarget>(object? source, Type? sourceType, MapperContext context)
    {
        if (source == null)
        {
            return default;
        }

        Type targetType = typeof(TTarget);

        Func<object, MapperContext, object>? ctor = GetCtor(sourceType, targetType);
        Action<object, object, MapperContext>? map = GetMap(sourceType, targetType);

        // if there is a direct constructor, map
        if (ctor != null && map != null)
        {
            var target = ctor(source, context);
            using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
            {
                map(source, target, context);
            }

            return (TTarget)target;
        }

        // otherwise, see if we can deal with enumerable
        Type? ienumerableOfT = typeof(IEnumerable<>);

        bool IsIEnumerableOfT(Type? type)
        {
            return type is not null &&
                   type.IsGenericType &&
                   type.GenericTypeArguments.Length == 1 &&
                   type.GetGenericTypeDefinition() == ienumerableOfT;
        }

        // try to get source as an IEnumerable<T>
        Type? sourceIEnumerable = IsIEnumerableOfT(sourceType)
            ? sourceType
            : sourceType?.GetInterfaces().FirstOrDefault(IsIEnumerableOfT);

        // if source is an IEnumerable<T> and target is T[] or IEnumerable<T>, we can create a map
        if (sourceIEnumerable != null && IsEnumerableOrArrayOfType(targetType))
        {
            Type sourceGenericArg = sourceIEnumerable.GenericTypeArguments[0];
            Type? targetGenericArg = GetEnumerableOrArrayTypeArgument(targetType);

            ctor = GetCtor(sourceGenericArg, targetGenericArg);
            map = GetMap(sourceGenericArg, targetGenericArg);

            // if there is a constructor for the underlying type, create & invoke the map
            if (ctor != null && map != null)
            {
                // register (for next time) and do it now (for this time)
                object NCtor(object s, MapperContext c)
                {
                    return MapEnumerableInternal<TTarget>((IEnumerable)s, targetGenericArg!, ctor, map, c)!;
                }

                DefineCtors(sourceType!)[targetType] = NCtor;
                DefineMaps(sourceType!)[targetType] = Identity;
                return (TTarget)NCtor(source, context);
            }

            throw new InvalidOperationException(
                $"Don't know how to map {sourceGenericArg.FullName} to {targetGenericArg?.FullName}, so don't know how to map {sourceType?.FullName} to {targetType.FullName}.");
        }

        throw new InvalidOperationException($"Don't know how to map {sourceType?.FullName} to {targetType.FullName}.");
    }

    private TTarget? MapEnumerableInternal<TTarget>(
        IEnumerable source,
        Type targetGenericArg,
        Func<object, MapperContext, object> ctor,
        Action<object, object, MapperContext> map,
        MapperContext context)
    {
        var targetList = (IList?)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetGenericArg));

        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        {
            foreach (var sourceItem in source)
            {
                var targetItem = ctor(sourceItem, context);
                map(sourceItem, targetItem, context);
                targetList?.Add(targetItem);
            }
        }

        object? target = targetList;

        if (typeof(TTarget).IsArray)
        {
            Type? elementType = typeof(TTarget).GetElementType();
            if (elementType == null)
            {
                throw new PanicException("elementType == null which should never occur");
            }

            var targetArray = Array.CreateInstance(elementType, targetList?.Count ?? 0);
            targetList?.CopyTo(targetArray, 0);
            target = targetArray;
        }

        return (TTarget?)target;
    }

    /// <summary>
    ///     Maps a source object to an existing target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <returns>The target object.</returns>
    public TTarget Map<TSource, TTarget>(TSource source, TTarget target)
        => Map(source, target, new MapperContext(this));

    /// <summary>
    ///     Maps a source object to an existing target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>The target object.</returns>
    public TTarget Map<TSource, TTarget>(TSource source, TTarget target, Action<MapperContext> f)
    {
        var context = new MapperContext(this);
        f(context);
        return Map(source, target, context);
    }

    /// <summary>
    ///     Maps a source object to an existing target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="context">A mapper context.</param>
    /// <returns>The target object.</returns>
    public TTarget Map<TSource, TTarget>(TSource source, TTarget target, MapperContext context)
    {
        Type sourceType = typeof(TSource);
        Type targetType = typeof(TTarget);

        Action<object, object, MapperContext>? map = GetMap(sourceType, targetType);

        // if there is a direct map, map
        if (map != null)
        {
            using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
            {
                map(source!, target!, context);
            }

            return target;
        }

        // we cannot really map to an existing enumerable - give up
        throw new InvalidOperationException(
            $"Don't know how to map {typeof(TSource).FullName} to {typeof(TTarget).FullName}.");
    }

    private Func<object, MapperContext, object>? GetCtor(Type? sourceType, Type? targetType)
    {
        if (sourceType is null || targetType is null)
        {
            return null;
        }

        if (_ctors.TryGetValue(sourceType, out Dictionary<Type, Func<object, MapperContext, object>>? sourceCtor) &&
            sourceCtor.TryGetValue(targetType, out Func<object, MapperContext, object>? ctor))
        {
            return ctor;
        }

        // we *may* run this more than once but it does not matter
        ctor = null;
        foreach ((Type stype, Dictionary<Type, Func<object, MapperContext, object>> sctors) in _ctors)
        {
            if (!stype.IsAssignableFrom(sourceType))
            {
                continue;
            }

            if (!sctors.TryGetValue(targetType, out ctor))
            {
                continue;
            }

            sourceCtor = sctors;
            break;
        }

        if (ctor is null || sourceCtor is null)
        {
            return null;
        }

        _ctors.AddOrUpdate(sourceType, sourceCtor, (k, v) =>
        {
            // Add missing constructors
            foreach (KeyValuePair<Type, Func<object, MapperContext, object>> c in sourceCtor)
            {
                if (!v.ContainsKey(c.Key))
                {
                    v.Add(c.Key, c.Value);
                }
            }

            return v;
        });

        return ctor;
    }

    private Action<object, object, MapperContext>? GetMap(Type? sourceType, Type? targetType)
    {
        if (sourceType is null || targetType is null)
        {
            return null;
        }

        if (_maps.TryGetValue(sourceType, out Dictionary<Type, Action<object, object, MapperContext>>? sourceMap) &&
            sourceMap.TryGetValue(targetType, out Action<object, object, MapperContext>? map))
        {
            return map;
        }

        // we *may* run this more than once but it does not matter
        map = null;
        foreach ((Type stype, Dictionary<Type, Action<object, object, MapperContext>> smap) in _maps)
        {
            if (!stype.IsAssignableFrom(sourceType))
            {
                continue;
            }

            // TODO: consider looking for assignable types for target too?
            if (!smap.TryGetValue(targetType, out map))
            {
                continue;
            }

            sourceMap = smap;
            break;
        }

        if (map is null || sourceMap is null)
        {
            return null;
        }

        if (_maps.ContainsKey(sourceType))
        {
            foreach (KeyValuePair<Type, Action<object, object, MapperContext>> m in sourceMap)
            {
                if (!_maps[sourceType].TryGetValue(m.Key, out _))
                {
                    _maps[sourceType].Add(m.Key, m.Value);
                }
            }
        }
        else
        {
            _maps[sourceType] = sourceMap;
        }

        return map;
    }

    private static bool IsEnumerableOrArrayOfType(Type type)
    {
        if (type.IsArray && type.GetArrayRank() == 1)
        {
            return true;
        }

        if (type.IsGenericType && type.GenericTypeArguments.Length == 1)
        {
            return true;
        }

        return false;
    }

    private static Type? GetEnumerableOrArrayTypeArgument(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType)
        {
            return type.GenericTypeArguments[0];
        }

        throw new PanicException($"Could not get enumerable or array type from {type}");
    }

    /// <summary>
    ///     Maps an enumerable of source objects to a new list of target objects.
    /// </summary>
    /// <typeparam name="TSourceElement">The type of the source objects.</typeparam>
    /// <typeparam name="TTargetElement">The type of the target objects.</typeparam>
    /// <param name="source">The source objects.</param>
    /// <returns>A list containing the target objects.</returns>
    public List<TTargetElement> MapEnumerable<TSourceElement, TTargetElement>(IEnumerable<TSourceElement> source) =>
        source
            .Select(Map<TSourceElement, TTargetElement>)
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

    /// <summary>
    ///     Maps an enumerable of source objects to a new list of target objects.
    /// </summary>
    /// <typeparam name="TSourceElement">The type of the source objects.</typeparam>
    /// <typeparam name="TTargetElement">The type of the target objects.</typeparam>
    /// <param name="source">The source objects.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>A list containing the target objects.</returns>
    public List<TTargetElement> MapEnumerable<TSourceElement, TTargetElement>(
        IEnumerable<TSourceElement> source,
        Action<MapperContext> f)
    {
        var context = new MapperContext(this);
        f(context);
        return source
            .Select(x => Map<TSourceElement, TTargetElement>(x, context))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
    }

    /// <summary>
    ///     Maps an enumerable of source objects to a new list of target objects.
    /// </summary>
    /// <typeparam name="TSourceElement">The type of the source objects.</typeparam>
    /// <typeparam name="TTargetElement">The type of the target objects.</typeparam>
    /// <param name="source">The source objects.</param>
    /// <param name="context">A mapper context.</param>
    /// <returns>A list containing the target objects.</returns>
    public List<TTargetElement> MapEnumerable<TSourceElement, TTargetElement>(IEnumerable<TSourceElement> source, MapperContext context) =>
        source
            .Select(x => Map<TSourceElement, TTargetElement>(x, context))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

    #endregion
}
