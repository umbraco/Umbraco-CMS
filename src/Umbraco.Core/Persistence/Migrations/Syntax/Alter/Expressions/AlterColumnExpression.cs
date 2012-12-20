using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions
{
    public class AlterColumnExpression : IMigrationExpression
    {
        public AlterColumnExpression()
        {
            Column = new ColumnDefinition() { ModificationType = ModificationType.Alter };
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual ColumnDefinition Column { get; set; }

        public override string ToString()
        {
            return string.Format(SyntaxConfig.SqlSyntaxProvider.AlterColumn,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(TableName),
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(Column.Name));
        }
    }
}