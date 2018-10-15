using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter;
using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.Migrations.Syntax.Update;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    internal class LocalMigrationContext : MigrationContext
    {
        private readonly ISqlSyntaxProvider _sqlSyntax;

        public LocalMigrationContext(DatabaseProviders databaseProvider, Database database, ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(databaseProvider, database, logger)
        {
            _sqlSyntax = sqlSyntax;
        }

        public IExecuteBuilder Execute
        {
            get { return new ExecuteBuilder(this, _sqlSyntax); }
        }

        public IDeleteBuilder Delete
        {
            get { return new DeleteBuilder(this, _sqlSyntax); }
        }

        public IUpdateBuilder Update
        {
            get { return new UpdateBuilder(this, _sqlSyntax); }
        }        

        public IAlterSyntaxBuilder Alter
        {
            get { return new AlterSyntaxBuilder(this, _sqlSyntax); }
        }

        public ICreateBuilder Create
        {
            get { return new CreateBuilder(this, _sqlSyntax); }
        }

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
