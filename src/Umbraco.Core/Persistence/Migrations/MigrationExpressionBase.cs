using System;
using System.Linq;
using NPoco;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    public abstract class MigrationExpressionBase : IMigrationExpression
    {
        private readonly IMigrationContext _context;

        protected MigrationExpressionBase(IMigrationContext context, DatabaseType[] supportedDatabaseTypes = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _context = context;
            SupportedDatabaseTypes = supportedDatabaseTypes;
        }

        public virtual DatabaseType[] SupportedDatabaseTypes { get; }

        public ISqlSyntaxProvider SqlSyntax => _context.Database.SqlSyntax;

        public virtual DatabaseType CurrentDatabaseType => _context.Database.DatabaseType;

        public bool IsExpressionSupported()
        {
            return SupportedDatabaseTypes == null
                || SupportedDatabaseTypes.Length == 0
                // beware!
                // DatabaseType.SqlServer2005 = DatabaseTypes.SqlServerDatabaseType
                // DatabaseType.SqlServer2012 = DatabaseTypes.SqlServer2012DatabaseType
                // with cascading inheritance, so if SqlServer2005 is "supported" we
                // need to accept SqlServer2012 too => cannot simply test with "Contains"
                // and have to test the types.
                //|| SupportedDatabaseTypes.Contains(CurrentDatabaseType);
                || SupportedDatabaseTypes.Any(x => CurrentDatabaseType.GetType().Inherits(x.GetType()));
        }

        public virtual string Process(Database database)
        {
            return ToString();
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