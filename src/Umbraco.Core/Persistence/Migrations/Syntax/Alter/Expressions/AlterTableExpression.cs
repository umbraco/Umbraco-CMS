using NPoco;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions
{
    public class AlterTableExpression : MigrationExpressionBase
    {
        public AlterTableExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes) 
            : base(context, supportedDatabaseTypes)
        { }

        public virtual string SchemaName { get; set; }

        public virtual string TableName { get; set; }

        public override string ToString()
        {
            return $"ALTER TABLE {TableName}";
        }
    }
}