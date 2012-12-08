namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteDefaultConstraintExpression : IMigrationExpression
    {
        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }

        public override string ToString()
        {
            //TODO Change to use sql syntax provider
            return base.ToString() +
                   string.Format("{0}.{1} {2}",
                                 SchemaName,
                                 TableName,
                                 ColumnName);
        }
    }
}