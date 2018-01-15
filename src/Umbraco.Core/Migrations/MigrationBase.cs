using System;
using NPoco;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Expressions.Alter;
using Umbraco.Core.Migrations.Expressions.Create;
using Umbraco.Core.Migrations.Expressions.Delete;
using Umbraco.Core.Migrations.Expressions.Execute;
using Umbraco.Core.Migrations.Expressions.Insert;
using Umbraco.Core.Migrations.Expressions.Rename;
using Umbraco.Core.Migrations.Expressions.Update;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Provides a base class to all migrations.
    /// </summary>
    public abstract partial class MigrationBase : IMigration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationBase"/> class.
        /// </summary>
        /// <param name="context">A migration context.</param>
        protected MigrationBase(IMigrationContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Gets the migration context.
        /// </summary>
        protected IMigrationContext Context { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger => Context.Logger;

        /// <summary>
        /// Gets the Sql syntax.
        /// </summary>
        protected ISqlSyntaxProvider SqlSyntax => Context.SqlContext.SqlSyntax;

        /// <summary>
        /// Gets the database instance.
        /// </summary>
        protected IUmbracoDatabase Database => Context.Database;

        /// <summary>
        /// Gets the database type.
        /// </summary>
        protected DatabaseType DatabaseType => Context.Database.DatabaseType;

        /// <summary>
        /// Creates a new Sql statement.
        /// </summary>
        protected Sql<ISqlContext> Sql() => Context.SqlContext.Sql();

        /// <summary>
        /// Creates a new Sql statement with arguments.
        /// </summary>
        protected Sql<ISqlContext> Sql(string sql, params object[] args) => Context.SqlContext.Sql(sql, args);

        /// <inheritdoc />
        public abstract void Migrate();

        /// <summary>
        /// Builds an Alter expression.
        /// </summary>
        public IAlterBuilder Alter => new AlterBuilder(Context);

        /// <summary>
        /// Builds a Create expression.
        /// </summary>
        public ICreateBuilder Create => new CreateBuilder(Context);

        /// <summary>
        /// Builds a Delete expression.
        /// </summary>
        public IDeleteBuilder Delete => new DeleteBuilder(Context);

        /// <summary>
        /// Builds an Execute expression.
        /// </summary>
        public IExecuteBuilder Execute => new ExecuteBuilder(Context);

        /// <summary>
        /// Builds an Insert expression.
        /// </summary>
        public IInsertBuilder Insert => new InsertBuilder(Context);

        /// <summary>
        /// Builds a Rename expression.
        /// </summary>
        public IRenameBuilder Rename => new RenameBuilder(Context);

        /// <summary>
        /// Builds an Update expression.
        /// </summary>
        public IUpdateBuilder Update => new UpdateBuilder(Context);
    }
}
