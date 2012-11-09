using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class RelationFactory : IEntityFactory<Relation, RelationDto>
    {
        private readonly RelationType _relationType;

        public RelationFactory(RelationType relationType)
        {
            _relationType = relationType;
        }

        #region Implementation of IEntityFactory<Relation,RelationDto>

        public Relation BuildEntity(RelationDto dto)
        {
            var entity = new Relation(dto.ParentId, dto.ChildId)
                             {
                                 Comment = dto.Comment,
                                 CreateDate = dto.Datetime,
                                 Id = dto.Id,
                                 UpdateDate = dto.Datetime,
                                 RelationType = _relationType
                             };
            return entity;
        }

        public RelationDto BuildDto(Relation entity)
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