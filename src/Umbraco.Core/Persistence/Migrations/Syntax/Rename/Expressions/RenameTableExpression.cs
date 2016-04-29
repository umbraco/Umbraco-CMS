using NPoco;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename.Expressions
{
    public class RenameTableExpression : MigrationExpressionBase
    {
        public RenameTableExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        public virtual string SchemaName { get; set; }
        public virtual string OldName { get; set; }
        public virtual string NewName { get; set; }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;
            
            return SqlSyntax.FormatTableRename(OldName, NewName);
        }
    }
}