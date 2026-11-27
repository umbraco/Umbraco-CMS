using System.Data.Common;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Represents a <see cref="DbConnection" /> that decorates another connection, such as one that adds
///     retry or transaction behaviour, so that callers can reach the provider's own connection.
/// </summary>
public interface IWrappedDbConnection
{
    /// <summary>
    ///     Gets the decorated connection.
    /// </summary>
    DbConnection Inner { get; }
}
