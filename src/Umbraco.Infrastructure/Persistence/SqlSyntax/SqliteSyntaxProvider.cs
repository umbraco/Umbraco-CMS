using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax
{
    public class SqliteSyntaxProvider : MicrosoftSqlSyntaxProviderBase<SqliteSyntaxProvider>
    {
        public override string ProviderName => Cms.Core.Constants.DatabaseProviders.SQLite;

        public override IsolationLevel DefaultIsolationLevel => IsolationLevel.ReadCommitted; // No idea if this is the right one ?

        public override string DbProvider => Cms.Core.Constants.DatabaseProviders.SQLite;


        public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db)
        {
            // PRAGMA index_info('My_Table');
            // SELECT name as INDEX_NAME, tbl_name as TABLE_NAME, sql, type FROM sqlite_master where type == 'index'


            var items = db.Fetch<dynamic>(
        @"SELECT
	m.tbl_name AS TABLE_NAME,
	ilist.name AS INDEX_NAME,
	iinfo.name AS COLUMN_NAME,
	ilist.[unique] AS UNIQUE_INDEX
FROM 
	sqlite_master AS m, 
	pragma_index_list(m.name) AS ilist, 
	pragma_index_info(ilist.name) AS iinfo");

            return items.Select(item => new Tuple<string, string, string, bool>(item.TABLE_NAME, item.INDEX_NAME, item.COLUMN_NAME, item.UNIQUE_INDEX == 1)).ToList();
        }

        public override void ReadLock(IDatabase db, params int[] lockIds)
        {
            throw new NotImplementedException();
        }

        public override void ReadLock(IDatabase db, TimeSpan timeout, int lockId)
        {
            throw new NotImplementedException();
        }

        public override Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top)
        {
            // SQLite uses LIMIT as opposed to TOP
            // SELECT TOP 5 * FROM My_Table
            // SELECT * FROM My_Table LIMIT 5;

            var result = new Sql<ISqlContext>(sql.SqlContext, sql.SQL.Insert(sql.SQL.IndexOf(' '), " TOP " + top), sql.Arguments);
            var other = new Sql<ISqlContext>(sql.SqlContext, sql.SQL.Insert(sql.SQL.IndexOf(' '), " LIMIT " + top), sql.Arguments);

            return other;
        }

        public override bool TryGetDefaultConstraint(IDatabase db, string tableName, string columnName, out string constraintName)
        {
            throw new NotImplementedException();
        }

        public override void WriteLock(IDatabase db, params int[] lockIds)
        {
            throw new NotImplementedException();
        }

        public override void WriteLock(IDatabase db, TimeSpan timeout, int lockId)
        {
            throw new NotImplementedException();
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString(column) : string.Empty;
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

        private static string GetIdentityString(ColumnDefinition column)
        {
            return "AUTOINCREMENT"; // AUTOINCREMENT as opposed to IDENTITY(1,1)
        }
    }
}
