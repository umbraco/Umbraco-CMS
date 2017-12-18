using Umbraco.Core.Migrations.Syntax.Rename.Column;
using Umbraco.Core.Migrations.Syntax.Rename.Table;

namespace Umbraco.Core.Migrations.Syntax.Rename
{
    public interface IRenameBuilder : IFluentSyntax
    {
        IRenameTableSyntax Table(string oldName);
        IRenameColumnTableSyntax Column(string oldName);
    }
}
