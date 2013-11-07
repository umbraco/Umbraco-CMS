using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class TagFactory : IEntityFactory<ITag, TagDto>
    {
        public ITag BuildEntity(TagDto dto)
        {
            var model = new Tag(dto.Id, dto.Tag, dto.Group);            
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            model.ResetDirtyProperties(false);
            return model;
        }

        public TagDto BuildDto(ITag entity)
        {
            return new TagDto
                {
                    Id = entity.Id,
                    Group = entity.Group,
                    Tag = entity.Text
                };
        }
    }
}