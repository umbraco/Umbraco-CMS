using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Mapping
{
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

            var sourceType = source.GetType();
            var targetType = typeof(TTarget);

            if (!_maps.TryGetValue(sourceType, out var sourceMap))
            {
                var type = _maps.Keys.FirstOrDefault(x => x.IsAssignableFrom(sourceType));
                if (type == null)
                    throw new InvalidOperationException($"Don't know how to map {sourceType.FullName} to {targetType.FullName}.");
                sourceMap = _maps[sourceType] = _maps[type];
            }

            if (!sourceMap.TryGetValue(targetType, out var map))
                throw new InvalidOperationException($"Don't know how to map {sourceType.FullName} to {targetType.FullName}.");

            return (TTarget) map(source);
        }
    }
}
