using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;
using Umbraco.Infrastructure.Persistence.Repositories;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class KeyValueRepository : NPocoRepositoryBase<string, IKeyValue>, IKeyValueRepository
    {
        public KeyValueRepository(IScopeAccessor scopeAccessor, ILogger logger)
            : base(scopeAccessor, AppCaches.NoCache, logger)
        { }

        public void Initialize()
        {
            var context = new MigrationContext(Database, Logger);
            var initMigration = new InitialMigration(context);
            initMigration.Migrate();
        }

        public string GetValue(string key)
        {
            return GetDtoByKey(key)?.Value;
        }

        public void SetValue(string key, string value)
        {
            var dto = GetDtoByKey(key);
            if (dto == null)
            {
                dto = new KeyValueDto
                {
                    Key = key,
                    Value = value,
                    UpdateDate = DateTime.Now
                };

                Database.Insert(dto);
            }
            else
            {
                UpdateDtoValue(dto, value);
            }
        }

        public bool TrySetValue(string key, string originalValue, string newValue)
        {
            var dto = GetDtoByKey(key);

            if (dto == null || dto.Value != originalValue)
                return false;

            UpdateDtoValue(dto, newValue);
            return true;
        }

        private void UpdateDtoValue(KeyValueDto dto, string value)
        {
            dto.Value = value;
            dto.UpdateDate = DateTime.Now;
            Database.Update(dto);
        }

        /// <summary>
        /// Gets a value directly from the database, no scope, nothing.
        /// </summary>
        /// <remarks>Used by <see cref="Runtime.CoreRuntime"/> to determine the runtime state.</remarks>
        internal static string GetValue(IUmbracoDatabase database, string key)
        {
            if (database is null) return null;

            var sql = database.SqlContext.Sql()
                .Select<KeyValueDto>()
                .From<KeyValueDto>()
                .Where<KeyValueDto>(x => x.Key == key);
            return database.FirstOrDefault<KeyValueDto>(sql)?.Value;
        }

        #region Overrides of NPocoRepositoryBase<string, IKeyValue>

        protected override Guid NodeObjectTypeId => throw new NotImplementedException();

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = SqlContext.Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<KeyValueDto>();

            sql
                .From<KeyValueDto>();

            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return Constants.DatabaseSchema.Tables.KeyValue + ".key = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            return new List<string>();
        }

        protected override IKeyValue PerformGet(string id)
        {
            var dto = GetDtoByKey(id);
            return dto == null ? null : Map(dto);
        }

        private KeyValueDto GetDtoByKey(string key)
        {
            var sql = GetBaseQuery(false).Where<KeyValueDto>(x => x.Key == key);
            return Database.Fetch<KeyValueDto>(sql).FirstOrDefault();
        }

        protected override IEnumerable<IKeyValue> PerformGetAll(params string[] ids)
        {
            var sql = GetBaseQuery(false).WhereIn<KeyValueDto>(x => x.Key, ids);
            var dtos = Database.Fetch<KeyValueDto>(sql);
            return dtos.WhereNotNull().Select(Map);
        }

        protected override IEnumerable<IKeyValue> PerformGetByQuery(IQuery<IKeyValue> query)
        {
            throw new NotImplementedException();
        }

        protected override void PersistNewItem(IKeyValue entity)
        {
            var dto = Map(entity);
            Database.Insert(dto);
        }

        protected override void PersistUpdatedItem(IKeyValue entity)
        {
            var dto = Map(entity);
            Database.Update(dto);
        }

        private static KeyValueDto Map(IKeyValue keyValue)
        {
            if (keyValue == null) return null;

            return new KeyValueDto
            {
                Key = keyValue.Identifier,
                Value = keyValue.Value,
                UpdateDate = keyValue.UpdateDate,
            };
        }

        private static IKeyValue Map(KeyValueDto dto)
        {
            if (dto == null) return null;

            return new KeyValue
            {
                Identifier = dto.Key,
                Value = dto.Value,
                UpdateDate = dto.UpdateDate,
            };
        }

        #endregion


        /// <summary>
        /// A custom migration that executes standalone during the Initialize phase of the KeyValueService.
        /// </summary>
        internal class InitialMigration : MigrationBase
        {
            public InitialMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // as long as we are still running 7 this migration will be invoked,
                // but due to multiple restarts during upgrades, maybe the table
                // exists already
                if (TableExists(Constants.DatabaseSchema.Tables.KeyValue))
                    return;

                Logger.Info<KeyValueRepository>("Creating KeyValue structure.");

                // the locks table was initially created with an identity (auto-increment) primary key,
                // but we don't want this, especially as we are about to insert a new row into the table,
                // so here we drop that identity
                DropLockTableIdentity();

                // insert the lock object for key/value
                Insert.IntoTable(Constants.DatabaseSchema.Tables.Lock).Row(new { id = Constants.Locks.KeyValues, name = "KeyValues", value = 1 }).Do();

                // create the key-value table
                Create.Table<KeyValueDto>().Do();
            }

            private void DropLockTableIdentity()
            {
                // one cannot simply drop an identity, that requires a bit of work

                // create a temp. id column and copy values
                Alter.Table(Constants.DatabaseSchema.Tables.Lock).AddColumn("nid").AsInt32().Nullable().Do();
                Execute.Sql("update umbracoLock set nid = id").Do();

                // drop the id column entirely (cannot just drop identity)
                Delete.PrimaryKey("PK_umbracoLock").FromTable(Constants.DatabaseSchema.Tables.Lock).Do();
                Delete.Column("id").FromTable(Constants.DatabaseSchema.Tables.Lock).Do();

                // recreate the id column without identity and copy values
                Alter.Table(Constants.DatabaseSchema.Tables.Lock).AddColumn("id").AsInt32().Nullable().Do();
                Execute.Sql("update umbracoLock set id = nid").Do();

                // drop the temp. id column
                Delete.Column("nid").FromTable(Constants.DatabaseSchema.Tables.Lock).Do();

                // complete the primary key
                Alter.Table(Constants.DatabaseSchema.Tables.Lock).AlterColumn("id").AsInt32().NotNullable().PrimaryKey("PK_umbracoLock").Do();
            }
        }
    }
}
