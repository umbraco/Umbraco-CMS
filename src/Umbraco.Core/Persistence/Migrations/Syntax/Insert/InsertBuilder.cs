using Umbraco.Core.Persistence.Migrations.Syntax.Insert.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert
{
    public class InsertBuilder : IInsertBuilder
    {
        private readonly IMigrationContext _context;

        public InsertBuilder(IMigrationContext context)
        {
            _context = context;
        }

        public IInsertDataSyntax IntoTable(string tableName)
        {
            var expression = new InsertDataExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new InsertDataBuilder(expression);
        }
    }
}