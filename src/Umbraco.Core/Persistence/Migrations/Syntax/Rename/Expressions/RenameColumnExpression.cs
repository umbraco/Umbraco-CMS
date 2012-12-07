namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename.Expressions
{
    public class RenameColumnExpression : IMigrationExpression
    {
        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string OldName { get; set; }
        public virtual string NewName { get; set; }

        public override string ToString()
        {
            //TODO Implement usage of sql syntax provider
            return base.ToString() + TableName + " " + OldName + " to " + NewName;
        }
    }
}