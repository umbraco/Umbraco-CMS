using Umbraco.Core.Migrations.Syntax.Create;
using Umbraco.Core.Migrations.Syntax.Delete;
using Umbraco.Core.Migrations.Syntax.Execute;
using Umbraco.Core.Migrations.Syntax.Rename;
using Umbraco.Core.Migrations.Syntax.Update;

namespace Umbraco.Core.Migrations.Syntax.IfDatabase
{
    public interface IIfDatabaseBuilder : IFluentSyntax
    {
        ICreateBuilder Create { get; }
        IExecuteBuilder Execute { get; }
        IDeleteBuilder Delete { get; }
        IRenameBuilder Rename { get; }
        IUpdateBuilder Update { get; }
    }
}
