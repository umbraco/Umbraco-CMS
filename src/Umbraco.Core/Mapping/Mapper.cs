using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Mapping
{
    // notes:
    // AutoMapper maps null to empty arrays, lists, etc
    // AutoMapper maps derived types - we have to be explicit (see DefineAs) - fixme / really?

    /// <summary>
    /// Umbraco Mapper.
    /// </summary>
    public class Mapper
    {
        private readonly Dictionary<Type, Dictionary<Type, Func<object, MapperContext, object>>> _ctors
            = new Dictionary<Type, Dictionary<Type, Func<object, MapperContext, object>>>();

        private readonly Dictionary<Type, Dictionary<Type, Action<object, object, MapperContext>>> _maps
            = new Dictionary<Type, Dictionary<Type, Action<object, object, MapperContext>>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Mapper"/> class.
        /// </summary>
        /// <param name="profiles"></param>
        public Mapper(MapperProfileCollection profiles)
        {
            foreach (var profile in profiles)
                profile.SetMaps(this);
        }

        #region Define

        private TTarget ThrowCtor<TSource, TTarget>(TSource source, MapperContext context)
            => throw new InvalidOperationException($"Don't know how to create {typeof(TTarget).FullName} instances.");

        private void Identity<TSource, TTarget>(TSource source, TTarget target, MapperContext context)
        { }

        /// <summary>
        /// Defines a mapping.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        public void Define<TSource, TTarget>()
            => Define<TSource, TTarget>(ThrowCtor<TSource, TTarget>, Identity);

        /// <summary>
        /// Defines a mapping.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="map">A mapping method.</param>
        public void Define<TSource, TTarget>(Action<TSource, TTarget, MapperContext> map)
            => Define(ThrowCtor<TSource, TTarget>, map);

        /// <summary>
        /// Defines a mapping.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="ctor">A constructor method.</param>
        public void Define<TSource, TTarget>(Func<TSource, MapperContext, TTarget> ctor)
            => Define(ctor, Identity);

        /// <summary>
        /// Defines a mapping.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="ctor">A constructor method.</param>
        /// <param name="map">A mapping method.</param>
        public void Define<TSource, TTarget>(Func<TSource, MapperContext, TTarget> ctor, Action<TSource, TTarget, MapperContext> map)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var sourceCtors = DefineCtors(sourceType);
            if (ctor != null)
                sourceCtors[targetType] = (source, context) => ctor((TSource)source, context);

            var sourceMaps = DefineMaps(sourceType);
            sourceMaps[targetType] = (source, target, context) => map((TSource)source, (TTarget)target, context);
        }

        /// <summary>
        /// Defines a mapping as a clone of an already defined mapping.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <typeparam name="TAsSource">The equivalent source type.</typeparam>
        /// <typeparam name="TAsTarget">The equivalent target type.</typeparam>
        public void DefineAs<TSource, TTarget, TAsSource, TAsTarget>()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var asSourceType = typeof(TAsSource);
            var asTargetType = typeof(TAsTarget);

            var asCtors = DefineCtors(asSourceType);
            var asMaps = DefineMaps(asSourceType);

            var sourceCtors = DefineCtors(sourceType);
            var sourceMaps = DefineMaps(sourceType);

            if (!asCtors.TryGetValue(asTargetType, out var ctor) || !asMaps.TryGetValue(asTargetType, out var map))
                throw new InvalidOperationException($"Don't know hwo to map from {asSourceType.FullName} to {targetType.FullName}.");

            sourceCtors[targetType] = ctor;
            sourceMaps[targetType] = map;
        }

        private Dictionary<Type, Func<object, MapperContext, object>> DefineCtors(Type sourceType)
        {
            if (!_ctors.TryGetValue(sourceType, out var sourceCtor))
                sourceCtor = _ctors[sourceType] = new Dictionary<Type, Func<object, MapperContext, object>>();
            return sourceCtor;
        }

        private Dictionary<Type, Action<object, object, MapperContext>> DefineMaps(Type sourceType)
        {
            if (!_maps.TryGetValue(sourceType, out var sourceMap))
                sourceMap = _maps[sourceType] = new Dictionary<Type, Action<object, object, MapperContext>>();
            return sourceMap;
        }

        #endregion

        #region Map

        /// <summary>
        /// Maps a source object to a new target object.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TTarget>(object source)
            => Map<TTarget>(source, new MapperContext(this));

        /// <summary>
        /// Maps a source object to a new target object.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="f">A mapper context preparation method.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TTarget>(object source, Action<MapperContext> f)
        {
            var context = new MapperContext(this);
            f(context);
            return Map<TTarget>(source, context);
        }

        /// <summary>
        /// Maps a source object to a new target object.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="context">A mapper context.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TTarget>(object source, MapperContext context)
            => Map<TTarget>(source, source?.GetType(), context);

        /// <summary>
        /// Maps a source object to a new target object.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TSource, TTarget>(TSource source)
            => Map<TSource, TTarget>(source, new MapperContext(this));

        /// <summary>
        /// Maps a source object to a new target object.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="f">A mapper context preparation method.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TSource, TTarget>(TSource source, Action<MapperContext> f)
        {
            var context = new MapperContext(this);
            f(context);
            return Map<TSource, TTarget>(source, context);
        }

        /// <summary>
        /// Maps a source object to a new target object.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="context">A mapper context.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TSource, TTarget>(TSource source, MapperContext context)
            => Map<TTarget>(source, typeof(TSource), context);

        private TTarget Map<TTarget>(object source, Type sourceType, MapperContext context)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var targetType = typeof(TTarget);

            var ctor = GetCtor(sourceType, targetType);
            var map = GetMap(sourceType, targetType);

            // if there is a direct constructor, map
            if (ctor != null && map != null)
            {
                var target = ctor(source, context);
                map(source, target, context);
                return (TTarget)target;
            }

            // else, handle enumerable-to-enumerable mapping
            if (IsGenericOrArray(sourceType) && IsGenericOrArray(targetType))
            {
                var sourceGenericArg = GetGenericOrArrayArg(sourceType);
                var targetGenericArg = GetGenericOrArrayArg(targetType);

                var sourceEnumerableType = typeof(IEnumerable<>).MakeGenericType(sourceGenericArg);
                var targetEnumerableType = typeof(IEnumerable<>).MakeGenericType(targetGenericArg);

                // if both are ienumerable
                if (sourceEnumerableType.IsAssignableFrom(sourceType) && targetEnumerableType.IsAssignableFrom(targetType))
                {
                    ctor = GetCtor(sourceGenericArg, targetGenericArg);
                    map = GetMap(sourceGenericArg, targetGenericArg);

                    // if there is a constructor for the underlying type, map
                    if (ctor != null && map != null)
                    {
                        var sourceEnumerable = (IEnumerable)source;
                        var targetEnumerable = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetGenericArg));

                        foreach (var sourceItem in sourceEnumerable)
                        {
                            var targetItem = ctor(sourceItem, context);
                            map(sourceItem, targetItem, context);
                            targetEnumerable.Add(targetItem);
                        }

                        return (TTarget)targetEnumerable;
                    }
                }
            }

            throw new InvalidOperationException($"Don't know how to map {sourceType.FullName} to {targetType.FullName}.");
        }

        /// <summary>
        /// Maps a source object to an existing target object.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TSource, TTarget>(TSource source, TTarget target)
            => Map(source, target, new MapperContext(this));

        /// <summary>
        /// Maps a source object to an existing target object.
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
        /// Maps a source object to an existing target object.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <param name="context">A mapper context.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TSource, TTarget>(TSource source, TTarget target, MapperContext context)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var map = GetMap(sourceType, targetType);

            // if there is a direct map, map
            if (map != null)
            {
                map(source, target, context);
                return target;
            }

            // we cannot really map to an existing enumerable - give up

            throw new InvalidOperationException($"Don't know how to map {typeof(TSource).FullName} to {typeof(TTarget).FullName}.");
        }

        private Func<object, MapperContext, object> GetCtor(Type sourceType, Type targetType)
        {
            if (!_ctors.TryGetValue(sourceType, out var sourceCtor))
            {
                var type = _maps.Keys.FirstOrDefault(x => x.IsAssignableFrom(sourceType));
                if (type == null)
                    return null;
                sourceCtor = _ctors[sourceType] = _ctors[type];
            }

            return sourceCtor.TryGetValue(targetType, out var ctor) ? ctor : null;
        }

        private Action<object, object, MapperContext> GetMap(Type sourceType, Type targetType)
        {
            if (!_maps.TryGetValue(sourceType, out var sourceMap))
            {
                var type = _maps.Keys.FirstOrDefault(x => x.IsAssignableFrom(sourceType));
                if (type == null)
                    return null;
                sourceMap = _maps[sourceType] = _maps[type];
            }

            return sourceMap.TryGetValue(targetType, out var map) ? map : null;
        }

        private static bool IsGenericOrArray(Type type)
        {
            // note: we're not going to work with just plain enumerables of anything,
            // only on arrays or anything that is generic and implements IEnumerable<>

            if (type.IsArray && type.GetArrayRank() == 1) return true;
            if (type.IsGenericType && type.GenericTypeArguments.Length == 1) return true;
            return false;
        }

        private static Type GetGenericOrArrayArg(Type type)
        {
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType) return type.GenericTypeArguments[0];
            throw new Exception("panic");
        }

        #endregion
    }
}
