using System.Collections.Generic;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions
{
    public class CheckForeignKeyExpression : MigrationExpressionBase
    {
        public CheckForeignKeyExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) : base(current, databaseProviders, sqlSyntax)
        {
            ForeignColumnNames = new List<string>();
            PrimaryColumnNames = new List<string>();
        }

        public virtual string ForeignKeyName { get; set; }
        public virtual string ForeignTableName { get; set; }
        public virtual string PrimaryTableName { get; set; }
        public ICollection<string> ForeignColumnNames { get; set; }
        public ICollection<string> PrimaryColumnNames { get; set; }
    }
}
