using Umbraco.Core.Persistence.Migrations.Syntax.Check.Column;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Table
{
    public interface ICheckTableOptionSyntax : ICheckOptionSyntax
    {
        ICheckColumnOptionSyntax Column(string columnName);
    }
}
