using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Scoping;

/// <summary>
/// Represents a scope that manages the unit of work and transactional boundaries within the application.
/// Typically used to ensure consistency and manage resources during operations that require transactional integrity.
/// </summary>
public interface IScope : ICoreScope
{
    /// <summary>
    ///     Gets the scope database.
    /// </summary>
    IUmbracoDatabase Database { get; }

    /// <summary>
    ///     Gets the Sql context.
    /// </summary>
    ISqlContext SqlContext { get; }
}
