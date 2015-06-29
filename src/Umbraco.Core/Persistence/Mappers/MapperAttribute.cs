using System;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// An attribute used to decorate entities in order to associate with a mapper
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    internal sealed class MapperAttribute : Attribute
    {
        public Type MapperType { get; private set; }

        public MapperAttribute(Type mapperType)
        {
            MapperType = mapperType;
        }
    }
}