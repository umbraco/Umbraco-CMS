using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    public abstract class MigrationExpressionBase : IMigrationExpression
    {
        //[Obsolete("Use the other constructors specifying an ISqlSyntaxProvider instead")]
        //protected MigrationExpressionBase()
        //    : this(SqlSyntaxContext.SqlSyntaxProvider)
        //{
        //}

        //[Obsolete("Use the other constructors specifying an ISqlSyntaxProvider instead")]
        //protected MigrationExpressionBase(DatabaseProviders current, DatabaseProviders[] databaseProviders)
        //    : this(current, databaseProviders, SqlSyntaxContext.SqlSyntaxProvider)
        //{
        //}

        //protected MigrationExpressionBase(ISqlSyntaxProvider sqlSyntax)
        //{
        //    SqlSyntax = sqlSyntax;
        //}

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

        protected string GetQuotedValue(object val)
        {
            if (val == null) return "NULL";

            var type = val.GetType();

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return ((bool)val) ? "1" : "0";
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return val.ToString();
                case TypeCode.DateTime:
                    return SqlSyntax.GetQuotedValue(SqlSyntax.FormatDateTime((DateTime) val));
                default:
                    return SqlSyntax.GetQuotedValue(val.ToString());
            }
        }
    }
}