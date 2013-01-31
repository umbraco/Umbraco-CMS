using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename
{
    public interface IRenameBuilder : IFluentSyntax
    {
        IRenameTableSyntax Table(string oldName);
        IRenameColumnTableSyntax Column(string oldName);
    }
}