namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions
{
    public class AlterDefaultConstraintExpression : IMigrationExpression
    {
        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }
        public virtual object DefaultValue { get; set; }

        public override string ToString()
        {
            return base.ToString() +
                    string.Format("{0}.{1} {2} {3}",
                                SchemaName,
                                TableName,
                                ColumnName,
                                DefaultValue);
        }
    }
}