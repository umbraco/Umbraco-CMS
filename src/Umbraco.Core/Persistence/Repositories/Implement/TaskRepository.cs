using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing.CompositionRoots;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class TaskRepository : NPocoRepositoryBase<int, Task>, ITaskRepository
    {
        public TaskRepository(IScopeAccessor scopeAccessor, [Inject(RepositoryCompositionRoot.DisabledCache)] CacheHelper cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override Task PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { id = id });

            var taskDto = Database.Fetch<TaskDto>(SqlContext.SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
            if (taskDto == null)
                return null;
            
            var entity = TaskFactory.BuildEntity(taskDto);
            return entity;
        }

        protected override IEnumerable<Task> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);

            if (ids.Any())
            {
                sql.Where("cmsTask.id IN (@ids)", new { ids = ids });
            }
            
            var dtos = Database.Fetch<TaskDto>(sql);
            return dtos.Select(TaskFactory.BuildEntity);
        }

        protected override IEnumerable<Task> PerformGetByQuery(IQuery<Task> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<Task>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<TaskDto>(sql);
            return dtos.Select(TaskFactory.BuildEntity);
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return isCount ? SqlContext.Sql().SelectCount().From<TaskDto>() : GetBaseQuery();
        }

        private Sql<ISqlContext> GetBaseQuery()
        {
            return SqlContext.Sql()
                .Select("cmsTask.closed,cmsTask.id,cmsTask.taskTypeId,cmsTask.nodeId,cmsTask.parentUserId,cmsTask.userId,cmsTask." + SqlContext.SqlSyntax.GetQuotedColumnName("DateTime") + ",cmsTask.Comment,cmsTaskType.id, cmsTaskType.alias")
                .From<TaskDto>()
                .InnerJoin<TaskTypeDto>()
                .On<TaskDto, TaskTypeDto>(left => left.TaskTypeId, right => right.Id)
                .InnerJoin<NodeDto>()
                .On<TaskDto, NodeDto>(left => left.NodeId, right => right.NodeId);
        }

        protected override string GetBaseWhereClause()
        {
            return "cmsTask.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM cmsTask WHERE id = @id"
                };
            return list;
        }

        protected override Guid NodeObjectTypeId => throw new WontImplementException();

        protected override void PersistNewItem(Task entity)
        {
            entity.AddingEntity();

            //ensure the task type exists
            var taskType = Database.SingleOrDefault<TaskTypeDto>("Where alias = @alias", new { alias = entity.TaskType.Alias });
            if (taskType == null)
            {
                var taskTypeId = Convert.ToInt32(Database.Insert(new TaskTypeDto { Alias = entity.TaskType.Alias }));
                entity.TaskType.Id = taskTypeId;
            }
            else
            {
                entity.TaskType.Id = taskType.Id;
            }
            
            var dto = TaskFactory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(Task entity)
        {
            entity.UpdatingEntity();
            
            var dto = TaskFactory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        public IEnumerable<Task> GetTasks(int? itemId = null, int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false)
        {
            var sql = GetGetTasksQuery(assignedUser, ownerUser, taskTypeAlias, includeClosed);
            if (itemId.HasValue)
            {
                sql.Where<NodeDto>(dto => dto.NodeId == itemId.Value);
            }

            var dtos = Database.Fetch<TaskDto>(sql);
     
            return dtos.Select(TaskFactory.BuildEntity);
        }

        private Sql<ISqlContext> GetGetTasksQuery(int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false)
        {
            var sql = GetBaseQuery(false);

            if (includeClosed == false)
            {
                sql.Where<TaskDto>(dto => dto.Closed == false);
            }
            if (taskTypeAlias.IsNullOrWhiteSpace() == false)
            {
                sql.Where("cmsTaskType.alias = @alias", new { alias = taskTypeAlias });
            }
            if (ownerUser.HasValue)
            {
                sql.Where<TaskDto>(dto => dto.ParentUserId == ownerUser.Value);
            }
            if (assignedUser.HasValue)
            {
                sql.Where<TaskDto>(dto => dto.UserId == assignedUser.Value);
            }
            return sql;
        }
    }
}
