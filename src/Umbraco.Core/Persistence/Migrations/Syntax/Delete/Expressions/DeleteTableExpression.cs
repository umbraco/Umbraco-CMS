using NPoco;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteTableExpression : MigrationExpressionBase
    {        
        public DeleteTableExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes) 
            : base(context, supportedDatabaseTypes)
        { }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }

        public override string ToString()
        {
            return string.Format(SqlSyntax.DropTable,
                                 SqlSyntax.GetQuotedTableName(TableName));
        }
    }
}