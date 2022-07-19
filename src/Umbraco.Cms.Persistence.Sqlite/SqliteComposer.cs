using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Persistence.Sqlite;

/// <summary>
///     Automatically adds SQLite support to Umbraco when this project is referenced.
/// </summary>
public class SqliteComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
        => builder.AddUmbracoSqliteSupport();
}
