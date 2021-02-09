using System;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Migrations;

namespace Umbraco.Core.Migrations
{
    public class MigrationBuilder : IMigrationBuilder
    {
        private readonly IServiceProvider _container;

        public MigrationBuilder(IServiceProvider container)
        {
            _container = container;
        }

        public IMigration Build(Type migrationType, IMigrationContext context)
        {
            return (IMigration) _container.CreateInstance(migrationType, context);
        }
    }
}
