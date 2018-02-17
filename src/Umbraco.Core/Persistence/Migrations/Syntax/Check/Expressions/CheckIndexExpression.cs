using System.Collections.Generic;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions
{
    public class CheckIndexExpression : MigrationExpressionBase
    {
        public CheckIndexExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) : base(current, databaseProviders, sqlSyntax)
        {
            ColumnNames = new List<string>();
        }

        public virtual string IndexName { get; set; }
        public virtual string TableName { get; set; }
        public ICollection<string> ColumnNames { get; set; }
        public virtual bool? Unique { get; set; }
    }
}
