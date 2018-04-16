using NPoco;

namespace Umbraco.Core.Migrations.Expressions.Alter.Expressions
{
    public class AlterTableExpression : MigrationExpressionBase
    {
        public AlterTableExpression(IMigrationContext context)
            : base(context)
        { }

        public virtual string TableName { get; set; }

        protected override string GetSql()
        {
            return string.Empty;
        }
    }
}
