﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

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
            return (IMigration) ActivatorUtilities.CreateInstance(_container, migrationType, context);
        }
    }
}
