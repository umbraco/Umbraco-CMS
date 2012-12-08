using System;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateColumnExpression : IMigrationExpression
    {
        public CreateColumnExpression()
        {
            Column = new ColumnDefinition { ModificationType = ModificationType.Create };
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual ColumnDefinition Column { get; set; }

        public override string ToString()
        {

            var output = String.Format(SyntaxConfig.SqlSyntaxProvider.AddColumn,
                                       SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(TableName),
                                       SyntaxConfig.SqlSyntaxProvider.Format(Column));
            return output;
        }
    }
}