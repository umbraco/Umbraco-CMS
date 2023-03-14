using NPoco;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Implements <see cref="ISqlContext" />.
/// </summary>
public class SqlContext : ISqlContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlContext" /> class.
    /// </summary>
    /// <param name="sqlSyntax">The sql syntax provider.</param>
    /// <param name="pocoDataFactory">The Poco data factory.</param>
    /// <param name="databaseType">The database type.</param>
    /// <param name="mappers">The mappers.</param>
    public SqlContext(ISqlSyntaxProvider sqlSyntax, DatabaseType databaseType, IPocoDataFactory pocoDataFactory, IMapperCollection? mappers = null)
    {
        // for tests
        Mappers = mappers;

        SqlSyntax = sqlSyntax ?? throw new ArgumentNullException(nameof(sqlSyntax));
        PocoDataFactory = pocoDataFactory ?? throw new ArgumentNullException(nameof(pocoDataFactory));
        DatabaseType = databaseType ?? throw new ArgumentNullException(nameof(databaseType));
        Templates = new SqlTemplates(this);
    }

    /// <inheritdoc />
    public ISqlSyntaxProvider SqlSyntax { get; }

    /// <inheritdoc />
    public DatabaseType DatabaseType { get; }

    /// <inheritdoc />
    public SqlTemplates Templates { get; }

    /// <inheritdoc />
    public IPocoDataFactory PocoDataFactory { get; }

    /// <inheritdoc />
    public IMapperCollection? Mappers { get; }

    /// <inheritdoc />
    public Sql<ISqlContext> Sql() => NPoco.Sql.BuilderFor((ISqlContext)this);

    /// <inheritdoc />
    public Sql<ISqlContext> Sql(string sql, params object[] args) => Sql().Append(sql, args);

    /// <inheritdoc />
    public IQuery<T> Query<T>() => new Query<T>(this);
}
