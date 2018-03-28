using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Expressions.Create;
using Umbraco.Core.Scoping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Services.Implement
{
    internal class KeyValueService : IKeyValueService
    {
        private readonly object _initialock = new object();
        private readonly IScopeProvider _scopeProvider;
        private readonly ILogger _logger;
        private bool _initialized;

        public KeyValueService(IScopeProvider scopeProvider, ILogger logger)
        {
            _scopeProvider = scopeProvider;
            _logger = logger;
        }

        private void EnsureInitialized()
        {
            lock (_initialock)
            {
                if (_initialized) return;
                Initialize();
                _initialized = true;
            }
        }

        private void Initialize()
        {
            // all this cannot be achieved via default migrations since it needs to run
            // before any migration, in order to figure out migrations, ironically we are using a custom migration to do this

            using (var scope = _scopeProvider.CreateScope())
            {
                // assume that if the lock object for key/value exists, then everything is ok
                if (scope.Database.Exists<LockDto>(Constants.Locks.KeyValues))
                {
                    scope.Complete();
                    return;
                }

                var context = new MigrationContext(scope.Database, _logger);
                var initMigration = new InitializeMigration(context);
                initMigration.Migrate();

                scope.Complete();
            }
        }

        /// <summary>
        /// A custom migration that executes standalone during the Initialize phase of this service
        /// </summary>
        private class InitializeMigration : MigrationBase
        {
            public InitializeMigration(IMigrationContext context) : base(context)
            {
            }

            public override void Migrate()
            {
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
                
                // insert the key-value lock
                Insert.IntoTable(Constants.DatabaseSchema.Tables.Lock).Row(new {id = Constants.Locks.KeyValues, name = "KeyValues", value = 1}).Do();                

                // create the key-value table if it's not there
                if (TableExists(Constants.DatabaseSchema.Tables.KeyValue) == false)
                    Create.Table<KeyValueDto>().Do();
            }
        }

        /// <inheritdoc />
        public string GetValue(string key)
        {
            EnsureInitialized();

            using (var scope = _scopeProvider.CreateScope())
            {
                var sql = scope.SqlContext.Sql().Select<KeyValueDto>().From<KeyValueDto>().Where<KeyValueDto>(x => x.Key == key);
                var dto = scope.Database.Fetch<KeyValueDto>(sql).FirstOrDefault();
                scope.Complete();
                return dto?.Value;
            }
        }

        /// <inheritdoc />
        public void SetValue(string key, string value)
        {
            EnsureInitialized();

            using (var scope = _scopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.KeyValues);

                var sql = scope.SqlContext.Sql().Select<KeyValueDto>().From<KeyValueDto>().Where<KeyValueDto>(x => x.Key == key);
                var dto = scope.Database.Fetch<KeyValueDto>(sql).FirstOrDefault();

                if (dto == null)
                {
                    dto = new KeyValueDto
                    {
                        Key = key,
                        Value = value,
                        Updated = DateTime.Now
                    };

                    scope.Database.Insert(dto);
                }
                else
                {
                    dto.Value = value;
                    dto.Updated = DateTime.Now;
                    scope.Database.Update(dto);
                }

                scope.Complete();
            }
        }

        /// <inheritdoc />
        public void SetValue(string key, string originValue, string newValue)
        {
            if (!TrySetValue(key, originValue, newValue))
                throw new InvalidOperationException("Could not set the value.");
        }

        /// <inheritdoc />
        public bool TrySetValue(string key, string originValue, string newValue)
        {
            EnsureInitialized();

            using (var scope = _scopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.KeyValues);

                var sql = scope.SqlContext.Sql().Select<KeyValueDto>().From<KeyValueDto>().Where<KeyValueDto>(x => x.Key == key);
                var dto = scope.Database.Fetch<KeyValueDto>(sql).FirstOrDefault();

                if (dto == null || dto.Value != originValue)
                    return false;

                dto.Value = newValue;
                dto.Updated = DateTime.Now;
                scope.Database.Update(dto);

                scope.Complete();
            }

            return true;
        }
    }
}
