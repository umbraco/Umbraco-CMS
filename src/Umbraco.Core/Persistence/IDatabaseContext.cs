using NPoco;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    // the main implementation of IDatabaseContext is IUmbracoDatabaseFactory
    // it is registered as a singleton <IDatabaseContext> and can be injected
    //
    // then for convenience,
    //  fixme - IScopeProvider exposes .DatabaseContext
    //  fixme - IScope : IDatabaseContext
    //  IUnitOfWorkProvider exposes .DatabaseContext
    //  IScopeUnitOfWork : IDatabaseContext
    //  IMigrationContext : IDatabaseContext
    //   NPocoRepositoryBase : IDatabaseContext

    public interface IDatabaseContext
    {
        /// <summary>
        /// Gets the database Sql syntax.
        /// </summary>
        ISqlSyntaxProvider SqlSyntax { get; }

        /// <summary>
        /// Creates a new Sql expression.
        /// </summary>
        Sql<SqlContext> Sql();

        /// <summary>
        /// Creates a new Sql expression.
        /// </summary>
        Sql<SqlContext> Sql(string sql, params object[] args);

        /// <summary>
        /// Creates a new query expression.
        /// </summary>
        IQuery<T> Query<T>();
    }
}
