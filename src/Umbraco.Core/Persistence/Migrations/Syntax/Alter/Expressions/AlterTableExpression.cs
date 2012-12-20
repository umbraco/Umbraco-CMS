namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions
{
    public class AlterTableExpression : IMigrationExpression
    {
        public AlterTableExpression()
        {
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }

        public override string ToString()
        {
            return string.Format("ALTER TABLE {0}", TableName);
        }
    }
}