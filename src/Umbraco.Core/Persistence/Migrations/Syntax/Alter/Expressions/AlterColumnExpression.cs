using Umbraco.Core.Persistence.Migrations.Model;

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
            //TODO Implement usage of the SqlSyntax provider here to generate the sql statement for this expression.
            return TableName + " " + Column.Name + " " + Column.Type ?? Column.CustomType;
        }
    }
}