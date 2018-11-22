using System.Linq;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public static class EntityExtensions
    {

        /// <summary>
        /// Returns true if this entity has just been created and persisted to the data store
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is useful when handling events to determine if an entity is a brand new entity or was
        /// already existing.
        /// </remarks>
        public static bool IsNewEntity(this IEntity entity)
        {
            var dirty = (IRememberBeingDirty)entity;
            return dirty.WasPropertyDirty("Id");
        }
    }
}