using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    /// <summary>
    /// Creates the model mappings for Tasks
    /// </summary>
    internal class TaskFactory
    {
        public Task BuildEntity(TaskDto dto)
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

        public TaskDto BuildDto(Task entity)
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