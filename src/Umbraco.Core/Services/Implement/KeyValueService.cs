using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Expressions.Create;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Scoping;
using Umbraco.Core.Persistence;

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
            // all this cannot be achieved via migrations since it needs to run
            // before any migration, in order to figure out migrations

            using (var scope = _scopeProvider.CreateScope())
            {
                // assume that if the lock object exists, then everything is ok
                if (scope.Database.Exists<LockDto>(Constants.Locks.KeyValues))
                {
                    scope.Complete();
                    return;
                }

                // drop the 'identity' on primary key
                foreach (var sql in new[]
                {
                    "alter table umbracoLock add column nid int null;",
                    "update umbracoLock set nid = id;",
                    "alter table umbracoLock drop constraint PK_umbracoLock;",
                    "alter table umbracoLock drop column id;",
                    "alter table umbracoLock add column id int null;",
                    "update umbracoLock set id = nid;",
                    "alter table umbracoLock drop column nid;",
                    "alter table umbracoLock alter column id int not null;",
                    "alter table umbracoLock add constraint PK_umbracoLock primary key (id);"
                })
                    scope.Database.Execute(sql);

                // insert the key-value lock
                scope.Database.Execute($@"INSERT {scope.SqlContext.SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.Lock)} (id, name, value)
VALUES ({Constants.Locks.KeyValues}, 'KeyValues', 1);");

                // create the key-value table
                var context = new MigrationContext(scope.Database, _logger);
                new CreateBuilder(context).Table<KeyValueDto>().Do();

                scope.Complete();
            }
        }

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

        public void SetValue(string key, string originValue, string newValue)
        {
            EnsureInitialized();

            using (var scope = _scopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.KeyValues);

                var sql = scope.SqlContext.Sql().Select<KeyValueDto>().From<KeyValueDto>().Where<KeyValueDto>(x => x.Key == key);
                var dto = scope.Database.Fetch<KeyValueDto>(sql).FirstOrDefault();

                if (dto == null)
                    throw new InvalidOperationException("Key not found.");

                if (dto.Value != originValue)
                    throw new InvalidOperationException("Value has changed.");

                dto.Value = newValue;
                dto.Updated = DateTime.Now;
                scope.Database.Update(dto);

                scope.Complete();
            }
        }
    }
}
