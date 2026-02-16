using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// Automatically adds SQLite EF Core support to Umbraco when this project is referenced.
/// </summary>
public class EFCoreSqliteComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
        => builder.AddUmbracoEFCoreSqliteSupport();
}
