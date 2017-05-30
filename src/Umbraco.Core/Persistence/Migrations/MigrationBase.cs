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
        public ISqlSyntaxProvider SqlSyntax => Context.SqlSyntax;

        public DatabaseType DatabaseType => Context.Database.DatabaseType;

        public ILogger Logger { get; }
        protected IMigrationContext Context { get; }

        protected MigrationBase(IMigrationContext context)
        {
            Logger = context.Logger;
            Context = context;
        }

        public virtual void Up()
        {
            throw new NotSupportedException("This migration does not implement the \"up\" operation.");
        }

        public virtual void Down()
        {
            throw new NotSupportedException("This migration does not implement the \"down\" operation.");
        }

        public IAlterSyntaxBuilder Alter => new AlterSyntaxBuilder(Context);

        public ICreateBuilder Create => new CreateBuilder(Context);

        public IDeleteBuilder Delete => new DeleteBuilder(Context);

        public IExecuteBuilder Execute => new ExecuteBuilder(Context);

        public IInsertBuilder Insert => new InsertBuilder(Context);

        public IRenameBuilder Rename => new RenameBuilder(Context);

        public IUpdateBuilder Update => new UpdateBuilder(Context);

        protected Sql<SqlContext> Sql() => Context.Sql();

        protected Sql<SqlContext> Sql(string sql, params object[] args) => Context.Sql(sql, args);

        public IIfDatabaseBuilder IfDatabase(params DatabaseType[] supportedDatabaseTypes)
        {
            return new IfDatabaseBuilder(Context, supportedDatabaseTypes);
        }
    }
}