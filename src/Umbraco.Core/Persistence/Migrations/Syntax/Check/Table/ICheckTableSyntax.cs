using Umbraco.Core.Persistence.Migrations.Syntax.Check.Column;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Table
{
    public interface ICheckTableSyntax : ICheckOptionSyntax
    {
        ICheckColumnOnTableSyntax Column(string columnName);
    }
}
