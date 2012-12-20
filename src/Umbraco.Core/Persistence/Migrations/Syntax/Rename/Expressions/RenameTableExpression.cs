using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename.Expressions
{
    public class RenameTableExpression : IMigrationExpression
    {
        public virtual string SchemaName { get; set; }
        public virtual string OldName { get; set; }
        public virtual string NewName { get; set; }

        public override string ToString()
        {
            return SyntaxConfig.SqlSyntaxProvider.FormatTableRename(OldName, NewName);
        }
    }
}