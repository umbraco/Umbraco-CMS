using System;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations
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
