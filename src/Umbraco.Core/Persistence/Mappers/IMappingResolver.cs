using System;

namespace Umbraco.Core.Persistence.Mappers
{
    public interface IMappingResolver
    {
        /// <summary>
        /// Return a mapper by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        BaseMapper ResolveMapperByType(Type type);
    }
}