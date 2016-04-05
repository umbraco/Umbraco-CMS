using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NPoco;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides extension methods to NPoco Sql class.
    /// </summary>
    public static class NPocoSqlExtensions
    {
        public static UmbracoSql For(this Sql sql, SqlContext sqlContext)
        {
            var uSql = sql as UmbracoSql;
            if (uSql != null && uSql.SqlContext == sqlContext)
                return uSql;

            // fixme - isBuilt?
            uSql = new UmbracoSql(sqlContext, sql.SQL, sql.Arguments);
            return uSql;
        }

        public static UmbracoSql For(this Sql sql, ISqlSyntaxProvider sqlSyntax, IDatabaseConfig database)
        {
            var sqlContext = new SqlContext(sqlSyntax, database);
            return sql.For(sqlContext);
        }

        // fixme - should we keep these?

        public static Sql.SqlJoinClause LeftOuterJoin(this Sql sql, string table) { return Join(sql, "LEFT OUTER JOIN ", table); }
        public static Sql.SqlJoinClause RightJoin(this Sql sql, string table) { return Join(sql, "RIGHT JOIN ", table); }

        // copied from NPoco (private)
        private static Sql.SqlJoinClause Join(Sql sql, string joinType, string table)
        {
            return new Sql.SqlJoinClause(sql.Append(new Sql(joinType + table)));
        }
    }
}