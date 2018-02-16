using Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check
{
    public interface ICheckBuilder : IFluentSyntax
    {
        ICheckConstraintSyntax Constraint(string constraintName);
        ICheckTableSyntax Table(string tableName);
    }
}
