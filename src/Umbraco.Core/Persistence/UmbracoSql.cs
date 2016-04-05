using System;
using NPoco;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Extends NPoco <see cref="Sql"/> class with a context.
    /// </summary>
    public class UmbracoSql : Sql
    {
        public UmbracoSql(SqlContext sqlContext)
        {
            if (sqlContext == null) throw new ArgumentNullException(nameof(sqlContext));
            SqlContext = sqlContext;
        }

        public UmbracoSql(SqlContext sqlContext, string sql, params object[] args)
            : base(sql, args)
        {
            if (sqlContext == null) throw new ArgumentNullException(nameof(sqlContext));
            SqlContext = sqlContext;
        }

        public UmbracoSql(SqlContext sqlContext, bool isBuilt, string sql, params object[] args)
            : base(isBuilt, sql, args)
        {
            if (sqlContext == null) throw new ArgumentNullException(nameof(sqlContext));
            SqlContext = sqlContext;
        }

        public SqlContext SqlContext { get; }

        public class UmbracoSqlJoinClause
        {
            private readonly UmbracoSql _sql;

            public UmbracoSqlJoinClause(UmbracoSql sql)
            {
                _sql = sql;
            }

            public SqlContext SqlContext => _sql.SqlContext;

            public UmbracoSql On(string onClause, params object[] args)
            {
                _sql.Append("ON " + onClause, args);
                return _sql;
            }
        }

        public UmbracoSql Append(UmbracoSql sql)
        {
            base.Append(sql);
            return this;
        }

        public new UmbracoSql Append(string sql, params object[] args)
        {
            Append(new Sql(sql, args));
            return this;
        }

        public new UmbracoSql Where(string sql, params object[] args)
        {
            Append("WHERE (" + sql + ")", args);
            return this;
        }

        public new UmbracoSql OrderBy(params object[] columns)
        {
            Append("ORDER BY " + string.Join(", ", columns));
            return this;
        }

        public new UmbracoSql Select(params object[] columns)
        {
            Append("SELECT " + string.Join(", ", columns));
            return this;
        }

        public new UmbracoSql From(params object[] tables)
        {
            Append("FROM " + string.Join(", ", tables));
            return this;
        }

        public new UmbracoSql GroupBy(params object[] columns)
        {
            Append("GROUP BY ", string.Join(", ", columns));
            return this;
        }

        public UmbracoSqlJoinClause Join(string joinType, string table)
        {
            Append(joinType + table);
            return new UmbracoSqlJoinClause(this);
        }

        public new UmbracoSqlJoinClause InnerJoin(string table) { return Join("INNER JOIN ", table); }
        public new UmbracoSqlJoinClause LeftJoin(string table) { return Join("LEFT JOIN ", table); }
    }
}
