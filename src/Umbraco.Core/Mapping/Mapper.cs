using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Mapping
{
    // notes:
    // AutoMapper maps null to empty arrays, lists, etc

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

            // otherwise, see if we can deal with enumerable

            var ienumerableOfT = typeof(IEnumerable<>);

            bool IsIEnumerableOfT(Type type) =>
                type.IsGenericType &&
                type.GenericTypeArguments.Length == 1 &&
                type.GetGenericTypeDefinition() == ienumerableOfT;

            // try to get source as an IEnumerable<T>
            var sourceIEnumerable = IsIEnumerableOfT(sourceType) ? sourceType : sourceType.GetInterfaces().FirstOrDefault(IsIEnumerableOfT);

            // if source is an IEnumerable<T> and target is T[] or IEnumerable<T>, we can create a map
            if (sourceIEnumerable != null && IsEnumerableOrArrayOfType(targetType))
            {
                var sourceGenericArg = sourceIEnumerable.GenericTypeArguments[0];
                var targetGenericArg = GetEnumerableOrArrayTypeArgument(targetType);

                ctor = GetCtor(sourceGenericArg, targetGenericArg);
                map = GetMap(sourceGenericArg, targetGenericArg);

                // if there is a constructor for the underlying type, create & invoke the map
                if (ctor != null && map != null)
                {
                    // register (for next time) and do it now (for this time)
                    object NCtor(object s, MapperContext c) => MapEnumerable<TTarget>((IEnumerable) s, targetGenericArg, ctor, map, c);
                    DefineCtors(sourceType)[targetType] = NCtor;
                    DefineMaps(sourceType)[targetType] = Identity;
                    return (TTarget) NCtor(source, context);
                }

                throw new InvalidOperationException($"Don't know how to map {sourceGenericArg.FullName} to {targetGenericArg.FullName}, so don't know how to map {sourceType.FullName} to {targetType.FullName}.");
            }

            throw new InvalidOperationException($"Don't know how to map {sourceType.FullName} to {targetType.FullName}.");
        }

        private TTarget MapEnumerable<TTarget>(IEnumerable source, Type targetGenericArg, Func<object, MapperContext, object> ctor, Action<object, object, MapperContext> map, MapperContext context)
        {
            var targetList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetGenericArg));

            foreach (var sourceItem in source)
            {
                var targetItem = ctor(sourceItem, context);
                map(sourceItem, targetItem, context);
                targetList.Add(targetItem);
            }

            object target = targetList;

            if (typeof(TTarget).IsArray)
            {
                var elementType = typeof(TTarget).GetElementType();
                if (elementType == null) throw new Exception("panic");
                var targetArray = Array.CreateInstance(elementType, targetList.Count);
                targetList.CopyTo(targetArray, 0);
                target = targetArray;
            }

            return (TTarget) target;
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
            if (_ctors.TryGetValue(sourceType, out var sourceCtor) && sourceCtor.TryGetValue(targetType, out var ctor))
                return ctor;

            ctor = null;
            foreach (var (stype, sctors) in _ctors)
            {
                if (!stype.IsAssignableFrom(sourceType)) continue;
                if (!sctors.TryGetValue(targetType, out ctor)) continue;

                sourceCtor = sctors;
                break;
            }

            if (ctor == null) return null;

            _ctors[sourceType] = sourceCtor;
            return ctor;
        }

        private Action<object, object, MapperContext> GetMap(Type sourceType, Type targetType)
        {
            if (_maps.TryGetValue(sourceType, out var sourceMap) && sourceMap.TryGetValue(targetType, out var map))
                return map;

            map = null;
            foreach (var (stype, smap) in _maps)
            {
                if (!stype.IsAssignableFrom(sourceType)) continue;
                if (!smap.TryGetValue(targetType, out map)) continue;

                sourceMap = smap;
                break;
            }

            if (map == null) return null;

            _maps[sourceType] = sourceMap;
            return map;
        }

        private static bool IsEnumerableOrArrayOfType(Type type)
        {
            if (type.IsArray && type.GetArrayRank() == 1) return true;
            if (type.IsGenericType && type.GenericTypeArguments.Length == 1) return true;
            return false;
        }

        private static Type GetEnumerableOrArrayTypeArgument(Type type)
        {
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType) return type.GenericTypeArguments[0];
            throw new Exception("panic");
        }

        #endregion
    }
}
