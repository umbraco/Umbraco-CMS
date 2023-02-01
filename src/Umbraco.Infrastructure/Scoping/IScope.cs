using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.EfCore;

namespace Umbraco.Cms.Infrastructure.Scoping;

public interface IScope : ICoreScope
{
    /// <summary>
    ///     Gets the scope database.
    /// </summary>
    IUmbracoDatabase Database { get; }

    IUmbracoEfCoreDatabase EfCoreDatabase { get; }

    /// <summary>
    ///     Gets the Sql context.
    /// </summary>
    ISqlContext SqlContext { get; }
}
