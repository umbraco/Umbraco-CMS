using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

/// <summary>
/// Prepares a connection that was opened outside EF Core, such as the Umbraco connection shared with a
/// scope, so a <see cref="DbContext" /> can use it as if EF Core had opened the connection itself.
/// </summary>
/// <remarks>
/// Every initializer whose <see cref="ProviderName" /> matches the Umbraco database provider is invoked each time
/// a scope attaches the shared connection to a context, so implementations should be cheap when called again
/// for a connection they have already prepared.
/// </remarks>
public interface IEFCoreSharedConnectionInitializer
{
    /// <summary>
    /// Gets the name of the database provider this initializer supports.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Prepares the shared connection for use by the context.
    /// </summary>
    /// <param name="dbContext">The context the connection has been attached to.</param>
    /// <param name="connection">The shared connection, as attached to the context.</param>
    void Initialize(DbContext dbContext, DbConnection connection);
}
