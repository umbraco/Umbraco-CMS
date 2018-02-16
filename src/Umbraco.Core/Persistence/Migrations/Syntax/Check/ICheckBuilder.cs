using Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Index;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check
{
    public interface ICheckBuilder : IFluentSyntax
    {
        ICheckConstraintSyntax Constraint(string constraintName);
        ICheckIndexSyntax Index(string indexName);
        ICheckTableSyntax Table(string tableName);
    }
}
