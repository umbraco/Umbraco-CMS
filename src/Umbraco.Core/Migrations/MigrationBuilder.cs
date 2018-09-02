using System;
using System.Collections.Generic;
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
            return (IMigration) _container.CreateInstance(migrationType, new Dictionary<string, object>{{"context", context}});
        }
    }
}
