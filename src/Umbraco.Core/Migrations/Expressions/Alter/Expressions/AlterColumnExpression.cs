using NPoco;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Alter.Expressions
{
    public class AlterColumnExpression : MigrationExpressionBase
    {

        public AlterColumnExpression(IMigrationContext context)
            : base(context)
        {
            Column = new ColumnDefinition { ModificationType = ModificationType.Alter };
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual ColumnDefinition Column { get; set; }

        protected override string GetSql()
        {
            return string.Format(SqlSyntax.AlterColumn,
                                SqlSyntax.GetQuotedTableName(TableName),
                                SqlSyntax.Format(Column));
        }
    }
}
