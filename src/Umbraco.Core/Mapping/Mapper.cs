using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Mapping
{
    // FIXME we should inject the mapper
    // FIXME in order to transition, this should also handle AutoMapper?
    // FIXME we might have to manage a 'context' for some contextual mappings?
    // FIXME we have an infinite loop problem w/ logging in due to mapping issues

    public class Mapper
    {
        private readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> _maps = new Dictionary<Type, Dictionary<Type, Func<object, object>>>();

        public Mapper(MapperProfileCollection profiles)
        {
            foreach (var profile in profiles)
                profile.SetMaps(this);
        }

        public void SetMap<TSource, TTarget>(Func<TSource, TTarget> map)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            if (!_maps.TryGetValue(sourceType, out var sourceMap))
                sourceMap = _maps[sourceType] = new Dictionary<Type, Func<object, object>>();

            sourceMap[targetType] = o => map((TSource)o);
        }

        public TTarget Map<TTarget>(object source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var map = GetMap(source.GetType(), typeof(TTarget));
            if (map == null)
            {
                // fixme this is temp
                //throw new InvalidOperationException($"Don't know how to map {sourceType.FullName} to {targetType.FullName}.");
                return AutoMapper.Mapper.Map<TTarget>(source);
            }

            return (TTarget) map(source);
        }

        private Func<object, object> GetMap(Type sourceType, Type targetType)
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

        public TTarget Map<TSource, TTarget>(TSource source)
        {
            return AutoMapper.Mapper.Map<TSource, TTarget>(source);

            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var map = GetMap(sourceType, targetType);
            if (map != null)
                return (TTarget) map(source);

            if (sourceType.IsGenericType && targetType.IsGenericType)
            {
                var sourceGeneric = sourceType.GetGenericTypeDefinition();
                var targetGeneric = targetType.GetGenericTypeDefinition();
                var ienumerable = typeof(IEnumerable<>);

                if (sourceGeneric == ienumerable && targetGeneric == ienumerable)
                {
                    var sourceGenericType = sourceGeneric.GetGenericArguments()[0];
                    var targetGenericType = targetGeneric.GetGenericArguments()[0];
                    map = GetMap(sourceGenericType, targetGenericType);
                    // fixme - how can we enumerate, generically?
                }
            }

            // fixme this is temp
            //throw new InvalidOperationException($"Don't know how to map {sourceType.FullName} to {targetType.FullName}.");
            return AutoMapper.Mapper.Map<TSource, TTarget>(source);
        }

        public TTarget Map<TSource, TTarget>(TSource source, TTarget target)
        {
            return AutoMapper.Mapper.Map(source, target); // fixme what does this do exactly?
        }
    }
}
