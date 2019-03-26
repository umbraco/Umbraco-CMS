using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Mapping
{
    // FIXME needs documentation and cleanup!

    public class Mapper
    {
        private readonly Dictionary<Type, Dictionary<Type, Func<object, MapperContext, object>>> _ctors
            = new Dictionary<Type, Dictionary<Type, Func<object, MapperContext, object>>>();

        private readonly Dictionary<Type, Dictionary<Type, Action<object, object, MapperContext>>> _maps
            = new Dictionary<Type, Dictionary<Type, Action<object, object, MapperContext>>>();

        public Mapper(MapperProfileCollection profiles)
        {
            foreach (var profile in profiles)
                profile.SetMaps(this);
        }

        #region Define

        public void Define<TSource, TTarget>()
            => Define<TSource, TTarget>((source, target, context) => { });

        public void Define<TSource, TTarget>(Action<TSource, TTarget, MapperContext> map)
            => Define((source, context) => throw new NotSupportedException($"Don't know how to create {typeof(TTarget)} instances."), map);

        public void Define<TSource, TTarget>(Func<TSource, MapperContext, TTarget> ctor)
            => Define(ctor, (source, target, context) => { });

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

        public void Define<TSource, TTarget>(Func<TSource, MapperContext, TTarget> ctor, Action<TSource, TTarget, MapperContext> map)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var sourceCtors = DefineCtors(sourceType);
            sourceCtors[targetType] = (source, context) => ctor((TSource)source, context);

            var sourceMaps = DefineMaps(sourceType);
            sourceMaps[targetType] = (source, target, context) => map((TSource)source, (TTarget)target, context);
        }

        #endregion

        #region Map

        public TTarget Map<TTarget>(object source)
            => Map<TTarget>(source, new MapperContext(this));

        public TTarget Map<TTarget>(object source, Action<MapperContext> f)
        {
            var context = new MapperContext(this);
            f(context);
            return Map<TTarget>(source, context);
        }

        public TTarget Map<TTarget>(object source, MapperContext context)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sourceType = source.GetType();
            var targetType = typeof(TTarget);

            var ctor = GetCtor(sourceType, typeof(TTarget));
            var map = GetMap(sourceType, typeof(TTarget));
            if (ctor != null && map != null)
            {
                var target = ctor(source, context);
                map(source, target, context);
                return (TTarget)target;
            }

            bool IsOk(Type type)
            {
                // note: we're not going to work with just plain enumerables of anything,
                // only on arrays or anything that is generic and implements IEnumerable<>

                if (type.IsArray && type.GetArrayRank() == 1) return true;
                if (type.IsGenericType && type.GenericTypeArguments.Length == 1) return true;
                return false;
            }

            Type GetGenericArg(Type type)
            {
                if (type.IsArray) return type.GetElementType();
                if (type.IsGenericType) return type.GenericTypeArguments[0];
                throw new Exception("panic");
            }

            if (IsOk(sourceType) && IsOk(targetType))
            {
                var sourceGenericArg = GetGenericArg(sourceType);
                var targetGenericArg = GetGenericArg(targetType);

                var sourceEnumerableType = typeof(IEnumerable<>).MakeGenericType(sourceGenericArg);
                var targetEnumerableType = typeof(IEnumerable<>).MakeGenericType(targetGenericArg);

                if (sourceEnumerableType.IsAssignableFrom(sourceType) && targetEnumerableType.IsAssignableFrom(targetType))
                {
                    ctor = GetCtor(sourceGenericArg, targetGenericArg);
                    map = GetMap(sourceGenericArg, targetGenericArg);

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

        // TODO: when AutoMapper is completely gone these two methods can merge

        public TTarget Map<TSource, TTarget>(TSource source)
            => Map<TSource, TTarget>(source, new MapperContext(this));

        public TTarget Map<TSource, TTarget>(TSource source, Action<MapperContext> f)
        {
            var context = new MapperContext(this);
            f(context);
            return Map<TSource, TTarget>(source, context);
        }

        public TTarget Map<TSource, TTarget>(TSource source, MapperContext context)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var ctor = GetCtor(sourceType, typeof(TTarget));
            var map = GetMap(sourceType, targetType);
            if (ctor != null && map != null)
            {
                var target = ctor(source, context);
                map(source, target, context);
                return (TTarget) target;
            }

            if (sourceType.IsGenericType && targetType.IsGenericType)
            {
                var sourceGeneric = sourceType.GetGenericTypeDefinition();
                var targetGeneric = targetType.GetGenericTypeDefinition();
                var ienumerable = typeof(IEnumerable<>);

                if (sourceGeneric == ienumerable && targetGeneric == ienumerable)
                {
                    var sourceGenericType = sourceType.GenericTypeArguments[0];
                    var targetGenericType = targetType.GenericTypeArguments[0];

                    ctor = GetCtor(sourceGenericType, targetGenericType);
                    map = GetMap(sourceGenericType, targetGenericType);

                    if (ctor != null && map != null)
                    {
                        var sourceEnumerable = (IEnumerable)source;
                        var targetEnumerable = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetGenericType));

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

        public TTarget Map<TSource, TTarget>(TSource source, TTarget target)
            => Map(source, target, new MapperContext(this));

        public TTarget Map<TSource, TTarget>(TSource source, TTarget target, Action<MapperContext> f)
        {
            var context = new MapperContext(this);
            f(context);
            return Map(source, target, context);
        }

        public TTarget Map<TSource, TTarget>(TSource source, TTarget target, MapperContext context)
        {
            // fixme should we deal with enumerables?

            var map = GetMap(source.GetType(), typeof(TTarget));
            if (map == null)
            {
                throw new InvalidOperationException($"Don't know how to map {typeof(TSource).FullName} to {typeof(TTarget).FullName}.");
            }

            map(source, target, context);
            return target;
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

        #endregion
    }
}
