using System.Linq;
using System.Text;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Expressions.Alter;
using Umbraco.Core.Migrations.Expressions.Create;
using Umbraco.Core.Migrations.Expressions.Delete;
using Umbraco.Core.Migrations.Expressions.Execute;
using Umbraco.Core.Migrations.Expressions.Update;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Migrations
{
    internal class LocalMigration : MigrationContext, ILocalMigration
    {
        public LocalMigration(IUmbracoDatabase database, ILogger logger)
            : base(database, logger)
        { }

        public IExecuteBuilder Execute => new ExecuteBuilder(this);

        public IDeleteBuilder Delete => new DeleteBuilder(this);

        public IUpdateBuilder Update => new UpdateBuilder(this);

        public IAlterBuilder Alter => new AlterBuilder(this);

        public ICreateBuilder Create => new CreateBuilder(this);

        public string GetSql()
        {
            var sb = new StringBuilder();
            foreach (var sql in Expressions.Select(x => x.Process(this)))
            {
                sb.Append(sql);
                sb.AppendLine();
                sb.AppendLine("GO");
            }
            return sb.ToString();
        }
    }
}
