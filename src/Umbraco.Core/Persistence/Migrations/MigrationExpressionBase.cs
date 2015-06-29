using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    public abstract class MigrationExpressionBase : IMigrationExpression
    {
        [Obsolete("Use the other constructors specifying an ISqlSyntaxProvider instead")]
        protected MigrationExpressionBase()
            : this(SqlSyntaxContext.SqlSyntaxProvider)
        {
        }

        [Obsolete("Use the other constructors specifying an ISqlSyntaxProvider instead")]
        protected MigrationExpressionBase(DatabaseProviders current, DatabaseProviders[] databaseProviders)
            : this(current, databaseProviders, SqlSyntaxContext.SqlSyntaxProvider)
        {
        }

        protected MigrationExpressionBase(ISqlSyntaxProvider sqlSyntax)
        {
            SqlSyntax = sqlSyntax;
        }

        protected MigrationExpressionBase(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
        {
            SupportedDatabaseProviders = databaseProviders;
            SqlSyntax = sqlSyntax;
            CurrentDatabaseProvider = current;
        }

        public virtual DatabaseProviders[] SupportedDatabaseProviders { get; private set; }
        public ISqlSyntaxProvider SqlSyntax { get; private set; }
        public virtual DatabaseProviders CurrentDatabaseProvider { get; private set; }

        public bool IsExpressionSupported()
        {
            if (SupportedDatabaseProviders == null || SupportedDatabaseProviders.Any() == false)
                return true;

            return SupportedDatabaseProviders.Any(x => x == CurrentDatabaseProvider);
        }

        public virtual string Process(Database database)
        {
            return this.ToString();
        }

        /// <summary>
        /// This might be useful in the future if we add it to the interface, but for now it's used to hack the DeleteAppTables & DeleteForeignKeyExpression
        /// to ensure they are not executed twice.
        /// </summary>
        internal string Name { get; set; }
    }
}