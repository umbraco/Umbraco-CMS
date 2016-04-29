using NPoco;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteDefaultConstraintExpression : MigrationExpressionBase
    {
        public DeleteDefaultConstraintExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            return string.Format(SqlSyntax.DeleteDefaultConstraint,
                                 TableName,
                                 ColumnName);
        }
    }
}