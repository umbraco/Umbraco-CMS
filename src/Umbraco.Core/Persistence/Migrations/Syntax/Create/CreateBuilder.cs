using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.ForeignKey;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Index;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Table;
using Umbraco.Core.Persistence.Migrations.Syntax.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create
{
    public class CreateBuilder : ICreateBuilder
    {
        private readonly IMigrationContext _context;

        public CreateBuilder(IMigrationContext context)
        {
            _context = context;
        }

        public ICreateTableWithColumnSyntax Table(string tableName)
        {
            var expression = new CreateTableExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new CreateTableBuilder(expression, _context);
        }

        public ICreateColumnOnTableSyntax Column(string columnName)
        {
            var expression = new CreateColumnExpression { Column = { Name = columnName } };
            _context.Expressions.Add(expression);
            return new CreateColumnBuilder(expression, _context);
        }

        public ICreateForeignKeyFromTableSyntax ForeignKey()
        {
            var expression = new CreateForeignKeyExpression();
            _context.Expressions.Add(expression);
            return new CreateForeignKeyBuilder(expression);
        }

        public ICreateForeignKeyFromTableSyntax ForeignKey(string foreignKeyName)
        {
            var expression = new CreateForeignKeyExpression { ForeignKey = { Name = foreignKeyName } };
            _context.Expressions.Add(expression);
            return new CreateForeignKeyBuilder(expression);
        }

        public ICreateIndexForTableSyntax Index()
        {
            var expression = new CreateIndexExpression();
            _context.Expressions.Add(expression);
            return new CreateIndexBuilder(expression);
        }

        public ICreateIndexForTableSyntax Index(string indexName)
        {
            var expression = new CreateIndexExpression { Index = { Name = indexName } };
            _context.Expressions.Add(expression);
            return new CreateIndexBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax PrimaryKey()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax PrimaryKey(string primaryKeyName)
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.ConstraintName = primaryKeyName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax UniqueConstraint()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax UniqueConstraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.ConstraintName = constraintName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }
    }
}