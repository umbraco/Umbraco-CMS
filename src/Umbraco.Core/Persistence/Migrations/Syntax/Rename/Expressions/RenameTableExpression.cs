using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename.Expressions
{
    public class RenameTableExpression : MigrationExpressionBase
    {
        public RenameTableExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(current, databaseProviders, sqlSyntax)
        {
        }

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