using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Locks every governed entity type except webhooks.
/// </summary>
public class LockEverythingButWebhooksConfigurator : ISchemaLockdownConfigurator
{
    /// <inheritdoc />
    public void Configure(ISchemaLockdownConfigurableRules rules)
    {
        foreach (var entityType in SchemaEntityTypes.All.Where(entityType => entityType != Constants.UdiEntityType.Webhook))
        {
            rules.BlockMutations(entityType);
        }
    }
}
