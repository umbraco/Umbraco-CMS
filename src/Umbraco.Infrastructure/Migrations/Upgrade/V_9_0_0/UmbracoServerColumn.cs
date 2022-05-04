using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;

public class UmbracoServerColumn : MigrationBase
{
    public UmbracoServerColumn(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    ///     Adds new columns to members table
    /// </summary>
    protected override void Migrate() => ReplaceColumn<ServerRegistrationDto>(
        Constants.DatabaseSchema.Tables.Server,
        "isMaster", "isSchedulingPublisher");
}
