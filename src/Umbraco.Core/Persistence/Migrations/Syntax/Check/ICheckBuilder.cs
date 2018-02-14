using Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check
{
    public interface ICheckBuilder : IFluentSyntax
    {
        ICheckTableOptionSyntax Table(string tableName);
        ICheckConstraintOptionSyntax Constraint(string constraintName);
    }
}
