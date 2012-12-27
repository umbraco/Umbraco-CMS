using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename;

namespace Umbraco.Core.Persistence.Migrations.Syntax.IfDatabase
{
    public interface IIfDatabaseBuilder : IFluentSyntax
    {
        ICreateBuilder Create { get; }
        IDeleteBuilder Delete { get; }
        IRenameBuilder Rename { get; }
    }
}