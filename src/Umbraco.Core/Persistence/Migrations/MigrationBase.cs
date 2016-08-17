using System;
using NPoco;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter;
using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.Migrations.Syntax.IfDatabase;
using Umbraco.Core.Persistence.Migrations.Syntax.Insert;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename;
using Umbraco.Core.Persistence.Migrations.Syntax.Update;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    public abstract class MigrationBase : IMigration
    {
        public ISqlSyntaxProvider SqlSyntax => Context.Database.SqlSyntax;

        public DatabaseType DatabaseType => Context.Database.DatabaseType;

        public ILogger Logger { get; }
        protected IMigrationContext Context { get; }

        protected MigrationBase(IMigrationContext context)
        {
            Logger = context.Logger;
            Context = context;
        }

        public abstract void Up();
        public abstract void Down();        

        public IAlterSyntaxBuilder Alter => new AlterSyntaxBuilder(Context);

        public ICreateBuilder Create => new CreateBuilder(Context);

        public IDeleteBuilder Delete => new DeleteBuilder(Context);

        public IExecuteBuilder Execute => new ExecuteBuilder(Context);

        public IInsertBuilder Insert => new InsertBuilder(Context);

        public IRenameBuilder Rename => new RenameBuilder(Context);

        public IUpdateBuilder Update => new UpdateBuilder(Context);

        public IIfDatabaseBuilder IfDatabase(params DatabaseType[] supportedDatabaseTypes)
        {
            return new IfDatabaseBuilder(Context, supportedDatabaseTypes);
        }

        protected Sql<SqlContext> Sql()
        {
            return Context.Database.Sql();
        }
    }
}