using NPoco;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions
{
    public class AlterColumnExpression : MigrationExpressionBase
    {

        public AlterColumnExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        {
            Column = new ColumnDefinition { ModificationType = ModificationType.Alter };
        }


        

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual ColumnDefinition Column { get; set; }

        public override string ToString()
        {

            return string.Format(SqlSyntax.AlterColumn,
                                SqlSyntax.GetQuotedTableName(TableName),
                                SqlSyntax.Format(Column));

        }
    }
}