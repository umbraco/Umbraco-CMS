using System;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Models.Entities
{
    [UmbracoVolatile]
    public static class EntityExtensions
    {
        /// <summary>
        /// Updates the entity when it is being saved.
        /// </summary>
        public static void UpdatingEntity(this IEntity entity)
        {
            var now = DateTime.Now;

            if (entity.CreateDate == default)
            {
                entity.CreateDate = now;
            }

            // set the update date if not already set
            if (entity.UpdateDate == default || (entity is ICanBeDirty canBeDirty && canBeDirty.IsPropertyDirty("UpdateDate") == false))
            {
                entity.UpdateDate = now;
            }
        }

        /// <summary>
        /// Updates the entity when it is being saved for the first time.
        /// </summary>
        public static void AddingEntity(this IEntity entity)
        {
            var now = DateTime.Now;
            var canBeDirty = entity as ICanBeDirty;

            // set the create and update dates, if not already set
            if (entity.CreateDate == default || canBeDirty?.IsPropertyDirty("CreateDate") == false)
            {
                entity.CreateDate = now;
            }
            if (entity.UpdateDate == default || canBeDirty?.IsPropertyDirty("UpdateDate") == false)
            {
                entity.UpdateDate = now;
            }
        }
    }
}
