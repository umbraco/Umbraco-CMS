using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Persistence.Mappers
{
    internal class MappingResolver : LazyManyObjectsResolverBase<MappingResolver, BaseMapper>
    {
        /// <summary>
        /// Constructor accepting a list of BaseMapper types that are attributed with the MapperFor attribute
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="assignedMapperTypes"></param>
        public MappingResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> assignedMapperTypes)
            : base(serviceProvider, logger, assignedMapperTypes)
        {
            
        }

        /// <summary>
        /// Caches the type -> mapper so that we don't have to type check each time we want one or lookup the attribute
        /// </summary>
        private readonly ConcurrentDictionary<Type, BaseMapper> _mapperCache = new ConcurrentDictionary<Type, BaseMapper>();

        /// <summary>
        /// Return a mapper by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal BaseMapper ResolveMapperByType(Type type)
        {
            return _mapperCache.GetOrAdd(type, type1 =>
                {
                    
                    //first check if we can resolve it by attribute

                    var byAttribute = TryGetMapperByAttribute(type);
                    if (byAttribute.Success)
                    {
                        return byAttribute.Result;
                    }
                    throw new Exception("Invalid Type: A Mapper could not be resolved based on the passed in Type");
                });
        }

        /// <summary>
        /// Check the entity type to see if it has a mapper attribute assigned and try to instantiate it
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private Attempt<BaseMapper> TryGetMapperByAttribute(Type entityType)
        {            
            //check if any of the mappers are assigned to this type
            var mapper = Values.FirstOrDefault(
                x => x.GetType().GetCustomAttributes<MapperForAttribute>(false)
                      .Any(m => m.EntityType == entityType));

            if (mapper == null)
            {
                return Attempt<BaseMapper>.Fail();
            }

            return Attempt<BaseMapper>.Succeed(mapper);
        }  

        internal string GetMapping(Type type, string propertyName)
        {
            var mapper = ResolveMapperByType(type);
            var result = mapper.Map(propertyName);
            if(string.IsNullOrEmpty(result))
                throw new Exception("Invalid mapping: The passed in property name could not be mapped using the passed in type");

            return result;
        }

    }

}