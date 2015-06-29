using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class TaskTypeRepository : PetaPocoRepositoryBase<int, TaskType>, ITaskTypeRepository
    {
        public TaskTypeRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        protected override TaskType PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var taskDto = Database.Fetch<TaskTypeDto>(sql).FirstOrDefault();
            if (taskDto == null)
                return null;

            var factory = new TaskTypeFactory();
            var entity = factory.BuildEntity(taskDto);
            return entity;
        }

        protected override IEnumerable<TaskType> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);

            if (ids.Any())
            {
                sql.Where("cmsTaskType.id IN (@ids)", new { ids = ids });
            }

            var factory = new TaskTypeFactory();
            var dtos = Database.Fetch<TaskTypeDto>(sql);
            return dtos.Select(factory.BuildEntity);
        }

        protected override IEnumerable<TaskType> PerformGetByQuery(IQuery<TaskType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<TaskType>(sqlClause, query);
            var sql = translator.Translate();

            var factory = new TaskTypeFactory();
            var dtos = Database.Fetch<TaskTypeDto>(sql);
            return dtos.Select(factory.BuildEntity);
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select("*").From<TaskTypeDto>(SqlSyntax);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "cmsTaskType.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
            {
                "DELETE FROM cmsTask WHERE taskTypeId = @Id",
                "DELETE FROM cmsTaskType WHERE id = @Id"
            };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

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

            var factory = new TaskTypeFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(TaskType entity)
        {
            entity.UpdatingEntity();

            var factory = new TaskTypeFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }
    }
}