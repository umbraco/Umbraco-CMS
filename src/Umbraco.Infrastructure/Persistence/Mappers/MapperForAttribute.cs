using System;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers
{
    /// <summary>
    /// An attribute used to decorate mappers to be associated with entities
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class MapperForAttribute : Attribute
    {
        public Type EntityType { get; private set; }

        public MapperForAttribute(Type entityType) => EntityType = entityType;
    }

}
