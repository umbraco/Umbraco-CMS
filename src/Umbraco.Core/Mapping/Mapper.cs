using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Mapping
{
    // FIXME we should inject the mapper
    // FIXME in order to transition, this should also handle AutoMapper?
    // FIXME we might have to manage a 'context' for some contextual mappings?
    // FIXME we have an infinite loop problem w/ logging in due to mapping issues
    // FIXME refactor
    //  ctor: (source, context) =>
    //  map:  (source, target, context) =>
    // and context.Mapper is mapper

    public class Mapper
    {
        private readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> _ctors = new Dictionary<Type, Dictionary<Type, Func<object, object>>>();
        private readonly Dictionary<Type, Dictionary<Type, Action<object, object>>> _maps = new Dictionary<Type, Dictionary<Type, Action<object, object>>>();

        public Mapper(MapperProfileCollection profiles)
        {
            foreach (var profile in profiles)
                profile.SetMaps(this);
        }

        public void Define<TSource, TTarget>()
            => Define<TSource, TTarget>((source, target) => { });

        public void Define<TSource, TTarget>(Action<TSource, TTarget> map)
            => Define(source => throw new NotSupportedException($"Don't know how to create {typeof(TTarget)} instances."), map);

        public void Define<TSource, TTarget>(Func<TSource, TTarget> ctor)
            => Define(ctor, (source, target) => { });

        public void Define<TSource, TTarget>(Func<TSource, TTarget> ctor, Action<TSource, TTarget> map)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            if (!_ctors.TryGetValue(sourceType, out var sourceCtor))
                sourceCtor = _ctors[sourceType] = new Dictionary<Type, Func<object, object>>();

            sourceCtor[targetType] = source => ctor((TSource) source);

            if (!_maps.TryGetValue(sourceType, out var sourceMap))
                sourceMap = _maps[sourceType] = new Dictionary<Type, Action<object, object>>();

            sourceMap[targetType] = (source, target) => map((TSource) source, (TTarget) target);
        }

        public TTarget Map<TTarget>(object source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sourceType = source.GetType();
            var targetType = typeof(TTarget);

            var ctor = GetCtor(sourceType, typeof(TTarget));
            var map = GetMap(sourceType, typeof(TTarget));
            if (ctor != null && map != null)
            {
                var target = ctor(source);
                map(source, target);
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
                            var targetItem = ctor(sourceItem);
                            map(sourceItem, targetItem);
                            targetEnumerable.Add(targetItem);
                        }

                        return (TTarget)targetEnumerable;
                    }

                    // fixme - temp
                    return AutoMapper.Mapper.Map<TTarget>(source);
                }
            }

            // fixme this is temp
            //throw new InvalidOperationException($"Don't know how to map {sourceType.FullName} to {targetType.FullName}.");
            return AutoMapper.Mapper.Map<TTarget>(source);
        }

        // TODO: when AutoMapper is completely gone these two methods can merge

        public TTarget Map<TSource, TTarget>(TSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var ctor = GetCtor(sourceType, typeof(TTarget));
            var map = GetMap(sourceType, targetType);
            if (ctor != null && map != null)
            {
                var target = ctor(source);
                map(source, target);
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
                            var targetItem = ctor(sourceItem);
                            map(sourceItem, targetItem);
                            targetEnumerable.Add(targetItem);
                        }

                        return (TTarget)targetEnumerable;
                    }

                    // fixme - temp
                    return AutoMapper.Mapper.Map<TSource, TTarget>(source);
                }
            }

            // fixme this is temp
            //throw new InvalidOperationException($"Don't know how to map {sourceType.FullName} to {targetType.FullName}.");
            return AutoMapper.Mapper.Map<TSource, TTarget>(source);
        }

        public TTarget Map<TSource, TTarget>(TSource source, TTarget target)
        {
            // fixme should we deal with enumerables?

            var map = GetMap(source.GetType(), typeof(TTarget));
            if (map == null)
            {
                // fixme this is temp
                //throw new InvalidOperationException($"Don't know how to map {sourceType.FullName} to {targetType.FullName}.");
                return AutoMapper.Mapper.Map(source, target);
            }

            map(source, target);
            return target;
        }

        private Func<object, object> GetCtor(Type sourceType, Type targetType)
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

        private Action<object, object> GetMap(Type sourceType, Type targetType)
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
    }
}
