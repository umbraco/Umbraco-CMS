using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class RelationFactory 
    {
        private readonly IRelationType _relationType;

        public RelationFactory(IRelationType relationType)
        {
            _relationType = relationType;
        }

        #region Implementation of IEntityFactory<Relation,RelationDto>

        public IRelation BuildEntity(RelationDto dto)
        {
            var entity = new Relation(dto.ParentId, dto.ChildId, _relationType);

            try
            {
                entity.DisableChangeTracking();

                entity.Comment = dto.Comment;
                entity.CreateDate = dto.Datetime;
                entity.Id = dto.Id;
                entity.UpdateDate = dto.Datetime;

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

        public RelationDto BuildDto(IRelation entity)
        {
            var dto = new RelationDto
            {
                ChildId = entity.ChildId,
                Comment = string.IsNullOrEmpty(entity.Comment) ? string.Empty : entity.Comment,
                Datetime = entity.CreateDate,
                ParentId = entity.ParentId,
                RelationType = entity.RelationType.Id
            };

            if (entity.HasIdentity)
                dto.Id = entity.Id;

            return dto;
        }

        #endregion
    }
}