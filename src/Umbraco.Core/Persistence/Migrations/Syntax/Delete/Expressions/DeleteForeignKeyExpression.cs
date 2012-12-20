using System;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteForeignKeyExpression : IMigrationExpression
    {
        public DeleteForeignKeyExpression()
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public virtual ForeignKeyDefinition ForeignKey { get; set; }

        public override string ToString()
        {
            if (ForeignKey.ForeignTable == null)
                throw new ArgumentNullException("Table name not specified, ensure you have appended the OnTable extension. Format should be Delete.ForeignKey(KeyName).OnTable(TableName)");

            return string.Format(SyntaxConfig.SqlSyntaxProvider.DeleteConstraint,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(ForeignKey.ForeignTable),
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(ForeignKey.Name));
        }
    }
}