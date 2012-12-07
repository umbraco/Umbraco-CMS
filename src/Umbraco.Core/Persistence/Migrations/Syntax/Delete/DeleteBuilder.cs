using Umbraco.Core.Persistence.Migrations.Model;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.DefaultConstraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.ForeignKey;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Index;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete
{
    public class DeleteBuilder : IDeleteBuilder
    {
        private readonly IMigrationContext _context;

        public DeleteBuilder(IMigrationContext context)
        {
            _context = context;
        }

        public void Table(string tableName)
        {
            var expression = new DeleteTableExpression { TableName = tableName };
            _context.Expressions.Add(expression);
        }

        public IDeleteColumnFromTableSyntax Column(string columnName)
        {
            var expression = new DeleteColumnExpression { ColumnNames = { columnName } };
            _context.Expressions.Add(expression);
            return new DeleteColumnBuilder(expression);
        }

        public IDeleteForeignKeyFromTableSyntax ForeignKey()
        {
            var expression = new DeleteForeignKeyExpression();
            _context.Expressions.Add(expression);
            return new DeleteForeignKeyBuilder(expression);
        }

        public IDeleteForeignKeyOnTableSyntax ForeignKey(string foreignKeyName)
        {
            var expression = new DeleteForeignKeyExpression { ForeignKey = { Name = foreignKeyName } };
            _context.Expressions.Add(expression);
            return new DeleteForeignKeyBuilder(expression);
        }

        public IDeleteDataSyntax FromTable(string tableName)
        {
            var expression = new DeleteDataExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new DeleteDataBuilder(expression);
        }

        public IDeleteIndexForTableSyntax Index()
        {
            var expression = new DeleteIndexExpression();
            _context.Expressions.Add(expression);
            return new DeleteIndexBuilder(expression);
        }

        public IDeleteIndexForTableSyntax Index(string indexName)
        {
            var expression = new DeleteIndexExpression {Index = {Name = indexName}};
            _context.Expressions.Add(expression);
            return new DeleteIndexBuilder(expression);
        }

        public IDeleteConstraintOnTableSyntax PrimaryKey(string primaryKeyName)
        {
            var expression = new DeleteConstraintExpression(ConstraintType.PrimaryKey)
                                 {
                                     Constraint = { ConstraintName = primaryKeyName }
                                 };
            _context.Expressions.Add(expression);
            return new DeleteConstraintBuilder(expression);
        }

        public IDeleteConstraintOnTableSyntax UniqueConstraint(string constraintName)
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique)
                                 {
                                     Constraint = { ConstraintName = constraintName }
                                 };
            _context.Expressions.Add(expression);
            return new DeleteConstraintBuilder(expression);
        }

        public IDeleteDefaultConstraintOnTableSyntax DefaultConstraint()
        {
            var expression = new DeleteDefaultConstraintExpression();
            _context.Expressions.Add(expression);
            return new DeleteDefaultConstraintBuilder(expression);
        }
    }
}