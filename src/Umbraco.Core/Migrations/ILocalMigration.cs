using Umbraco.Core.Migrations.Syntax.Alter;
using Umbraco.Core.Migrations.Syntax.Create;
using Umbraco.Core.Migrations.Syntax.Delete;
using Umbraco.Core.Migrations.Syntax.Execute;
using Umbraco.Core.Migrations.Syntax.Update;

namespace Umbraco.Core.Migrations
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
