using Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Index;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check
{
    public interface ICheckBuilder : IFluentSyntax
    {
        ICheckConstraintSyntax Constraint(string constraintName);
        ICheckForeignKeySyntax ForeignKey(string foreignKeyName);
        ICheckIndexSyntax Index(string indexName);
        ICheckTableSyntax Table(string tableName);
    }
}
