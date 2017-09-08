using System.Linq;
using System.Text;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter;
using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.Migrations.Syntax.Update;

namespace Umbraco.Core.Persistence.Migrations
{
    internal class LocalMigration : MigrationContext, ILocalMigration
    {
        public LocalMigration(IUmbracoDatabase database, ILogger logger)
            : base(database, logger)
        { }

        public IExecuteBuilder Execute => new ExecuteBuilder(this);

        public IDeleteBuilder Delete => new DeleteBuilder(this);

        public IUpdateBuilder Update => new UpdateBuilder(this);

        public IAlterSyntaxBuilder Alter => new AlterSyntaxBuilder(this);

        public ICreateBuilder Create => new CreateBuilder(this);

        public string GetSql()
        {
            var sb = new StringBuilder();
            foreach (var sql in Expressions.Select(x => x.Process(Database)))
            {
                sb.Append(sql);
                sb.AppendLine();
                sb.AppendLine("GO");
            }
            return sb.ToString();
        }
    }
}
