﻿using System;
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
        private readonly Lazy<IMapperCollection> _mappers;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlContext"/> class.
        /// </summary>
        /// <param name="sqlSyntax">The sql syntax provider.</param>
        /// <param name="pocoDataFactory">The Poco data factory.</param>
        /// <param name="databaseType">The database type.</param>
        /// <param name="mappers">The mappers.</param>
        public SqlContext(ISqlSyntaxProvider sqlSyntax, DatabaseType databaseType, IPocoDataFactory pocoDataFactory, IMapperCollection mappers = null)
            : this(sqlSyntax, databaseType, pocoDataFactory, new Lazy<IMapperCollection>(() => mappers ?? new Mappers.MapperCollection(Enumerable.Empty<BaseMapper>())))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlContext"/> class.
        /// </summary>
        /// <param name="sqlSyntax">The sql syntax provider.</param>
        /// <param name="pocoDataFactory">The Poco data factory.</param>
        /// <param name="databaseType">The database type.</param>
        /// <param name="mappers">The mappers.</param>
        public SqlContext(ISqlSyntaxProvider sqlSyntax, DatabaseType databaseType, IPocoDataFactory pocoDataFactory, Lazy<IMapperCollection> mappers)
        {
            // for tests
            _mappers = mappers;

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
        public Sql<ISqlContext> Sql() => NPoco.Sql.BuilderFor((ISqlContext) this);

        /// <inheritdoc />
        public Sql<ISqlContext> Sql(string sql, params object[] args) => Sql().Append(sql, args);

        /// <inheritdoc />
        public IQuery<T> Query<T>() => new Query<T>(this);

        /// <inheritdoc />
        public SqlTemplates Templates { get; }

        /// <inheritdoc />
        public IPocoDataFactory PocoDataFactory { get; }

        /// <inheritdoc />
        public IMapperCollection Mappers => _mappers.Value;
    }
}
