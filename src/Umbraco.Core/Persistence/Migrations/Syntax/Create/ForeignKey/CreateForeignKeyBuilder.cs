using System.Data;
using Umbraco.Core.Persistence.Migrations.Syntax.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.ForeignKey
{
    public class CreateForeignKeyBuilder : ExpressionBuilderBase<CreateForeignKeyExpression>,
                                           ICreateForeignKeyFromTableSyntax,
                                           ICreateForeignKeyForeignColumnSyntax,
                                           ICreateForeignKeyToTableSyntax,
                                           ICreateForeignKeyPrimaryColumnSyntax,
                                           ICreateForeignKeyCascadeSyntax
    {
        public CreateForeignKeyBuilder(CreateForeignKeyExpression expression) : base(expression)
        {
        }

        public ICreateForeignKeyForeignColumnSyntax FromTable(string table)
        {
            Expression.ForeignKey.ForeignTable = table;
            return this;
        }

        public ICreateForeignKeyToTableSyntax ForeignColumn(string column)
        {
            Expression.ForeignKey.ForeignColumns.Add(column);
            return this;
        }

        public ICreateForeignKeyToTableSyntax ForeignColumns(params string[] columns)
        {
            foreach (var column in columns)
                Expression.ForeignKey.ForeignColumns.Add(column);
            return this;
        }

        public ICreateForeignKeyPrimaryColumnSyntax ToTable(string table)
        {
            Expression.ForeignKey.PrimaryTable = table;
            return this;
        }

        public ICreateForeignKeyCascadeSyntax PrimaryColumn(string column)
        {
            Expression.ForeignKey.PrimaryColumns.Add(column);
            return this;
        }

        public ICreateForeignKeyCascadeSyntax PrimaryColumns(params string[] columns)
        {
            foreach (var column in columns)
                Expression.ForeignKey.PrimaryColumns.Add(column);
            return this;
        }

        public ICreateForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            Expression.ForeignKey.OnDelete = rule;
            return this;
        }

        public ICreateForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            Expression.ForeignKey.OnUpdate = rule;
            return this;
        }

        public void OnDeleteOrUpdate(Rule rule)
        {
            Expression.ForeignKey.OnDelete = rule;
            Expression.ForeignKey.OnUpdate = rule;
        }
    }
}