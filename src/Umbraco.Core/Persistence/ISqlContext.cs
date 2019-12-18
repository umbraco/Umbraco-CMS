using NPoco;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Specifies the Sql context.
    /// </summary>
    public interface ISqlContext
    {
        /// <summary>
        /// Gets the Sql syntax provider.
        /// </summary>
        ISqlSyntaxProvider SqlSyntax { get; }

        /// <summary>
        /// Gets the database type.
        /// </summary>
        DatabaseType DatabaseType { get; }

        /// <summary>
        /// Creates a new Sql expression.
        /// </summary>
        Sql<ISqlContext> Sql();

        /// <summary>
        /// Creates a new Sql expression.
        /// </summary>
        Sql<ISqlContext> Sql(string sql, params object[] args);

        /// <summary>
        /// Creates a new query expression.
        /// </summary>
        IQuery<T> Query<T>();

        /// <summary>
        /// Gets the Sql templates.
        /// </summary>
        SqlTemplates Templates { get; }

        /// <summary>
        /// Gets the Poco data factory.
        /// </summary>
        IPocoDataFactory PocoDataFactory { get; }

        /// <summary>
        /// Gets the mappers.
        /// </summary>
        IMapperCollection Mappers { get; }
    }
}
