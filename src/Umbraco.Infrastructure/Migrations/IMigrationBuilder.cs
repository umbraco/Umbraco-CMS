using System;
using Umbraco.Cms.Core.Migrations;

namespace Umbraco.Core.Migrations
{
    public interface IMigrationBuilder
    {
        IMigration Build(Type migrationType, IMigrationContext context);
    }
}
