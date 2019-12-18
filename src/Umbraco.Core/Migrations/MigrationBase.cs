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

        /// <summary>
        /// Executes the migration.
        /// </summary>
        public abstract void Migrate();

        /// <inheritdoc />
        void IMigration.Migrate()
        {
            Migrate();

            // ensure there is no building expression
            // ie we did not forget to .Do() an expression
            if (Context.BuildingExpression)
                throw new IncompleteMigrationExpressionException("The migration has run, but leaves an expression that has not run.");
        }

        // ensures we are not already building,
        // ie we did not forget to .Do() an expression
        private T BeginBuild<T>(T builder)
        {
            if (Context.BuildingExpression)
                throw new IncompleteMigrationExpressionException("Cannot create a new expression: the previous expression has not run.");
            Context.BuildingExpression = true;
            return builder;
        }

        /// <summary>
        /// Builds an Alter expression.
        /// </summary>
        public IAlterBuilder Alter => BeginBuild(new AlterBuilder(Context));

        /// <summary>
        /// Builds a Create expression.
        /// </summary>
        public ICreateBuilder Create => BeginBuild(new CreateBuilder(Context));

        /// <summary>
        /// Builds a Delete expression.
        /// </summary>
        public IDeleteBuilder Delete => BeginBuild(new DeleteBuilder(Context));

        /// <summary>
        /// Builds an Execute expression.
        /// </summary>
        public IExecuteBuilder Execute => BeginBuild(new ExecuteBuilder(Context));

        /// <summary>
        /// Builds an Insert expression.
        /// </summary>
        public IInsertBuilder Insert => BeginBuild(new InsertBuilder(Context));

        /// <summary>
        /// Builds a Rename expression.
        /// </summary>
        public IRenameBuilder Rename => BeginBuild(new RenameBuilder(Context));

        /// <summary>
        /// Builds an Update expression.
        /// </summary>
        public IUpdateBuilder Update => BeginBuild(new UpdateBuilder(Context));
    }
}
