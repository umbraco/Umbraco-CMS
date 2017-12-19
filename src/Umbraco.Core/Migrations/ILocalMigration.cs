using Umbraco.Core.Migrations.Expressions.Alter;
using Umbraco.Core.Migrations.Expressions.Create;
using Umbraco.Core.Migrations.Expressions.Delete;
using Umbraco.Core.Migrations.Expressions.Execute;
using Umbraco.Core.Migrations.Expressions.Update;

namespace Umbraco.Core.Migrations
{
    public interface ILocalMigration
    {
        IExecuteBuilder Execute { get; }
        IDeleteBuilder Delete { get; }
        IUpdateBuilder Update { get; }
        IAlterBuilder Alter { get; }
        ICreateBuilder Create { get; }
        string GetSql();
    }
}
