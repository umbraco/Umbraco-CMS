using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Core.Persistence.Mappers
{
    public class MapperCollection : BuilderCollectionBase<BaseMapper>, IMapperCollection
    {
        public MapperCollection(IEnumerable<BaseMapper> items)
            : base(items)
        {

            _index = new Lazy<ConcurrentDictionary<Type, BaseMapper>>(() =>
            {
                var d = new ConcurrentDictionary<Type, BaseMapper>();
                foreach(var mapper in this)
                {
                    var attributes = mapper.GetType().GetCustomAttributes<MapperForAttribute>(false);
                    foreach(var a in attributes)
                    {
                        d.TryAdd(a.EntityType, mapper);
                    }
                }
                return d;
            });
        }

        private readonly Lazy<ConcurrentDictionary<Type, BaseMapper>> _index;

        /// <summary>
        /// Returns a mapper for this type, throw an exception if not found
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public BaseMapper this[Type type]
        {
            get
            {
                if (_index.Value.TryGetValue(type, out var mapper))
                    return mapper;
                throw new Exception($"Could not find a mapper matching type {type.FullName}.");
            }
        }

        public bool TryGetMapper(Type type, out BaseMapper mapper) => _index.Value.TryGetValue(type, out mapper);
    }
}
