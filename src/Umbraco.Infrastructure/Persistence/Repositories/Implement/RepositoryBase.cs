using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Base repository class for all <see cref="IRepository" /> instances
/// </summary>
public abstract class RepositoryBase : IRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RepositoryBase" /> class.
    /// </summary>
    protected RepositoryBase(IScopeAccessor scopeAccessor, AppCaches appCaches)
    {
        ScopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));
        AppCaches = appCaches ?? throw new ArgumentNullException(nameof(appCaches));
    }

    /// <summary>
    ///     Gets the <see cref="AppCaches" />
    /// </summary>
    protected AppCaches AppCaches { get; }

    /// <summary>
    ///     Gets the <see cref="IScopeAccessor" />
    /// </summary>
    protected IScopeAccessor ScopeAccessor { get; }

    /// <summary>
    ///     Gets the AmbientScope
    /// </summary>
    protected IScope AmbientScope
    {
        get
        {
            IScope? scope = ScopeAccessor.AmbientScope;
            if (scope == null)
            {
                throw new InvalidOperationException("Cannot run a repository without an ambient scope.");
            }

            return scope;
        }
    }

    /// <summary>
    ///     Gets the repository's database.
    /// </summary>
    protected IUmbracoDatabase Database => AmbientScope.Database;

    /// <summary>
    ///     Gets the Sql context.
    /// </summary>
    protected ISqlContext SqlContext => AmbientScope.SqlContext;

    /// <summary>
    ///     Gets the <see cref="ISqlSyntaxProvider" />
    /// </summary>
    protected ISqlSyntaxProvider SqlSyntax => SqlContext.SqlSyntax;

    /// <summary>
    ///     Creates an<see cref="Sql{ISqlContext}" /> expression
    /// </summary>
    protected Sql<ISqlContext> Sql() => SqlContext.Sql();

    /// <summary>
    ///     Creates a <see cref="Sql{ISqlContext}" /> expression
    /// </summary>
    protected Sql<ISqlContext> Sql(string sql, params object[] args) => SqlContext.Sql(sql, args);

    /// <summary>
    ///     Creates a new query expression
    /// </summary>
    protected IQuery<T> Query<T>() => SqlContext.Query<T>();


    /// <summary>
    /// Quotes a table name according to the registered SQL syntax provider.
    /// </summary>
    protected string QuoteTableName(string? tableName) => SqlSyntax.GetQuotedTableName(tableName);

    /// <summary>
    /// Quotes a column name according to the registered SQL syntax provider.
    /// </summary>
    protected string QuoteColumnName(string? columnName) => SqlSyntax.GetQuotedColumnName(columnName);

    /// <summary>
    /// Returns the column name, qualified by the specified table name, with appropriate quoting for use in SQL
    /// statements.
    /// </summary>
    /// <param name="tableName">The name of the table to qualify the column with. Cannot be null or empty.</param>
    /// <param name="columnName">The name of the column to quote. Cannot be null or empty.</param>
    /// <returns>A string containing the quoted and qualified column name suitable for use in SQL queries.</returns>
    protected string QuoteColumnName(string tableName, string columnName) => SqlSyntax.GetQuotedColumn(tableName, columnName);

    /// <summary>
    /// Quotes a column name according to the registered SQL syntax provider.
    /// </summary>
    protected string QuoteName(string? name) => SqlSyntax.GetQuotedName(name);
}
