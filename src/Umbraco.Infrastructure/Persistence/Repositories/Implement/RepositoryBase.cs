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
}
