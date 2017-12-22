using System;

namespace Umbraco.Core.Migrations
{
    public interface IMigrationBuilder
    {
        IMigration Build(Type migrationType, IMigrationContext context);
    }
}
