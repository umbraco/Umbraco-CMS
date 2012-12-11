namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteTableExpression : IMigrationExpression
    {
        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }

        public override string ToString()
        {
            //TODO implement the use of sql syntax provider

            return base.ToString() + TableName;
        }
    }
}