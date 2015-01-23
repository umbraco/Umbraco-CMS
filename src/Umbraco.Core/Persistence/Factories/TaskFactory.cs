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
            var entity = new Task(new TaskType(dto.TaskTypeDto.Alias) { Id = dto.TaskTypeDto.Id })
            {
                Closed = dto.Closed,
                AssigneeUserId = dto.UserId,
                Comment = dto.Comment,
                CreateDate = dto.DateTime,
                EntityId = dto.NodeId,
                Id = dto.Id,
                OwnerUserId = dto.ParentUserId,
            };
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            entity.ResetDirtyProperties(false);
            return entity;
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