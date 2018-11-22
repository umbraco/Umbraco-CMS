using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class TaskTypeFactory
    {
        public TaskType BuildEntity(TaskTypeDto dto)
        {
            var entity = new TaskType(dto.Alias) {Id = dto.Id};
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            entity.ResetDirtyProperties(false);
            return entity;
        }

        public TaskTypeDto BuildDto(TaskType entity)
        {
            var dto = new TaskTypeDto
            {
                Id = (byte)entity.Id,
                Alias = entity.Alias
            };
            return dto;
        }
    }
}