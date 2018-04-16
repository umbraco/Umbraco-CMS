using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal class TaskTypeFactory
    {
        public TaskType BuildEntity(TaskTypeDto dto)
        {
            var entity = new TaskType(dto.Alias) {Id = dto.Id};
            // reset dirty initial properties (U4-1946)
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
