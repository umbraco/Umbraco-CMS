using System;
using NPoco;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// Represents the Sql Translator for translating a IQuery object to Sql
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SqlTranslator<T>
    {
        private readonly Sql<SqlContext> _sql;

        public SqlTranslator(Sql<SqlContext> sql, IQuery<T> query)
        {
            if (sql == null)
                throw new Exception("Sql cannot be null");

            _sql = sql;
            foreach (var clause in query.GetWhereClauses())
            {
                _sql.Where(clause.Item1, clause.Item2);
            }
        }

        public Sql<SqlContext> Translate()
        {
            return _sql;
        }

        public override string ToString()
        {
            return _sql.SQL;
        }
    }
}