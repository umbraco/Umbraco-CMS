using System;
using System.Linq;
using NPoco;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Implements <see cref="ISqlContext"/>.
    /// </summary>
    public class SqlContext : ISqlContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlContext"/> class.
        /// </summary>
        /// <param name="sqlSyntax">The sql syntax provider.</param>
        /// <param name="pocoDataFactory">The Poco data factory.</param>
        /// <param name="databaseType">The database type.</param>
        /// <param name="mappers">The mappers.</param>
        public SqlContext(ISqlSyntaxProvider sqlSyntax, DatabaseType databaseType, IPocoDataFactory pocoDataFactory, IMapperCollection mappers = null)
        {
            // for tests
            Mappers = mappers ?? new Mappers.MapperCollection(Enumerable.Empty<BaseMapper>());

            SqlSyntax = sqlSyntax ?? throw new ArgumentNullException(nameof(sqlSyntax));
            PocoDataFactory = pocoDataFactory ?? throw new ArgumentNullException(nameof(pocoDataFactory));
            DatabaseType = databaseType ?? throw new ArgumentNullException(nameof(databaseType));
            Templates = new SqlTemplates(this);
        }

        // fixme
        internal SqlContext()
        { }

        internal void Initialize(ISqlSyntaxProvider sqlSyntax, DatabaseType databaseType, IPocoDataFactory pocoDataFactory, IMapperCollection mappers = null)
        {
            // for tests
            Mappers = mappers ?? new Mappers.MapperCollection(Enumerable.Empty<BaseMapper>());

            SqlSyntax = sqlSyntax ?? throw new ArgumentNullException(nameof(sqlSyntax));
            PocoDataFactory = pocoDataFactory ?? throw new ArgumentNullException(nameof(pocoDataFactory));
            DatabaseType = databaseType ?? throw new ArgumentNullException(nameof(databaseType));
            Templates = new SqlTemplates(this);
        }

        /// <inheritdoc />
        public ISqlSyntaxProvider SqlSyntax { get; private set; }

        /// <inheritdoc />
        public DatabaseType DatabaseType { get; private set; }

        /// <inheritdoc />
        public Sql<ISqlContext> Sql() => NPoco.Sql.BuilderFor((ISqlContext) this);

        /// <inheritdoc />
        public Sql<ISqlContext> Sql(string sql, params object[] args) => Sql().Append(sql, args);

        /// <inheritdoc />
        public IQuery<T> Query<T>() => new Query<T>(this);

        /// <inheritdoc />
        public SqlTemplates Templates { get; private set; }

        /// <inheritdoc />
        public IPocoDataFactory PocoDataFactory { get; private set; }

        /// <inheritdoc />
        public IMapperCollection Mappers { get; private set; }
    }
}
