﻿using System;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Migrations
{
    public class MigrationBuilder : IMigrationBuilder
    {
        private readonly IFactory _container;

        public MigrationBuilder(IFactory container)
        {
            _container = container;
        }

        public IMigration Build(Type migrationType, IMigrationContext context)
        {
            return (IMigration) _container.CreateInstance(migrationType, context);
        }
    }
}
