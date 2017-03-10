using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class RelationTypeFactory 
    {
        #region Implementation of IEntityFactory<RelationType,RelationTypeDto>

        public IRelationType BuildEntity(RelationTypeDto dto)
        {
            var entity = new RelationType(dto.ChildObjectType, dto.ParentObjectType, dto.Alias);

            try
            {
                entity.DisableChangeTracking();

                entity.Id = dto.Id;
                entity.IsBidirectional = dto.Dual;
                entity.Name = dto.Name;

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                entity.ResetDirtyProperties(false);
                return entity;
            }
            finally
            {
                entity.EnableChangeTracking();
            }
        }

        public RelationTypeDto BuildDto(IRelationType entity)
        {
            var dto = new RelationTypeDto
            {
                Alias = entity.Alias,
                ChildObjectType = entity.ChildObjectType,
                Dual = entity.IsBidirectional,
                Name = entity.Name,
                ParentObjectType = entity.ParentObjectType
            };
            if (entity.HasIdentity)
                dto.Id = entity.Id;

            return dto;
        }

        #endregion
    }
}