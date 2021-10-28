using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax
{
    public interface ISqlSyntaxProviderExtended
    {
        string GetFieldNameForUpdate<TDto>(Expression<Func<TDto, object>> fieldSelector, string tableAlias = null);

        Sql<ISqlContext> ForUpdate(Sql<ISqlContext> sql);
    }

    public class SqliteSyntaxProvider : SqlSyntaxProviderBase<SqliteSyntaxProvider>, ISqlSyntaxProviderExtended
    {
        private readonly IOptions<GlobalSettings> _globalSettings;

        public SqliteSyntaxProvider(IOptions<GlobalSettings> globalSettings)
        {
            _globalSettings = globalSettings;
        }

        public override string ProviderName => Cms.Core.Constants.DatabaseProviders.SQLite;

        public override IsolationLevel DefaultIsolationLevel => System.Data.IsolationLevel.RepeatableRead;

        public override string DbProvider => Cms.Core.Constants.DatabaseProviders.SQLite;


        public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db)
        {
            // PRAGMA index_info('My_Table');
            // SELECT name as INDEX_NAME, tbl_name as TABLE_NAME, sql, type FROM sqlite_master where type == 'index'

            var items = db.Fetch<IndexMeta>(
        @"SELECT
                m.tbl_name AS tableName,
                ilist.name AS indexName,
                iinfo.name AS columnName,
                ilist.[unique] AS isUnique
            FROM 
                sqlite_master AS m, 
                pragma_index_list(m.name) AS ilist, 
                pragma_index_info(ilist.name) AS iinfo");

            return items.Select(item => new Tuple<string, string, string, bool>(item.TableName, item.IndexName, item.ColumnName, item.IsUnique)).ToList();
        }

        private class IndexMeta
        {
            public string TableName { get; set; }
            public string IndexName { get; set; }
            public string ColumnName { get; set; }
            public bool IsUnique { get; set; }
        }

        public override bool TryGetDefaultConstraint(IDatabase db, string tableName, string columnName, out string constraintName)
        {
            constraintName = string.Empty;
            return false;
        }

        public override Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top)
        {
            // SQLite uses LIMIT as opposed to TOP
            // SELECT TOP 5 * FROM My_Table
            // SELECT * FROM My_Table LIMIT 5;

            return sql.Append($"LIMIT {top}");
        }


        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "PRIMARY KEY AUTOINCREMENT" : string.Empty;
        }

        public override string FormatPrimaryKey(TableDefinition table)
        {
            return string.Empty;
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return "NEWID()"; // No NEWID() in SQLite perhaps try RANDOM()
                case SystemMethods.CurrentDateTime:
                    return "DATE()"; // No GETDATE() trying DATE()
            }

            return null;
        }

        public override bool SupportsIdentityInsert() => false;

        public override bool SupportsClustered() => false;

        public override string GetIndexType(IndexTypes indexTypes) => string.Empty;

        public override List<string> Format(IEnumerable<ForeignKeyDefinition> foreignKeys)
        {
            return new List<string>();
        }

        public override string GetSpecialDbType(SpecialDbType dbType)
        {
            return "TEXT";
        }

        public override IEnumerable<string> GetTablesInSchema(IDatabase db)
        {
            return db.Fetch<string>("select name from sqlite_master where type='table'");
        }

        public override void WriteLock(IDatabase db, TimeSpan timeout, int lockId)
        {
            // soon as we get Database, a transaction is started

            if (db.Transaction.IsolationLevel < IsolationLevel.RepeatableRead)
                throw new InvalidOperationException("A transaction with minimum RepeatableRead isolation level is required.");

            ObtainWriteLock(db, timeout, lockId);
        }

        public override void WriteLock(IDatabase db, params int[] lockIds)
        {
            // soon as we get Database, a transaction is started

            if (db.Transaction.IsolationLevel < IsolationLevel.RepeatableRead)
                throw new InvalidOperationException("A transaction with minimum RepeatableRead isolation level is required.");

            var timeout = _globalSettings.Value.SqlWriteLockTimeOut;

            foreach (var lockId in lockIds)
            {
                ObtainWriteLock(db, timeout, lockId);
            }
        }

        private static void ObtainWriteLock(IDatabase db, TimeSpan timeout, int lockId)
        {
            db.Execute(@$"PRAGMA busy_timeout = {timeout.Milliseconds};");

            var i = db.Execute(@"UPDATE umbracoLock SET value = (CASE WHEN (value=1) THEN -1 ELSE 1 END) WHERE id=@id", new { id = lockId });
            if (i == 0) // ensure we are actually locking!
                throw new ArgumentException($"LockObject with id={lockId} does not exist.");
        }

        public override void ReadLock(IDatabase db, TimeSpan timeout, int lockId)
        {
            // soon as we get Database, a transaction is started

            if (db.Transaction.IsolationLevel < IsolationLevel.RepeatableRead)
                throw new InvalidOperationException("A transaction with minimum RepeatableRead isolation level is required.");

            ObtainReadLock(db, timeout, lockId);
        }

        public override void ReadLock(IDatabase db, params int[] lockIds)
        {
            // soon as we get Database, a transaction is started

            if (db.Transaction.IsolationLevel < IsolationLevel.RepeatableRead)
                throw new InvalidOperationException("A transaction with minimum RepeatableRead isolation level is required.");

            foreach (var lockId in lockIds)
            {
                ObtainReadLock(db, null, lockId);
            }
        }

        private static void ObtainReadLock(IDatabase db, TimeSpan? timeout, int lockId)
        {
            if (timeout.HasValue)
            {
                db.Execute(@$"PRAGMA busy_timeout = {timeout.Value.Milliseconds};");
            }

            var i = db.ExecuteScalar<int?>("SELECT value FROM umbracoLock WHERE id=@id", new { id = lockId });

            if (i == null) // ensure we are actually locking!
                throw new ArgumentException($"LockObject with id={lockId} does not exist.");
        }

        public string GetFieldNameForUpdate<TDto>(Expression<Func<TDto, object>> fieldSelector, string tableAlias = null)
        {
            var field = ExpressionHelper.FindProperty(fieldSelector).Item1 as PropertyInfo;
            var fieldName = field.GetColumnName();

            return GetQuotedColumnName(fieldName);
        }

        public Sql<ISqlContext> ForUpdate(Sql<ISqlContext> sql)
        {
            return sql;
        }
    }
}
