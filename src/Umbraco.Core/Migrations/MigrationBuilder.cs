using System;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Migrations
{
    public class MigrationBuilder : IMigrationBuilder
    {
        private readonly IContainer _container;

        public MigrationBuilder(IContainer container)
        {
            _container = container;
        }

        public IMigration Build(Type migrationType, IMigrationContext context)
        {
            _container.Register(migrationType);
            return (IMigration) _container.GetInstance(migrationType, context);
        }
    }
}
