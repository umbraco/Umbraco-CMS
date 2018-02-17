using System.Collections.Generic;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions
{
    public class CheckConstraintExpression : MigrationExpressionBase
    {
        public CheckConstraintExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) : base(current, databaseProviders, sqlSyntax)
        {
            ColumnNames = new List<string>();
        }

        public ICollection<string> ColumnNames { get; set; }
        public virtual string ConstraintName { get; set; }
        public virtual string TableName { get; set; }
    }
}
