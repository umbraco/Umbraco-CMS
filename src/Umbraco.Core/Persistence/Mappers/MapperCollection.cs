using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Persistence.Mappers
{
    public class MapperCollection : BuilderCollectionBase<BaseMapper>, IMapperCollection
    {
        public MapperCollection(IEnumerable<BaseMapper> items)
            : base(items)
        { }

        // maintain our own index for faster lookup
        private readonly ConcurrentDictionary<Type, BaseMapper> _index = new ConcurrentDictionary<Type, BaseMapper>();

        public BaseMapper this[Type type]
        {
            get
            {
                return _index.GetOrAdd(type, t =>
                {
                    // check if any of the mappers are assigned to this type
                    var mapper = this.FirstOrDefault(x => x.GetType()
                        .GetCustomAttributes<MapperForAttribute>(false)
                        .Any(m => m.EntityType == type));

                    if (mapper != null) return mapper;

                    throw new Exception($"Could not find a mapper matching type {type.FullName}.");
                });
            }
        }
    }
}
