using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions
{
    public class CreateTableExpression : IMigrationExpression
    {
        public CreateTableExpression()
        {
            Columns = new List<ColumnDefinition>();
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual IList<ColumnDefinition> Columns { get; set; }

        public override string ToString()
        {
            return string.Format(SyntaxConfig.SqlSyntaxProvider.CreateTable,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(TableName),
                                 SyntaxConfig.SqlSyntaxProvider.Format(Columns));
        }
    }
}