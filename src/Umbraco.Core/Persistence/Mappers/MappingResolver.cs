using System;
using System.Collections.Concurrent;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Persistence.Mappers
{
    internal static class MappingResolver
    {
        /// <summary>
        /// Caches the type -> mapper so that we don't have to type check each time we want one or lookup the attribute
        /// </summary>
        private static readonly ConcurrentDictionary<Type, BaseMapper> MapperCache = new ConcurrentDictionary<Type, BaseMapper>();

        /// <summary>
        /// Return a mapper by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static BaseMapper ResolveMapperByType(Type type)
        {
            return MapperCache.GetOrAdd(type, type1 =>
                {
                    var mappers = TypeFinder.FindClassesOfTypeWithAttribute<BaseMapper, MapperAttribute>();

                    //first check if we can resolve it by attribute

                    var byAttribute = TryGetMapperByAttribute(type);
                    if (byAttribute.Success)
                    {
                        return byAttribute.Result;
                    }

                    //static mapper registration if not using attributes, could be something like this:
                    //if (type == typeof (UserType))
                    //    return new UserTypeMapper();

                    throw new Exception("Invalid Type: A Mapper could not be resolved based on the passed in Type");
                });
        }

        /// <summary>
        /// Check the entity type to see if it has a mapper attribute assigned and try to instantiate it
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Attempt<BaseMapper> TryGetMapperByAttribute(Type type)
        {
            var attribute = type.GetCustomAttribute<MapperAttribute>(false);
            if (attribute == null)
            {
                return Attempt<BaseMapper>.False;
            }
            try
            {
                var instance = Activator.CreateInstance(attribute.MapperType) as BaseMapper;
                return instance != null 
                    ? new Attempt<BaseMapper>(true, instance) 
                    : Attempt<BaseMapper>.False;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(MappingResolver), "Could not instantiate mapper of type " + attribute.MapperType, ex);
                return new Attempt<BaseMapper>(ex);
            }
        }  

        internal static string GetMapping(Type type, string propertyName)
        {
            var mapper = ResolveMapperByType(type);
            var result = mapper.Map(propertyName);
            if(string.IsNullOrEmpty(result))
                throw new Exception("Invalid mapping: The passed in property name could not be mapped using the passed in type");

            return result;
        }
    }
}