using System;
using Umbraco.Cms.Core.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations
{
    public interface IMigrationBuilder
    {
        IMigration Build(Type migrationType, IMigrationContext context);
    }
}
