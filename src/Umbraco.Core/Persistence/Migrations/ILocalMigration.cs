using Umbraco.Core.Persistence.Migrations.Syntax.Alter;
using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.Migrations.Syntax.Update;

namespace Umbraco.Core.Persistence.Migrations
{
    public interface ILocalMigration
    {
        IExecuteBuilder Execute { get; }
        IDeleteBuilder Delete { get; }
        IUpdateBuilder Update { get; }
        IAlterSyntaxBuilder Alter { get; }
        ICreateBuilder Create { get; }
        string GetSql();
    }
}
