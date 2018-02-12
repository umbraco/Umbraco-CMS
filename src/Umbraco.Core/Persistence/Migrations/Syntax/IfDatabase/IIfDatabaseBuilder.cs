using Umbraco.Core.Persistence.Migrations.Syntax.Check;
using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename;
using Umbraco.Core.Persistence.Migrations.Syntax.Update;

namespace Umbraco.Core.Persistence.Migrations.Syntax.IfDatabase
{
    public interface IIfDatabaseBuilder : IFluentSyntax
    {
        ICheckBuilder Check { get; }
        ICreateBuilder Create { get; }
        IExecuteBuilder Execute { get; }
        IDeleteBuilder Delete { get; }
        IRenameBuilder Rename { get; }
        IUpdateBuilder Update { get; }
    }
}
