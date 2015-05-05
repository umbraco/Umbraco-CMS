using System;
using System.Collections.Generic;
using Umbraco.Core.LightInject;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    /// <summary>
    /// A resolver to return all IMigrations
    /// </summary>
    internal class MigrationResolver : ContainerLazyManyObjectsResolver<MigrationResolver, IMigration>, IMigrationResolver
    {

        public MigrationResolver(IServiceContainer container, ILogger logger, Func<IEnumerable<Type>> migrations)
            : base(container, logger, migrations, ObjectLifetimeScope.Transient)
        {
        }

        /// <summary>
        /// Gets the migrations
        /// </summary>
        public IEnumerable<IMigration> Migrations
        {
            get { return Values; }
        }

    }
}