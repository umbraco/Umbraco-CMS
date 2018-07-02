using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    /// <summary>
    /// Creates the model mappings for Tasks
    /// </summary>
    internal static class TaskFactory
    {
        public static Task BuildEntity(TaskDto dto)
        {
            var entity = new Task(new TaskType(dto.TaskTypeDto.Alias) { Id = dto.TaskTypeDto.Id });

            try
            {
                entity.DisableChangeTracking();

                entity.Closed = dto.Closed;
                entity.AssigneeUserId = dto.UserId;
                entity.Comment = dto.Comment;
                entity.CreateDate = dto.DateTime;
                entity.EntityId = dto.NodeId;
                entity.Id = dto.Id;
                entity.OwnerUserId = dto.ParentUserId;

                // reset dirty initial properties (U4-1946)
                entity.ResetDirtyProperties(false);
                return entity;
            }
            finally
            {
                entity.EnableChangeTracking();
            }
        }

        public static TaskDto BuildDto(Task entity)
        {
            var dto = new TaskDto
            {
                Closed = entity.Closed,
                Comment = entity.Comment,
                DateTime = entity.CreateDate,
                Id = entity.Id,
                NodeId = entity.EntityId,
                ParentUserId = entity.OwnerUserId,
                TaskTypeId = (byte)entity.TaskType.Id,
                UserId = entity.AssigneeUserId
            };

            return dto;
        }
    }
}
