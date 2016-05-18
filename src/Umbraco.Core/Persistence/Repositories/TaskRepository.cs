using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using LightInject;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class TaskRepository : NPocoRepositoryBase<int, Task>, ITaskRepository
    {
        public TaskRepository(IDatabaseUnitOfWork work, [Inject(RepositoryCompositionRoot.DisabledCache)] CacheHelper cache, ILogger logger, IMappingResolver mappingResolver)
            : base(work, cache, logger, mappingResolver)
        {
        }

        protected override Task PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var taskDto = Database.Fetch<TaskDto>(sql).FirstOrDefault();
            if (taskDto == null)
                return null;

            var factory = new TaskFactory();
            var entity = factory.BuildEntity(taskDto);
            return entity;
        }

        protected override IEnumerable<Task> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);

            if (ids.Any())
            {
                sql.Where("cmsTask.id IN (@ids)", new { ids = ids });
            }

            var factory = new TaskFactory();
            var dtos = Database.Fetch<TaskDto>(sql);
            return dtos.Select(factory.BuildEntity);
        }

        protected override IEnumerable<Task> PerformGetByQuery(IQuery<Task> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<Task>(sqlClause, query);
            var sql = translator.Translate();

            var factory = new TaskFactory();
            var dtos = Database.Fetch<TaskDto>(sql);
            return dtos.Select(factory.BuildEntity);
        }

        protected override Sql<SqlContext> GetBaseQuery(bool isCount)
        {
            return isCount ? Sql().SelectCount().From<TaskDto>() : GetBaseQuery();
        }

        private Sql<SqlContext> GetBaseQuery()
        {
            return Sql()
                .Select("cmsTask.closed,cmsTask.id,cmsTask.taskTypeId,cmsTask.nodeId,cmsTask.parentUserId,cmsTask.userId,cmsTask." + SqlSyntax.GetQuotedColumnName("DateTime") + ",cmsTask.Comment,cmsTaskType.id, cmsTaskType.alias")
                .From<TaskDto>()
                .InnerJoin<TaskTypeDto>()
                .On<TaskDto, TaskTypeDto>(left => left.TaskTypeId, right => right.Id)
                .InnerJoin<NodeDto>()
                .On<TaskDto, NodeDto>(left => left.NodeId, right => right.NodeId);
        }

        protected override string GetBaseWhereClause()
        {
            return "cmsTask.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM cmsTask WHERE id = @Id"
                };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

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

            var factory = new TaskFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(Task entity)
        {
            entity.UpdatingEntity();

            var factory = new TaskFactory();
            var dto = factory.BuildDto(entity);

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
            var factory = new TaskFactory();
            return dtos.Select(factory.BuildEntity);
        }

        private Sql<SqlContext> GetGetTasksQuery(int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false)
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