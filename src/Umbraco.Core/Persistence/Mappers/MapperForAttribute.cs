using System;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// An attribute used to decorate mappers to be associated with entities
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class MapperForAttribute : Attribute
    {
        public Type EntityType { get; private set; }

        public MapperForAttribute(Type entityType)
        {
            EntityType = entityType;
        }
    }

}
