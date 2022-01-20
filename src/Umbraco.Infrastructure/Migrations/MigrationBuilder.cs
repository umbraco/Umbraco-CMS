using System;
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

        public MigrationBase Build(Type migrationType, IMigrationContext context)
        {
            return (MigrationBase) _container.CreateInstance(migrationType, context);
        }
    }
}
