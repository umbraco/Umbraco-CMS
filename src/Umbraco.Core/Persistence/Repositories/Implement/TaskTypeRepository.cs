using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class TaskTypeRepository : NPocoRepositoryBase<int, TaskType>, ITaskTypeRepository
    {
        public TaskTypeRepository(IScopeAccessor scopeAccessor, CacheHelper cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override TaskType PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var taskDto = Database.Fetch<TaskTypeDto>(SqlContext.SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
            if (taskDto == null)
                return null;
            
            var entity = TaskTypeFactory.BuildEntity(taskDto);
            return entity;
        }

        protected override IEnumerable<TaskType> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);

            if (ids.Any())
            {
                sql.Where("cmsTaskType.id IN (@ids)", new { ids });
            }
            
            var dtos = Database.Fetch<TaskTypeDto>(sql);
            return dtos.Select(TaskTypeFactory.BuildEntity);
        }

        protected override IEnumerable<TaskType> PerformGetByQuery(IQuery<TaskType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<TaskType>(sqlClause, query);
            var sql = translator.Translate();
            
            var dtos = Database.Fetch<TaskTypeDto>(sql);
            return dtos.Select(TaskTypeFactory.BuildEntity);
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return SqlContext.Sql().SelectAll().From<TaskTypeDto>();
        }

        protected override string GetBaseWhereClause()
        {
            return "cmsTaskType.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
            {
                "DELETE FROM cmsTask WHERE taskTypeId = @id",
                "DELETE FROM cmsTaskType WHERE id = @id"
            };
            return list;
        }

        protected override Guid NodeObjectTypeId => throw new NotImplementedException();

        protected override void PersistNewItem(TaskType entity)
        {
            entity.AddingEntity();

            //TODO: Just remove the task type db table or add a unique index to the alias

            //ensure the task type exists
            var taskType = Database.SingleOrDefault<TaskTypeDto>("Where alias = @alias", new { alias = entity.Alias });
            if (taskType != null)
            {
                throw new InvalidOperationException("A task type already exists with the given alias " + entity.Alias);
            }
            
            var dto = TaskTypeFactory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(TaskType entity)
        {
            entity.UpdatingEntity();
            
            var dto = TaskTypeFactory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }
    }
}
