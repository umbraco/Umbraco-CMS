using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.ForeignKey
{
    public class DeleteForeignKeyBuilder : ExpressionBuilderBase<DeleteForeignKeyExpression>,
                                           IDeleteForeignKeyFromTableSyntax,
                                           IDeleteForeignKeyForeignColumnSyntax,
                                           IDeleteForeignKeyToTableSyntax,
                                           IDeleteForeignKeyPrimaryColumnSyntax,
                                           IDeleteForeignKeyOnTableSyntax
    {
        public DeleteForeignKeyBuilder(DeleteForeignKeyExpression expression) : base(expression)
        {
        }

        public IDeleteForeignKeyForeignColumnSyntax FromTable(string foreignTableName)
        {
            Expression.ForeignKey.ForeignTable = foreignTableName;
            return this;
        }

        public IDeleteForeignKeyToTableSyntax ForeignColumn(string column)
        {
            Expression.ForeignKey.ForeignColumns.Add(column);
            return this;
        }

        public IDeleteForeignKeyToTableSyntax ForeignColumns(params string[] columns)
        {
            foreach (var column in columns)
            {
                Expression.ForeignKey.ForeignColumns.Add(column);
            }

            return this;
        }

        public IDeleteForeignKeyPrimaryColumnSyntax ToTable(string table)
        {
            Expression.ForeignKey.PrimaryTable = table;
            return this;
        }

        public void PrimaryColumn(string column)
        {
            Expression.ForeignKey.PrimaryColumns.Add(column);
        }

        public void PrimaryColumns(params string[] columns)
        {
            foreach (var column in columns)
            {
                Expression.ForeignKey.PrimaryColumns.Add(column);
            }
        }

        public void OnTable(string foreignTableName)
        {
            Expression.ForeignKey.ForeignTable = foreignTableName;
        }
    }
}