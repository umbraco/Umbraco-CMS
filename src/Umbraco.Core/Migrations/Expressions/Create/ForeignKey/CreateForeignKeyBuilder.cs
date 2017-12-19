using System.Data;
using Umbraco.Core.Migrations.Expressions.Common.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Create.ForeignKey
{
    public class CreateForeignKeyBuilder : ExpressionBuilderBase<CreateForeignKeyExpression>,
                                           ICreateForeignKeyFromTableBuilder,
                                           ICreateForeignKeyForeignColumnBuilder,
                                           ICreateForeignKeyToTableBuilder,
                                           ICreateForeignKeyPrimaryColumnBuilder,
                                           ICreateForeignKeyCascadeBuilder
    {
        public CreateForeignKeyBuilder(CreateForeignKeyExpression expression) : base(expression)
        {
        }

        public ICreateForeignKeyForeignColumnBuilder FromTable(string table)
        {
            Expression.ForeignKey.ForeignTable = table;
            return this;
        }

        public ICreateForeignKeyToTableBuilder ForeignColumn(string column)
        {
            Expression.ForeignKey.ForeignColumns.Add(column);
            return this;
        }

        public ICreateForeignKeyToTableBuilder ForeignColumns(params string[] columns)
        {
            foreach (var column in columns)
                Expression.ForeignKey.ForeignColumns.Add(column);
            return this;
        }

        public ICreateForeignKeyPrimaryColumnBuilder ToTable(string table)
        {
            Expression.ForeignKey.PrimaryTable = table;
            return this;
        }

        public ICreateForeignKeyCascadeBuilder PrimaryColumn(string column)
        {
            Expression.ForeignKey.PrimaryColumns.Add(column);
            return this;
        }

        public ICreateForeignKeyCascadeBuilder PrimaryColumns(params string[] columns)
        {
            foreach (var column in columns)
                Expression.ForeignKey.PrimaryColumns.Add(column);
            return this;
        }

        public ICreateForeignKeyCascadeBuilder OnDelete(Rule rule)
        {
            Expression.ForeignKey.OnDelete = rule;
            return this;
        }

        public ICreateForeignKeyCascadeBuilder OnUpdate(Rule rule)
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
