using NPoco;

namespace Umbraco.Core.Migrations.Expressions.Alter.Expressions
{
    public class AlterTableExpression : MigrationExpressionBase
    {
        public AlterTableExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }


        public virtual string TableName { get; set; }

        public override string ToString() // fixme kill
            => GetSql();

        protected override string GetSql()
        {
            return $"ALTER TABLE {TableName}";
        }
    }
}
